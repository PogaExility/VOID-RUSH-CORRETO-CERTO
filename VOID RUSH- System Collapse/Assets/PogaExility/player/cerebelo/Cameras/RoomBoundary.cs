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

    [Tooltip("Teto MÁXIMO absoluto. Mesmo que a sala seja gigante, o zoom nunca passará deste valor.")]
    [SerializeField] private float absoluteMaxZoom = 14f; // Adicione isso

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

    // Agora aceita um parâmetro opcional 'instant' (padrão false)
    public void ActivateRoom(Collider2D player, bool instant = false)
    {
        currentRoom = this;
        nextRoom = null;

        if (cachedCam == null) cachedCam = FindAnyObjectByType<CinemachineCamera>();
        if (cachedConfiner == null) cachedConfiner = FindAnyObjectByType<CinemachineConfiner2D>();

        if (cachedCam == null || cachedConfiner == null) return;

        if (activeRoutine != null) StopCoroutine(activeRoutine);

        if (instant)
        {
            // Se for instantâneo (respawn), aplica configurações sem animação
            ApplyImmediate(player.transform);
        }
        else
        {
            // Se for normal (andando), faz a transição suave
            activeRoutine = StartCoroutine(TransitionRoutine(player.transform));
        }
    }

    // NOVA FUNÇÃO: Aplica tudo num único frame (Corte Seco)
    // Atualizada para teleportar a câmera fisicamente
    private void ApplyImmediate(Transform target)
    {
        // 1. Calcula o zoom seguro
        float targetZoom = CalculateSafeZoom();

        // 2. Teleporta a câmera FISICAMENTE para o player (mantendo o Z padrão)
        // Isso é crucial! Se a câmera estiver longe (onde o player morreu), 
        // o Confiner acha que está "fora do mapa" e para de funcionar.
        Vector3 newPos = target.position;
        newPos.z = cachedCam.transform.position.z;
        cachedCam.transform.position = newPos;

        // 3. Aplica configurações
        cachedCam.Lens.OrthographicSize = targetZoom;

        cachedConfiner.BoundingShape2D = roomCollider;
        cachedConfiner.InvalidateBoundingShapeCache();

        cachedCam.Follow = target;

        // 4. Reseta o amortecimento para evitar "drift" visual
        cachedCam.PreviousStateIsValid = false;
    }

    private IEnumerator TransitionRoutine(Transform playerTarget)
    {
        float startZoom = cachedCam.Lens.OrthographicSize;
        float timer = 0f;
        float halfTime = transitionDuration * 0.5f;

        // --- CÁLCULO MATEMÁTICO ---

        // Limite físico da sala (com padding)
        float roomLimit = CalculateSafeZoom();

        // Define o Pico: Tenta ir até o maxTransitionZoom, mas para no teto da sala ou no teto absoluto
        float peakZoom = Mathf.Min(maxTransitionZoom, roomLimit);

        // Se a câmera atual é maior que a sala nova, o pico é o limite da sala (redução forçada)
        if (startZoom > roomLimit) peakZoom = roomLimit;

        // FASE 1: Zoom para o Pico
        while (timer < halfTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / halfTime);
            cachedCam.Lens.OrthographicSize = Mathf.Lerp(startZoom, peakZoom, t);
            timer += Time.deltaTime;
            yield return null;
        }
        cachedCam.Lens.OrthographicSize = peakZoom;

        // FASE 2: Troca de Contexto
        cachedConfiner.BoundingShape2D = roomCollider;
        cachedConfiner.InvalidateBoundingShapeCache();
        cachedCam.Follow = playerTarget;

        cachedCam.PreviousStateIsValid = false;

        yield return null; // Frame essencial para o Confiner processar a troca

        // FASE 3: Zoom Final
        // O alvo é: Se quer sala inteira -> Limite da sala. 
        // Senão -> O que vc pediu (9), mas nunca passando do limite da sala.
        float finalTarget = showEntireRoom ? roomLimit : Mathf.Min(targetOrthographicSize, roomLimit);

        timer = 0f;
        while (timer < halfTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / halfTime);
            cachedCam.Lens.OrthographicSize = Mathf.Lerp(peakZoom, finalTarget, t);
            timer += Time.deltaTime;
            yield return null;
        }
        cachedCam.Lens.OrthographicSize = finalTarget;

        activeRoutine = null;
    }

    // Função corrigida e consolidada
    private float CalculateSafeZoom()
    {
        float aspect = cachedCam.Lens.Aspect;
        if (aspect < 0.01f) aspect = (float)Screen.width / Screen.height;

        Bounds b = roomCollider.bounds;
        float halfH = b.extents.y;
        float halfW_as_H = b.extents.x / aspect;

        // 1. O limite físico puro (a parede)
        float physicalLimit = Mathf.Min(halfH, halfW_as_H);

        // 2. Aplica o padding (espaço para o player andar)
        float fitSize = physicalLimit * roomPadding;

        // 3. Aplica o Teto Absoluto (configuração nova)
        // Retorna o menor entre "Tamanho da Sala com folga" e "14" (ou o valor que vc definiu)
        return Mathf.Min(fitSize, absoluteMaxZoom);
    }
}