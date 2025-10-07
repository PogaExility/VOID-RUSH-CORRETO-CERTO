using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    [Header("Modo da Sala")]
    [Tooltip("Marque esta opção para a câmera mostrar a sala inteira. Desmarcada, ela seguirá o jogador.")]
    [SerializeField] private bool showEntireRoom = false;

    [Header("Configurações de Câmera (Modo Seguir)")]
    [Tooltip("O tamanho (zoom) que a câmera terá ao seguir o jogador nesta sala.")]
    [SerializeField] private float targetOrthographicSize = 9f;

    [Header("Configurações de Câmera (Modo Sala Inteira)")]
    [Tooltip("Uma margem para o zoom, para que a sala não fique colada nas bordas da tela. 1.1 = 10% de margem.")]
    [SerializeField] private float roomPadding = 1.1f;

    [Header("Configurações de Transição")]
    [Tooltip("A duração em segundos da animação do alvo da câmera para o centro da sala (apenas no modo Sala Inteira).")]
    [SerializeField] private float positionTransitionDuration = 0.8f;

    private Collider2D roomCollider;
    private static Coroutine activeTransitionCoroutine;

    // Alvo fantasma para guiar a câmera no modo "Sala Inteira"
    private static Transform _proxyTarget;
    private static Transform ProxyTarget
    {
        get
        {
            if (_proxyTarget == null)
            {
                GameObject proxyGO = new GameObject("CameraProxyTarget");
                _proxyTarget = proxyGO.transform;
                // Opcional: Impedir que o alvo seja destruído ao carregar novas cenas
                // DontDestroyOnLoad(proxyGO);
            }
            return _proxyTarget;
        }
    }

    void Awake()
    {
        roomCollider = GetComponent<Collider2D>();
        if (!roomCollider.isTrigger)
        {
            Debug.LogWarning($"O Collider2D no objeto '{gameObject.name}' precisa estar marcado como 'Is Trigger' para o script RoomBoundary funcionar.", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CinemachineCamera virtualCamera = FindAnyObjectByType<CinemachineCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("ERRO: RoomBoundary não encontrou uma CinemachineCamera.");
                return;
            }

            // Para qualquer transição de posição que esteja ocorrendo.
            if (activeTransitionCoroutine != null)
            {
                StopCoroutine(activeTransitionCoroutine);
                activeTransitionCoroutine = null;
            }

            // --- LÓGICA CORRIGIDA ---
            if (showEntireRoom)
            {
                // MODO SALA INTEIRA
                // 1. Define o zoom final IMEDIATAMENTE.
                virtualCamera.Lens.OrthographicSize = CalculateOrthographicSize();
                // 2. Atualiza o confiner com as informações corretas.
                UpdateConfiner();
                // 3. Inicia a transição SUAVE apenas da POSIÇÃO do alvo.
                activeTransitionCoroutine = StartCoroutine(TransitionProxyTarget(virtualCamera, other.transform));
            }
            else
            {
                // MODO SEGUIR JOGADOR
                // 1. Define o zoom final IMEDIATAMENTE.
                virtualCamera.Lens.OrthographicSize = targetOrthographicSize;
                // 2. Atualiza o confiner com as informações corretas.
                UpdateConfiner();
                // 3. Garante que a câmera volte a seguir o jogador. O Damping fará a suavização.
                virtualCamera.Follow = other.transform;
            }
        }
    }

    private void UpdateConfiner()
    {
        CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();
        if (confiner != null)
        {
            confiner.BoundingShape2D = roomCollider;
            confiner.InvalidateBoundingShapeCache();
            Debug.Log($"Confiner atualizado para '{roomCollider.name}'.");
        }
    }

    // Coroutine para mover suavemente o ALVO FANTASMA para o centro da sala.
    private IEnumerator TransitionProxyTarget(CinemachineCamera cam, Transform playerTransform)
    {
        cam.Follow = ProxyTarget;
        // Posição inicial usa a do jogador para evitar um salto inicial
        Vector3 startPosition = playerTransform.position;
        Vector3 targetPosition = roomCollider.bounds.center;

        float elapsedTime = 0f;
        while (elapsedTime < positionTransitionDuration)
        {
            float progress = elapsedTime / positionTransitionDuration;
            ProxyTarget.position = Vector3.Lerp(startPosition, targetPosition, EaseInOut(progress));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ProxyTarget.position = targetPosition;
        activeTransitionCoroutine = null;
    }

    // Calcula o tamanho ortográfico ideal para enquadrar o colisor da sala.
    private float CalculateOrthographicSize()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float roomAspect = roomCollider.bounds.size.x / roomCollider.bounds.size.y;
        float size;
        if (roomAspect > screenAspect)
        {
            size = (roomCollider.bounds.size.x / screenAspect) * 0.5f;
        }
        else
        {
            size = roomCollider.bounds.size.y * 0.5f;
        }
        return size * roomPadding;
    }

    // Função de suavização (Ease In/Out) para transições mais agradáveis.
    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }
}