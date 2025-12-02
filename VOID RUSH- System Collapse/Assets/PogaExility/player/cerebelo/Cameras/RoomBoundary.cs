using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Se true, trava o zoom no tamanho máximo da sala. Se false, usa o Target Size (respeitando o limite).")]
    [SerializeField] private bool showEntireRoom = false;

    [Tooltip("Tamanho de Zoom ideal. Será ignorado se a sala for menor que isso.")]
    [SerializeField] private float targetOrthographicSize = 9f;

    [Tooltip("Margem de respiro. 0.9 = 90% do tamanho da sala (10% de folga para mover).")]
    [Range(0.5f, 0.99f)]
    [SerializeField] private float roomPadding = 0.95f; // Reduzi levemente para garantir movimento

    [Tooltip("Tempo da transição.")]
    [SerializeField] private float transitionDuration = 0.25f;

    [Tooltip("Tentativa de Zoom Out. Será cortado matematicamente se a sala for apertada.")]
    [SerializeField] private float maxTransitionZoom = 10.5f;

    private Collider2D roomCollider;
    private static Coroutine activeRoutine;

    // Cache
    private static CinemachineCamera cachedCam;
    private static CinemachineConfiner2D cachedConfiner;

    // Lógica de Estado
    private static RoomBoundary currentRoom = null;
    private static RoomBoundary nextRoom = null;

    void Awake()
    {
        roomCollider = GetComponent<Collider2D>();
        if (!roomCollider.isTrigger)
            Debug.LogError($"[RoomBoundary] O Collider de {name} precisa ser Trigger!", gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (currentRoom == null) ActivateRoom(other);
        else if (this != currentRoom) nextRoom = this;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (this == currentRoom && nextRoom != null)
            nextRoom.ActivateRoom(other);
        else if (this == nextRoom)
            nextRoom = null;
    }

    public void ActivateRoom(Collider2D player)
    {
        currentRoom = this;
        nextRoom = null;

        if (cachedCam == null) cachedCam = FindAnyObjectByType<CinemachineCamera>();
        if (cachedConfiner == null) cachedConfiner = FindAnyObjectByType<CinemachineConfiner2D>();

        if (cachedCam == null || cachedConfiner == null) return;

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(TransitionRoutine(player.transform));
    }

    private IEnumerator TransitionRoutine(Transform playerTarget)
    {
        float startZoom = cachedCam.Lens.OrthographicSize;
        float timer = 0f;
        float halfTime = transitionDuration * 0.5f;

        // 1. CÁLCULO RIGOROSO DOS LIMITES
        // Pega as extensões (metade da largura/altura) do colisor da sala
        Bounds b = roomCollider.bounds;
        float roomHalfHeight = b.extents.y;

        // Calcula o Aspect Ratio (evita erros se for 0)
        float aspect = cachedCam.Lens.Aspect;
        if (aspect < 0.01f) aspect = (float)Screen.width / Screen.height;

        // Calcula a altura máxima equivalente baseada na largura da sala
        float roomHalfWidthAsHeight = b.extents.x / aspect;

        // O Limite Físico é o menor valor. Se a câmera passar disso, ela sai da sala.
        float physicalLimit = Mathf.Min(roomHalfHeight, roomHalfWidthAsHeight);

        // 2. APLICAÇÃO DA FOLGA (CRUCIAL PARA O MOVIMENTO)
        // Multiplicamos pelo padding (ex: 0.95). Isso garante que a câmera seja 
        // 5% menor que a sala, permitindo que ela se mova para seguir o player.
        float usableMaxZoom = physicalLimit * roomPadding;

        // 3. DEFINIÇÃO DOS ALVOS
        // Se showEntireRoom for true, usamos o máximo permitido. 
        // Se for false, usamos o target desejado, mas CEIFADO pelo máximo permitido.
        float finalTargetZoom = showEntireRoom ? usableMaxZoom : Mathf.Min(targetOrthographicSize, usableMaxZoom);

        // Define o pico da transição. Ele tenta ir até o maxTransitionZoom, 
        // mas nunca pode exceder o usableMaxZoom desta sala.
        float peakZoom = Mathf.Min(maxTransitionZoom, usableMaxZoom);

        // LÓGICA DE SEGURANÇA:
        // Se a câmera atual (startZoom) for maior que o que a nova sala aguenta (usableMaxZoom),
        // o "Pico" deve ser o usableMaxZoom. Isso força a câmera a encolher na Fase 1 
        // antes de ligarmos o Confiner, evitando que ela trave no centro.
        if (startZoom > usableMaxZoom)
        {
            peakZoom = usableMaxZoom;
        }

        // ====================================================================
        // FASE 1: AJUSTE INICIAL (Zoom para o Pico)
        // ====================================================================
        while (timer < halfTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / halfTime);
            cachedCam.Lens.OrthographicSize = Mathf.Lerp(startZoom, peakZoom, t);
            timer += Time.deltaTime;
            yield return null;
        }
        cachedCam.Lens.OrthographicSize = peakZoom;

        // ====================================================================
        // FASE 2: TROCA DE CONTEXTO
        // ====================================================================
        // Agora que o zoom está seguro (menor que a sala), trocamos o Confiner.
        cachedConfiner.BoundingShape2D = roomCollider;
        cachedConfiner.InvalidateBoundingShapeCache();

        // Define o player como alvo imediatamente
        cachedCam.Follow = playerTarget;

        // Força o Cinemachine a ignorar o amortecimento anterior e pular para o novo cálculo
        // Isso ajuda a destravar do centro se houve uma troca brusca
        cachedCam.PreviousStateIsValid = false;

        yield return null; // Espera um frame para a física processar

        // ====================================================================
        // FASE 3: AJUSTE FINAL (Zoom para o Alvo)
        // ====================================================================
        timer = 0f;
        while (timer < halfTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / halfTime);
            cachedCam.Lens.OrthographicSize = Mathf.Lerp(peakZoom, finalTargetZoom, t);
            timer += Time.deltaTime;
            yield return null;
        }
        cachedCam.Lens.OrthographicSize = finalTargetZoom;

        activeRoutine = null;
    }
}