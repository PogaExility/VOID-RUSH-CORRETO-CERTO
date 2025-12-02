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
    [Tooltip("Duração total da transição.")]
    [SerializeField] private float transitionDuration = 0.25f;

    [Tooltip("Pico do zoom out. Deve ser maior que o Zoom padrão.")]
    [SerializeField] private float maxTransitionZoom = 10.5f;

    // Referências internas
    private Collider2D roomCollider;
    private static Coroutine activeTransitionCoroutine;

    // --- LÓGICA DE GERENCIAMENTO DE SALAS ---
    private static RoomBoundary currentActiveRoom = null;
    private static RoomBoundary nextPotentialRoom = null;

    // --- CACHE DE COMPONENTES DO CINEMACHINE ---
    // Static para que todas as salas compartilhem as mesmas referências da câmera principal
    private static CinemachineCamera cachedCamera;
    private static CinemachineConfiner2D cachedConfiner;

    // Proxy para o modo "Sala Inteira" (Centraliza a câmera)
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
        if (!roomCollider.isTrigger)
            Debug.LogWarning($"RoomBoundary em '{gameObject.name}' precisa de um Collider Trigger.", gameObject);
    }

    void Start()
    {
        // Tenta encontrar a câmera e o confiner no início do jogo para evitar lag depois
        if (cachedCamera == null) cachedCamera = FindAnyObjectByType<CinemachineCamera>();
        if (cachedConfiner == null) cachedConfiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Se não há sala ativa, esta vira a ativa
            if (currentActiveRoom == null)
            {
                ActivateRoom(other);
            }
            // Se já tem sala ativa, esta é a próxima em potencial
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
            // Se saiu da sala atual...
            if (this == currentActiveRoom)
            {
                // ...e tem uma próxima engatilhada, ativa a próxima
                if (nextPotentialRoom != null)
                {
                    nextPotentialRoom.ActivateRoom(other);
                }
            }
            // Se saiu da sala potencial, cancela a previsão
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

        // Segurança: Garante as referências caso o Start não tenha pego (ex: Player spawnou depois)
        if (cachedCamera == null) cachedCamera = FindAnyObjectByType<CinemachineCamera>();
        if (cachedConfiner == null) cachedConfiner = FindAnyObjectByType<CinemachineConfiner2D>();

        if (cachedCamera == null) return;

        // Interrompe transição anterior se houver
        if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

        // Inicia a nova transição
        activeTransitionCoroutine = StartCoroutine(StagedTransition(cachedCamera, playerCollider.transform));
    }

    private IEnumerator StagedTransition(CinemachineCamera cam, Transform playerTransform)
    {
        float startSize = cam.Lens.OrthographicSize;
        float halfDuration = transitionDuration / 2f;
        float elapsedTime = 0f;

        // =================================================================
        // FASE 1: ZOOM OUT
        // =================================================================
        // Abre a câmera para dar "respiro" na troca de colisor
        while (elapsedTime < halfDuration)
        {
            float progress = Mathf.SmoothStep(0, 1, elapsedTime / halfDuration);
            cam.Lens.OrthographicSize = Mathf.Lerp(startSize, maxTransitionZoom, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cam.Lens.OrthographicSize = maxTransitionZoom;

        // =================================================================
        // TROCA DE CONTEXTO (NO PICO DO ZOOM)
        // =================================================================

        // 1. Aplica o novo formato da sala ao Confiner
        if (cachedConfiner != null)
        {
            cachedConfiner.BoundingShape2D = roomCollider;
            cachedConfiner.InvalidateBoundingShapeCache(); // Importante: Força o recálculo imediato
        }

        // 2. Define o alvo da câmera
        float finalTargetSize = showEntireRoom ? CalculateOrthographicSize() : targetOrthographicSize;
        Vector3 proxyStartPos = playerTransform.position;

        if (showEntireRoom)
        {
            cam.Follow = ProxyTarget;
            // Começa o proxy na posição do player para suavizar o movimento até o centro
            ProxyTarget.position = proxyStartPos;
        }
        else
        {
            cam.Follow = playerTransform;
        }

        // Aguarda um frame para o Cinemachine processar a mudança de shape
        yield return null;

        // =================================================================
        // FASE 2: ZOOM IN
        // =================================================================
        // Enquanto fecha o zoom, o Confiner vai ajustar a câmera aos novos limites
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float progress = Mathf.SmoothStep(0, 1, elapsedTime / halfDuration);

            // Zoom In
            cam.Lens.OrthographicSize = Mathf.Lerp(maxTransitionZoom, finalTargetSize, progress);

            // Se for sala inteira, move o foco para o centro
            if (showEntireRoom)
            {
                ProxyTarget.position = Vector3.Lerp(proxyStartPos, roomCollider.bounds.center, progress);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalização exata
        cam.Lens.OrthographicSize = finalTargetSize;
        if (showEntireRoom) ProxyTarget.position = roomCollider.bounds.center;

        activeTransitionCoroutine = null;
    }

    private float CalculateOrthographicSize()
    {
        // Calcula o tamanho necessário para mostrar a sala inteira baseada na proporção da tela
        float screenAspect = (float)Screen.width / Screen.height;
        float roomAspect = roomCollider.bounds.size.x / roomCollider.bounds.size.y;

        float size;
        if (roomAspect > screenAspect)
        {
            // Sala mais larga que a tela: ajusta pela largura
            size = (roomCollider.bounds.size.x / screenAspect) * 0.5f;
        }
        else
        {
            // Sala mais alta que a tela: ajusta pela altura
            size = roomCollider.bounds.size.y * 0.5f;
        }
        return size * roomPadding;
    }
}