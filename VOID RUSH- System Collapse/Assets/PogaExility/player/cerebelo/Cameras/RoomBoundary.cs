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

    public void ActivateRoom()
    {
        currentActiveRoom = this;
        InitializeCameraReferences();

        if (activeConfiner != null)
        {
            activeConfiner.BoundingShape2D = roomCollider;
            // CORREÇÃO 1: Fornecendo o segundo argumento necessário.
            StartZoomTransition(calculatedOrthographicSize, zoomTransitionSpeed);
            Debug.Log($"Sala '{gameObject.name}' ativada. Zoom: {calculatedOrthographicSize}.");
        }
    }

    public static void SetFollowMode()
    {
        currentActiveRoom = null;
        InitializeCameraReferences();

        if (activeConfiner != null)
        {
            activeConfiner.BoundingShape2D = null;
            // CORREÇÃO 2: Usando o método moderno para encontrar um objeto.
            RoomBoundary anyRoom = FindAnyObjectByType<RoomBoundary>();
            if (anyRoom != null)
            {
                StartZoomTransition(anyRoom.followOrthographicSize, anyRoom.zoomTransitionSpeed);
            }
            Debug.Log("Modo 'Follow' ativado.");
        }
    }

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