using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class CameraOverrideZone : MonoBehaviour
{
    [Header("Configurações da Zona")]
    [SerializeField] private float targetOrthographicSize = 5f;
    [SerializeField] private float transitionDuration = 0.5f;

    private Collider2D zoneCollider;
    private Coroutine activeTransitionCoroutine;

    // Cache para evitar FindObject pesado durante o jogo
    private static CinemachineCamera cachedCamera;
    private static CinemachineConfiner2D cachedConfiner;

    void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
        if (!zoneCollider.isTrigger) Debug.LogWarning("CameraOverrideZone requer Is Trigger.", gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Inicializa cache se necessário
            if (cachedCamera == null) cachedCamera = FindAnyObjectByType<CinemachineCamera>();
            if (cachedConfiner == null) cachedConfiner = FindAnyObjectByType<CinemachineConfiner2D>();

            if (cachedCamera == null || cachedConfiner == null) return;

            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

            activeTransitionCoroutine = StartCoroutine(TransitionSequence(cachedCamera, cachedConfiner));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

            // Procura a sala "mãe" onde o player está agora
            RoomBoundary parentRoom = FindParentRoomFor(other.transform);

            if (parentRoom != null)
            {
                // Devolve o controle para a sala grande
                parentRoom.ActivateRoom(other.GetComponent<Collider2D>());
            }
        }
    }

    private RoomBoundary FindParentRoomFor(Transform playerTransform)
    {
        // Nota: FindObjectsByType ainda é pesado, mas no Exit é menos crítico que no Enter.
        // O ideal seria ter uma lista global gerenciada por um Manager, mas manteremos assim por enquanto.
        RoomBoundary[] allRooms = FindObjectsByType<RoomBoundary>(FindObjectsSortMode.None);
        foreach (var room in allRooms)
        {
            Collider2D roomCollider = room.GetComponent<Collider2D>();
            // Verifica qual sala contém o player
            if (roomCollider.bounds.Contains(playerTransform.position))
            {
                return room;
            }
        }
        return null;
    }

    private IEnumerator TransitionSequence(CinemachineCamera cam, CinemachineConfiner2D confiner)
    {
        float startSize = cam.Lens.OrthographicSize;
        float elapsedTime = 0f;

        // 1. DESATIVA O CONFINER (Para não puxar a câmera violentamente)
        confiner.enabled = false;

        // 2. TROCA O SHAPE
        confiner.BoundingShape2D = zoneCollider;
        confiner.InvalidateBoundingShapeCache();

        // 3. FAZ O ZOOM E A TRANSIÇÃO
        while (elapsedTime < transitionDuration)
        {
            float progress = elapsedTime / transitionDuration;
            // Usando SmoothStep para ficar mais suave
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            cam.Lens.OrthographicSize = Mathf.Lerp(startSize, targetOrthographicSize, smoothProgress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.Lens.OrthographicSize = targetOrthographicSize;

        // 4. REATIVA O CONFINER (Agora que o zoom já ajustou)
        confiner.enabled = true;

        activeTransitionCoroutine = null;
    }
}