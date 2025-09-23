using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    [Header("Configura��o de Zoom")]
    [Tooltip("O tamanho do zoom quando a c�mera est� seguindo o jogador livremente.")]
    public float followOrthographicSize = 6f;
    [Tooltip("A velocidade da transi��o de zoom.")]
    public float zoomTransitionSpeed = 2f;
    [Tooltip("O valor m�nimo que o zoom autom�tico da sala pode atingir.")]
    public float minOrthographicSize = 5f;
    [Tooltip("O valor m�ximo que o zoom autom�tico da sala pode atingir.")]
    public float maxOrthographicSize = 20f;

    // --- C�REBRO COMPARTILHADO ---
    private static CinemachineConfiner2D activeConfiner;
    private static CinemachineCamera activeVirtualCamera;
    private static RoomBoundary currentActiveRoom;
    private static Coroutine activeZoomCoroutine;
    // --- FIM DO C�REBRO COMPARTILHADO ---

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
            // CORRE��O 1: Fornecendo o segundo argumento necess�rio.
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
            // CORRE��O 2: Usando o m�todo moderno para encontrar um objeto.
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