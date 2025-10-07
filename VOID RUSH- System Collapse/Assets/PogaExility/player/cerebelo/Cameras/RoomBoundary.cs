using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    [Header("Modo da Sala")]
    [Tooltip("Marque esta op��o para a c�mera mostrar a sala inteira. Desmarcada, ela seguir� o jogador.")]
    [SerializeField] private bool showEntireRoom = false;

    [Header("Configura��es de C�mera (Modo Seguir)")]
    [Tooltip("O tamanho (zoom) que a c�mera ter� ao seguir o jogador nesta sala.")]
    [SerializeField] private float targetOrthographicSize = 9f;

    [Header("Configura��es de C�mera (Modo Sala Inteira)")]
    [Tooltip("Uma margem para o zoom, para que a sala n�o fique colada nas bordas da tela. 1.1 = 10% de margem.")]
    [SerializeField] private float roomPadding = 1.1f;

    [Header("Configura��es de Transi��o")]
    [Tooltip("A dura��o em segundos da anima��o do alvo da c�mera para o centro da sala (apenas no modo Sala Inteira).")]
    [SerializeField] private float positionTransitionDuration = 0.8f;

    private Collider2D roomCollider;
    private static Coroutine activeTransitionCoroutine;

    // Alvo fantasma para guiar a c�mera no modo "Sala Inteira"
    private static Transform _proxyTarget;
    private static Transform ProxyTarget
    {
        get
        {
            if (_proxyTarget == null)
            {
                GameObject proxyGO = new GameObject("CameraProxyTarget");
                _proxyTarget = proxyGO.transform;
                // Opcional: Impedir que o alvo seja destru�do ao carregar novas cenas
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
                Debug.LogError("ERRO: RoomBoundary n�o encontrou uma CinemachineCamera.");
                return;
            }

            // Para qualquer transi��o de posi��o que esteja ocorrendo.
            if (activeTransitionCoroutine != null)
            {
                StopCoroutine(activeTransitionCoroutine);
                activeTransitionCoroutine = null;
            }

            // --- L�GICA CORRIGIDA ---
            if (showEntireRoom)
            {
                // MODO SALA INTEIRA
                // 1. Define o zoom final IMEDIATAMENTE.
                virtualCamera.Lens.OrthographicSize = CalculateOrthographicSize();
                // 2. Atualiza o confiner com as informa��es corretas.
                UpdateConfiner();
                // 3. Inicia a transi��o SUAVE apenas da POSI��O do alvo.
                activeTransitionCoroutine = StartCoroutine(TransitionProxyTarget(virtualCamera, other.transform));
            }
            else
            {
                // MODO SEGUIR JOGADOR
                // 1. Define o zoom final IMEDIATAMENTE.
                virtualCamera.Lens.OrthographicSize = targetOrthographicSize;
                // 2. Atualiza o confiner com as informa��es corretas.
                UpdateConfiner();
                // 3. Garante que a c�mera volte a seguir o jogador. O Damping far� a suaviza��o.
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
        // Posi��o inicial usa a do jogador para evitar um salto inicial
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

    // Calcula o tamanho ortogr�fico ideal para enquadrar o colisor da sala.
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

    // Fun��o de suaviza��o (Ease In/Out) para transi��es mais agrad�veis.
    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }
}