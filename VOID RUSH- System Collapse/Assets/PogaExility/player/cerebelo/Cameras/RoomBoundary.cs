using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    [Header("Configuração de Zoom")]
    [Tooltip("O tamanho do zoom quando a câmera está seguindo o jogador livremente.")]
    public float followOrthographicSize = 6f;
    [Tooltip("A velocidade da transição de zoom.")]
    public float zoomTransitionSpeed = 2f;
    [Tooltip("O valor mínimo que o zoom automático da sala pode atingir.")]
    public float minOrthographicSize = 5f;
    [Tooltip("O valor máximo que o zoom automático da sala pode atingir.")]
    public float maxOrthographicSize = 20f;

    // --- CÉREBRO COMPARTILHADO ---
    private static CinemachineConfiner2D activeConfiner;
    private static CinemachineCamera activeVirtualCamera;
    private static RoomBoundary currentActiveRoom;
    private static Coroutine activeZoomCoroutine;
    // --- FIM DO CÉREBRO COMPARTILHADO ---

    private Collider2D roomCollider;
    private float calculatedOrthographicSize;

    private void Awake()
    {
        roomCollider = GetComponent<Collider2D>();
        roomCollider.isTrigger = true;
        CalculateOptimalZoom();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Se o jogador entrou nesta sala e ela não é a sala ativa, ativa-a.
            if (currentActiveRoom != this)
            {
                ActivateRoom();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Se o jogador está saindo da sala que estava ativa, entra em modo livre.
            if (currentActiveRoom == this)
            {
                currentActiveRoom = null;
                SetFollowMode();
            }
        }
    }

    private void CalculateOptimalZoom()
    {
        Bounds bounds = roomCollider.bounds;
        float screenRatio = (float)Screen.width / Screen.height;
        float requiredSizeX = (bounds.size.x / screenRatio) / 2f * 1.1f;
        float requiredSizeY = bounds.size.y / 2f * 1.1f;

        float optimalSize = Mathf.Max(requiredSizeX, requiredSizeY);
        calculatedOrthographicSize = Mathf.Clamp(optimalSize, minOrthographicSize, maxOrthographicSize);
    }

    // Tornamos este método público para que o script da porta possa chamá-lo.
    public void ActivateRoom()
    {
        currentActiveRoom = this;
        InitializeCameraReferences();

        if (activeConfiner != null)
        {
            activeConfiner.BoundingShape2D = roomCollider;
            StartZoomTransition(calculatedOrthographicSize);
            Debug.Log($"Sala '{gameObject.name}' ativada. Zoom: {calculatedOrthographicSize}.");
        }
    }

    // Tornamos este método estático para ser chamado de qualquer lugar.
    public static void SetFollowMode()
    {
        currentActiveRoom = null; // Se estamos em modo livre, nenhuma sala está ativa.
        InitializeCameraReferences();

        if (activeConfiner != null)
        {
            activeConfiner.BoundingShape2D = null;
            // Usa o primeiro RoomBoundary que encontrar para pegar os valores de zoom e velocidade.
            // Isso assume que os valores de follow são os mesmos para todas as salas.
            RoomBoundary anyRoom = FindObjectOfType<RoomBoundary>();
            if (anyRoom != null)
            {
                StartZoomTransition(anyRoom.followOrthographicSize, anyRoom.zoomTransitionSpeed);
            }
            Debug.Log("Modo 'Follow' ativado.");
        }
    }

    // Tornamos estático para ser chamado pelo SetFollowMode.
    public static void StartZoomTransition(float targetSize, float transitionSpeed)
    {
        InitializeCameraReferences();
        if (activeVirtualCamera == null) return;

        if (activeZoomCoroutine != null)
        {
            activeVirtualCamera.StopCoroutine(activeZoomCoroutine);
        }
        activeZoomCoroutine = activeVirtualCamera.StartCoroutine(SmoothZoomCoroutine(targetSize, transitionSpeed));
    }

    private static IEnumerator SmoothZoomCoroutine(float targetSize, float transitionSpeed)
    {
        while (activeVirtualCamera != null && !Mathf.Approximately(activeVirtualCamera.Lens.OrthographicSize, targetSize))
        {
            float newSize = Mathf.MoveTowards(activeVirtualCamera.Lens.OrthographicSize, targetSize, transitionSpeed * Time.deltaTime);
            activeVirtualCamera.Lens.OrthographicSize = newSize;
            yield return null;
        }
        if (activeVirtualCamera != null) activeVirtualCamera.Lens.OrthographicSize = targetSize;
        activeZoomCoroutine = null;
    }

    private static void InitializeCameraReferences()
    {
        if (activeVirtualCamera == null)
        {
            GameObject vcamObject = GameObject.FindGameObjectWithTag("VirtualCamera");
            if (vcamObject != null)
            {
                activeVirtualCamera = vcamObject.GetComponent<CinemachineCamera>();
                activeConfiner = vcamObject.GetComponent<CinemachineConfiner2D>();
            }
        }
    }
}