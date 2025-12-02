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

        // 1. MATEMÁTICA: CALCULA O LIMITE FÍSICO DA ZONA
        // Descobre o tamanho máximo que a câmera pode ter aqui dentro
        float aspect = cam.Lens.Aspect > 0 ? cam.Lens.Aspect : 1.77f;
        Bounds b = zoneCollider.bounds;
        float heightLimit = b.size.y * 0.5f;
        float widthLimit = (b.size.x / aspect) * 0.5f;

        // O limite absoluto é a menor dimensão (para não vazar nem em cima nem dos lados)
        float absoluteLimit = Mathf.Min(heightLimit, widthLimit);

        // 2. DEFINE ALVOS SEGUROS
        // O pico do zoom nunca pode ser maior que o limite da zona
        float safePeak = Mathf.Min(maxTransitionZoom, absoluteLimit);

        // O alvo final também não (com 1% de margem de segurança para não ver o "vazio")
        float safeTarget = Mathf.Min(targetOrthographicSize, absoluteLimit * 0.99f);

        // Se a câmera atual já for maior que a zona (ex: entrando de uma sala grande para um túnel),
        // o pico deve ser imediatamente o limite da zona para evitar glitch visual.
        if (startSize > absoluteLimit) safePeak = absoluteLimit;

        // FASE 1: TRANSIÇÃO ATÉ O PICO
        while (elapsedTime < halfDuration)
        {
            float progress = Mathf.SmoothStep(0, 1, elapsedTime / halfDuration);
            cam.Lens.OrthographicSize = Mathf.Lerp(startSize, safePeak, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cam.Lens.OrthographicSize = safePeak;

        // --- TROCA O CONFINER ---
        confiner.BoundingShape2D = zoneCollider;
        confiner.InvalidateBoundingShapeCache();

        // Espera um frame para o Cinemachine processar a troca
        yield return null;

        // FASE 2: AJUSTE FINAL (Zoom para o tamanho alvo seguro)
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float progress = Mathf.SmoothStep(0, 1, elapsedTime / halfDuration);
            cam.Lens.OrthographicSize = Mathf.Lerp(safePeak, safeTarget, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.Lens.OrthographicSize = safeTarget;
        activeTransitionCoroutine = null;
    }
}