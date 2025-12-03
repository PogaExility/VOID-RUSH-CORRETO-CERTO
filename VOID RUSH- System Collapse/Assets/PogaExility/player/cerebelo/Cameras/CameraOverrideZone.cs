using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class CameraOverrideZone : MonoBehaviour
{
    [Header("Configurações da Zona")]
    [Tooltip("Tamanho do Zoom desejado nesta zona.")]
    [SerializeField] private float targetOrthographicSize = 5f;

    [Tooltip("Pico do Zoom Out na transição. Será cortado se a zona for pequena.")]
    [SerializeField] private float maxTransitionZoom = 10.5f;

    [Tooltip("Duração total da transição.")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Tooltip("Teto MÁXIMO absoluto. O zoom nunca passará deste valor, mesmo que a zona seja enorme.")]
    [SerializeField] private float absoluteMaxZoom = 14f;

    private Collider2D zoneCollider;
    private Coroutine activeTransitionCoroutine;

    // Cache estático para performance
    private static CinemachineCamera cachedCamera;
    private static CinemachineConfiner2D cachedConfiner;

    void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
        if (!zoneCollider.isTrigger)
            Debug.LogWarning($"CameraOverrideZone em '{gameObject.name}' requer 'Is Trigger' marcado.", gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Inicializa cache
            if (cachedCamera == null) cachedCamera = FindAnyObjectByType<CinemachineCamera>();
            if (cachedConfiner == null) cachedConfiner = FindAnyObjectByType<CinemachineConfiner2D>();

            if (cachedCamera == null || cachedConfiner == null) return;

            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

            activeTransitionCoroutine = StartCoroutine(PulseTransition(cachedCamera, cachedConfiner));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

            // Ao sair da zona, precisamos avisar a "Sala Mãe" para retomar o controle
            RoomBoundary parentRoom = FindParentRoomFor(other.transform);

            if (parentRoom != null)
            {
                // A sala mãe vai recalcular o zoom e o confiner dela
                parentRoom.ActivateRoom(other.GetComponent<Collider2D>());
            }
        }
    }

    // Busca qual RoomBoundary contém o player neste momento
    private RoomBoundary FindParentRoomFor(Transform playerTransform)
    {
        RoomBoundary[] allRooms = FindObjectsByType<RoomBoundary>(FindObjectsSortMode.None);
        foreach (var room in allRooms)
        {
            Collider2D roomCol = room.GetComponent<Collider2D>();
            // Verifica se o player está dentro dos limites desta sala
            if (roomCol != null && roomCol.bounds.Contains(playerTransform.position))
            {
                return room;
            }
        }
        return null;
    }

    private IEnumerator PulseTransition(CinemachineCamera cam, CinemachineConfiner2D confiner)
    {
        float startSize = cam.Lens.OrthographicSize;
        float halfDuration = transitionDuration / 2f;
        float elapsedTime = 0f;

        // --- 1. MATEMÁTICA BLINDADA (Igual ao RoomBoundary) ---

        // Pega Aspect Ratio seguro
        float aspect = cam.Lens.Aspect;
        if (aspect < 0.01f) aspect = (float)Screen.width / Screen.height;

        // Calcula limites físicos (Metade do tamanho)
        Bounds b = zoneCollider.bounds;
        float zoneHalfHeight = b.extents.y;
        float zoneHalfWidthAsHeight = b.extents.x / aspect;

        // Limite Físico Absoluto (Menor dimensão)
        float physicalLimit = Mathf.Min(zoneHalfHeight, zoneHalfWidthAsHeight);

        // Limite Utilizável:
        // É o menor valor entre: (Limite Físico com 1% de folga) E (Teto Absoluto configurado)
        float usableMaxZoom = Mathf.Min(physicalLimit * 0.99f, absoluteMaxZoom);

        // --- 2. DEFINIÇÃO DE ALVOS ---

        // Alvo Final: O que você quer (5), mas nunca maior que o Usável
        float safeTarget = Mathf.Min(targetOrthographicSize, usableMaxZoom);

        // Pico da Transição: Tenta ir ao máximo (10.5), mas nunca maior que o Usável
        float safePeak = Mathf.Min(maxTransitionZoom, usableMaxZoom);

        // Correção de Entrada: Se a câmera atual já é maior que a zona permite,
        // o pico TEM que ser o limite da zona para forçar encolhimento imediato.
        if (startSize > usableMaxZoom)
        {
            safePeak = usableMaxZoom;
        }

        // --- FASE 1: TRANSIÇÃO PARA O PICO ---
        while (elapsedTime < halfDuration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsedTime / halfDuration);
            cam.Lens.OrthographicSize = Mathf.Lerp(startSize, safePeak, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cam.Lens.OrthographicSize = safePeak;

        // --- TROCA O CONFINER ---
        confiner.BoundingShape2D = zoneCollider;
        confiner.InvalidateBoundingShapeCache();

        // Reseta amortecimento para evitar drift
        cam.PreviousStateIsValid = false;

        yield return null; // Frame de respiro

        // --- FASE 2: AJUSTE FINAL ---
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsedTime / halfDuration);
            cam.Lens.OrthographicSize = Mathf.Lerp(safePeak, safeTarget, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.Lens.OrthographicSize = safeTarget;
        activeTransitionCoroutine = null;
    }
}