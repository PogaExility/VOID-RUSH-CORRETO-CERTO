using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    [Header("Modo da Sala")]
    [SerializeField] private bool showEntireRoom = false;

    [Header("Configurações de Câmera")]
    [Tooltip("Zoom padrão ao seguir o player.")]
    [SerializeField] private float targetOrthographicSize = 9f;
    [Tooltip("Margem para o modo Sala Inteira.")]
    [SerializeField] private float roomPadding = 1.1f;

    [Header("Configurações de Transição (Turbo)")]
    [Tooltip("Duração total. 0.25s é muito rápido.")]
    [SerializeField] private float transitionDuration = 0.25f; // MUITO RÁPIDO

    [Tooltip("Pico do zoom out. Manter próximo do zoom normal deixa mais ágil.")]
    [SerializeField] private float maxTransitionZoom = 10.5f; // MOVIMENTO CURTO

    [SerializeField] private float transitionCooldown = 0.05f; // QUASE INSTANTÂNEO

    private Collider2D roomCollider;
    private static Coroutine activeTransitionCoroutine;

    // --- LÓGICA DE TRANSFERÊNCIA ---
    private static RoomBoundary currentActiveRoom = null;
    private static RoomBoundary nextPotentialRoom = null;

    private static Transform _proxyTarget;
    private static Transform ProxyTarget
    {
        get
        {
            if (_proxyTarget == null)
            {
                GameObject proxyGO = new GameObject("CameraProxyTarget");
                _proxyTarget = proxyGO.transform;
            }
            return _proxyTarget;
        }
    }

    void Awake()
    {
        roomCollider = GetComponent<Collider2D>();
        if (!roomCollider.isTrigger) Debug.LogWarning("RoomBoundary precisa de um Collider Trigger.", gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentActiveRoom == null)
            {
                ActivateRoom(other);
            }
            else if (this != currentActiveRoom)
            {
                nextPotentialRoom = this;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (this == currentActiveRoom)
            {
                if (nextPotentialRoom != null)
                {
                    nextPotentialRoom.ActivateRoom(other);
                }
            }
            else if (this == nextPotentialRoom)
            {
                nextPotentialRoom = null;
            }
        }
    }

    public void ActivateRoom(Collider2D playerCollider)
    {
        currentActiveRoom = this;
        nextPotentialRoom = null;

        CinemachineCamera virtualCamera = FindAnyObjectByType<CinemachineCamera>();
        if (virtualCamera == null) return;

        if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

        activeTransitionCoroutine = StartCoroutine(StagedTransition(virtualCamera, playerCollider.transform));
    }

    private IEnumerator StagedTransition(CinemachineCamera cam, Transform playerTransform)
    {
        float startSize = cam.Lens.OrthographicSize;
        float halfDuration = transitionDuration / 2f;
        float elapsedTime = 0f;

        // FASE 1: PULSO PARA TRÁS (Zoom Out Rápido)
        while (elapsedTime < halfDuration)
        {
            // Usando SmoothStep para um movimento mais orgânico mesmo sendo rápido
            float progress = Mathf.SmoothStep(0, 1, elapsedTime / halfDuration);
            cam.Lens.OrthographicSize = Mathf.Lerp(startSize, maxTransitionZoom, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // --- TROCA INSTANTÂNEA ---
        UpdateConfiner();

        float finalTargetSize = showEntireRoom ? CalculateOrthographicSize() : targetOrthographicSize;
        Vector3 proxyStartPos = playerTransform.position;

        if (showEntireRoom)
        {
            cam.Follow = ProxyTarget;
            ProxyTarget.position = proxyStartPos;
        }
        else
        {
            cam.Follow = playerTransform;
        }

        // FASE 2: ENCAIXE RÁPIDO (Zoom In)
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float progress = Mathf.SmoothStep(0, 1, elapsedTime / halfDuration);
            cam.Lens.OrthographicSize = Mathf.Lerp(maxTransitionZoom, finalTargetSize, progress);

            if (showEntireRoom)
            {
                ProxyTarget.position = Vector3.Lerp(proxyStartPos, roomCollider.bounds.center, progress);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.Lens.OrthographicSize = finalTargetSize;
        if (showEntireRoom) ProxyTarget.position = roomCollider.bounds.center;

        activeTransitionCoroutine = null;
    }

    private void UpdateConfiner()
    {
        CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();
        if (confiner != null)
        {
            confiner.BoundingShape2D = roomCollider;
            confiner.InvalidateBoundingShapeCache();
        }
    }

    private float CalculateOrthographicSize()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float roomAspect = roomCollider.bounds.size.x / roomCollider.bounds.size.y;
        float size = (roomAspect > screenAspect)
            ? (roomCollider.bounds.size.x / screenAspect) * 0.5f
            : roomCollider.bounds.size.y * 0.5f;
        return size * roomPadding;
    }
}