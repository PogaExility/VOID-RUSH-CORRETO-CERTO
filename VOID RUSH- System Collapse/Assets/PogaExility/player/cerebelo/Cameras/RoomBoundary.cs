using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic; // Necess�rio para usar List

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

    [Header("Configura��o das Portas")]
    [Tooltip("Arraste para c� todos os Colliders que funcionar�o como 'portas' para esta sala.")]
    public List<Collider2D> doorTriggers;

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

        // Garante que todas as portas tamb�m sejam triggers.
        foreach (var door in doorTriggers)
        {
            if (door != null) door.isTrigger = true;
        }

        CalculateOptimalZoom();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // --- PONTO DE VERIFICA��O 1 ---
        Debug.Log($"Jogador entrou no trigger '{other.gameObject.name}' DENTRO da �rea de '{gameObject.name}'.");

        if (doorTriggers.Contains(other))
        {
            SetFollowMode();
            return;
        }

        if (other == roomCollider)
        {
            if (currentActiveRoom != this)
            {
                // --- PONTO DE VERIFICA��O 2 ---
                Debug.Log($"Condi��o para ativar a sala '{gameObject.name}' foi satisfeita. Chamando ActivateRoom...");
                ActivateRoom();
            }
            else
            {
                // --- PONTO DE VERIFICA��O (EXTRA) ---
                Debug.Log($"Jogador entrou na sala '{gameObject.name}', mas ela j� � a sala ativa. Nenhuma a��o necess�ria.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Se o jogador est� saindo do collider principal da sala...
        if (other == roomCollider)
        {
            // ... e esta era a sala ativa, ela deixa de ser.
            if (currentActiveRoom == this)
            {
                currentActiveRoom = null;
                // Ao sair da sala, ativa o modo livre imediatamente.
                SetFollowMode();
            }
        }
        // Se o jogador est� saindo de uma porta...
        else if (doorTriggers.Contains(other))
        {
            // ...e ainda est� dentro do collider desta sala, reativa a sala.
            if (roomCollider.bounds.Contains(other.transform.position))
            {
                ActivateRoom();
            }
        }
    }

    private void CalculateOptimalZoom()
    {
        Bounds bounds = roomCollider.bounds;
        float screenRatio = (float)Screen.width / Screen.height;
        float requiredSizeX = (bounds.size.x / screenRatio) / 2f * 1.1f; // 10% padding
        float requiredSizeY = bounds.size.y / 2f * 1.1f; // 10% padding

        float optimalSize = Mathf.Max(requiredSizeX, requiredSizeY);
        calculatedOrthographicSize = Mathf.Clamp(optimalSize, minOrthographicSize, maxOrthographicSize);
    }

    private void ActivateRoom()
    {
        // --- PONTO DE VERIFICA��O 3 ---
        Debug.Log($"Iniciando ActivateRoom para '{gameObject.name}'.");

        currentActiveRoom = this;
        InitializeCameraReferences();

        if (activeConfiner != null)
        {
            // --- PONTO DE VERIFICA��O 4 ---
            Debug.Log($"'activeConfiner' encontrado! Atribuindo '{roomCollider.name}' ao BoundingShape2D.");
            activeConfiner.BoundingShape2D = roomCollider;
            StartZoomTransition(calculatedOrthographicSize);
        }
        else
        {
            // --- PONTO DE VERIFICA��O DE FALHA ---
            Debug.LogError($"FALHA em ActivateRoom para '{gameObject.name}': 'activeConfiner' � NULO. A c�mera n�o foi encontrada ou n�o tem o componente Confiner2D.");
        }
    }


    private void SetFollowMode()
    {
        // Se o jogador sai para o modo livre, esta sala n�o pode mais ser a ativa.
        if (currentActiveRoom == this) currentActiveRoom = null;

        InitializeCameraReferences();
        if (activeConfiner != null)
        {
            activeConfiner.BoundingShape2D = null;
            StartZoomTransition(followOrthographicSize);
            Debug.Log("Modo 'Follow' ativado pela porta/sa�da.");
        }
    }

    private void StartZoomTransition(float targetSize)
    {
        InitializeCameraReferences();
        if (activeVirtualCamera == null) return;

        if (activeZoomCoroutine != null)
        {
            activeVirtualCamera.StopCoroutine(activeZoomCoroutine);
        }
        activeZoomCoroutine = activeVirtualCamera.StartCoroutine(SmoothZoomCoroutine(targetSize));
    }

    private IEnumerator SmoothZoomCoroutine(float targetSize)
    {
        while (activeVirtualCamera != null && !Mathf.Approximately(activeVirtualCamera.Lens.OrthographicSize, targetSize))
        {
            float newSize = Mathf.MoveTowards(activeVirtualCamera.Lens.OrthographicSize, targetSize, zoomTransitionSpeed * Time.deltaTime);
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
            // --- PONTO DE VERIFICA��O 5 ---
            Debug.Log("Tentando inicializar refer�ncias da c�mera...");
            GameObject vcamObject = GameObject.FindGameObjectWithTag("VirtualCamera");
            if (vcamObject != null)
            {
                Debug.Log("Objeto com a tag 'VirtualCamera' foi encontrado com sucesso!", vcamObject);
                activeVirtualCamera = vcamObject.GetComponent<CinemachineCamera>();
                activeConfiner = vcamObject.GetComponent<CinemachineConfiner2D>();
            }
            else
            {
                // --- PONTO DE VERIFICA��O DE FALHA GRAVE ---
                Debug.LogError("ERRO CR�TICO: Nenhum objeto com a tag 'VirtualCamera' foi encontrado na cena.");
            }
        }
    }

}