using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    // --- GERENCIADOR DE ESTADO GLOBAL ---
    public static RoomBoundary currentActiveRoom;

    // --- C�REBRO COMPARTILHADO (REFER�NCIAS EST�TICAS) ---
    private static CinemachineCamera activeVirtualCamera;
    private static CinemachineConfiner2D activeConfiner;
    private static float currentSize;

    [Header("Configura��o de Zoom")]
    [Tooltip("A velocidade da transi��o de zoom.")]
    public float zoomTransitionSpeed = 2f;

    [Tooltip("Fator de corre��o para o zoom. Diminua este valor (ex: 0.9) se a c�mera estiver mostrando al�m dos limites.")]
    [Range(0.5f, 1.5f)] // Cria um slider no Inspector
    public float zoomCorrectionFactor = 1.0f;

    // Vari�veis de inst�ncia
    private Collider2D roomCollider;
    private float targetOrthographicSize;

    private void Awake()
    {
        roomCollider = GetComponent<Collider2D>();
        roomCollider.isTrigger = true;
        CalculateOptimalZoom();
    }

    private void Update()
    {
        InitializeCameraReferences();
        if (activeVirtualCamera == null) return;

        if (currentActiveRoom == this)
        {
            currentSize = Mathf.MoveTowards(currentSize, targetOrthographicSize, zoomTransitionSpeed * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        InitializeCameraReferences();
        if (activeVirtualCamera == null) return;

        activeVirtualCamera.Lens.OrthographicSize = currentSize;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (currentActiveRoom != this)
        {
            ActivateRoom();
        }
    }

    private void ActivateRoom()
    {
        InitializeCameraReferences();
        currentActiveRoom = this;

        if (activeConfiner != null)
        {
            activeConfiner.BoundingShape2D = roomCollider;
            activeConfiner.InvalidateBoundingShapeCache();
        }
    }

    /// <summary>
    /// Fun��o final e corrigida.
    /// </summary>
    private void CalculateOptimalZoom()
    {
        Vector2 size = (roomCollider as BoxCollider2D).size;
        Vector3 scale = transform.lossyScale;
        float width = size.x * scale.x;
        float height = size.y * scale.y;

        float screenRatio = (float)Screen.width / Screen.height;
        float requiredSizeX = (width / screenRatio) / 2f;
        float requiredSizeY = height / 2f;

        float optimalSize = Mathf.Max(requiredSizeX, requiredSizeY);

        // --- A CORRE��O FINAL EST� AQUI ---
        // Aplicamos o fator de corre��o manual ao resultado final.
        targetOrthographicSize = optimalSize * zoomCorrectionFactor;

        Debug.Log($"Zoom calculado para '{gameObject.name}': {optimalSize} * {zoomCorrectionFactor} = {targetOrthographicSize}");
    }

    private static void InitializeCameraReferences()
    {
        if (activeVirtualCamera != null) return;

        GameObject vcamObject = GameObject.FindGameObjectWithTag("VirtualCamera");
        if (vcamObject != null)
        {
            activeVirtualCamera = vcamObject.GetComponent<CinemachineCamera>();
            activeConfiner = vcamObject.GetComponent<CinemachineConfiner2D>();

            if (activeVirtualCamera != null)
            {
                currentSize = activeVirtualCamera.Lens.OrthographicSize;
            }
        }
    }
}