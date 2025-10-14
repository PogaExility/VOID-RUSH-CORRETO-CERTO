using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    [Header("Modo da Sala")]
    [SerializeField] private bool showEntireRoom = false;

    [Header("Configurações de Câmera (Modo Seguir)")]
    [SerializeField] private float targetOrthographicSize = 9f;

    [Header("Configurações de Câmera (Modo Sala Inteira)")]
    [SerializeField] private float roomPadding = 1.1f;

    [Header("Configurações de Transição")]
    [SerializeField] private float positionTransitionDuration = 0.8f;

    private Collider2D roomCollider;
    private static Coroutine activeTransitionCoroutine;
    private static Transform _proxyTarget;
    private static RoomBoundary currentActiveRoom = null;
    private static RoomBoundary nextPotentialRoom = null;



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
        {
            Debug.LogWarning($"O Collider2D no objeto '{gameObject.name}' precisa estar marcado como 'Is Trigger'.", gameObject);
        }
    }

    // --- OnTriggerEnter AGORA É O ÚNICO GATILHO ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Se esta é a primeira sala, ativa-a imediatamente.
            if (currentActiveRoom == null)
            {
                ActivateRoom(other);
            }
            // Se o jogador está entrando em uma nova sala, anota-a como o próximo destino.
            else if (this != currentActiveRoom)
            {
                Debug.Log($"Jogador entrou na área de '{gameObject.name}'. Preparando como próximo destino.");
                nextPotentialRoom = this;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Se o jogador está saindo da sala ATUALMENTE ATIVA...
            if (this == currentActiveRoom)
            {
                // ...e existe uma sala de destino esperando...
                if (nextPotentialRoom != null)
                {
                    // ...então a transição é autorizada.
                    Debug.Log($"Jogador saiu completamente de '{gameObject.name}'. Ativando '{nextPotentialRoom.name}'.");
                    nextPotentialRoom.ActivateRoom(other);
                }
            }
            // Se o jogador sai de uma sala que era um destino potencial (ou seja, ele recuou)...
            else if (this == nextPotentialRoom)
            {
                // ...limpa a anotação para cancelar a transição pendente.
                Debug.Log($"Jogador recuou de '{gameObject.name}'. Transição pendente cancelada.");
                nextPotentialRoom = null;
            }
        }
    }


    // O método de ativação agora é o coração do sistema.
    public void ActivateRoom(Collider2D playerCollider)
    {
        // Define esta sala como a ativa
        currentActiveRoom = this;
        // Limpa qualquer destino pendente, pois a transição foi concluída.
        nextPotentialRoom = null;

        CinemachineCamera virtualCamera = FindAnyObjectByType<CinemachineCamera>();
        if (virtualCamera == null) return;

        if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

        if (showEntireRoom)
        {
            virtualCamera.Lens.OrthographicSize = CalculateOrthographicSize();
            UpdateConfiner();
            activeTransitionCoroutine = StartCoroutine(TransitionProxyTarget(virtualCamera, playerCollider.transform));
        }
        else
        {
            virtualCamera.Lens.OrthographicSize = targetOrthographicSize;
            UpdateConfiner();
            virtualCamera.Follow = playerCollider.transform;
        }
    }

    private void UpdateConfiner()
    {
        CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();
        if (confiner != null)
        {
            confiner.BoundingShape2D = roomCollider;
            confiner.InvalidateBoundingShapeCache();
            Debug.Log($"Confiner ATUALIZADO para: '{roomCollider.name}'.");
        }
    }

    private IEnumerator TransitionProxyTarget(CinemachineCamera cam, Transform playerTransform)
    {
        cam.Follow = ProxyTarget;
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

    private float CalculateOrthographicSize()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float roomAspect = roomCollider.bounds.size.x / roomCollider.bounds.size.y;
        float size;
        if (roomAspect > screenAspect) { size = (roomCollider.bounds.size.x / screenAspect) * 0.5f; }
        else { size = roomCollider.bounds.size.y * 0.5f; }
        return size * roomPadding;
    }

    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }
}