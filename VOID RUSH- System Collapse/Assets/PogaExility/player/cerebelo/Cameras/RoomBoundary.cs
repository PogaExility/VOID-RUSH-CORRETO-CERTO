using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic; // Necessário para usar List

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

    [Header("Configuração das Portas")]
    [Tooltip("Arraste para cá todos os Colliders que funcionarão como 'portas' para esta sala.")]
    public List<Collider2D> doorTriggers;

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

        // Garante que todas as portas também sejam triggers.
        foreach (var door in doorTriggers)
        {
            if (door != null) door.isTrigger = true;
        }

        CalculateOptimalZoom();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // --- PONTO DE VERIFICAÇÃO 1 ---
        Debug.Log($"Jogador entrou no trigger '{other.gameObject.name}' DENTRO da área de '{gameObject.name}'.");

        if (doorTriggers.Contains(other))
        {
            SetFollowMode();
            return;
        }

        if (other == roomCollider)
        {
            if (currentActiveRoom != this)
            {
                // --- PONTO DE VERIFICAÇÃO 2 ---
                Debug.Log($"Condição para ativar a sala '{gameObject.name}' foi satisfeita. Chamando ActivateRoom...");
                ActivateRoom();
            }
            else
            {
                // --- PONTO DE VERIFICAÇÃO (EXTRA) ---
                Debug.Log($"Jogador entrou na sala '{gameObject.name}', mas ela já é a sala ativa. Nenhuma ação necessária.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Se o jogador está saindo do collider principal da sala...
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
        // Se o jogador está saindo de uma porta...
        else if (doorTriggers.Contains(other))
        {
            // ...e ainda está dentro do collider desta sala, reativa a sala.
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
        // --- PONTO DE VERIFICAÇÃO 3 ---
        Debug.Log($"Iniciando ActivateRoom para '{gameObject.name}'.");

        currentActiveRoom = this;
        InitializeCameraReferences();

        if (activeConfiner != null)
        {
            // --- PONTO DE VERIFICAÇÃO 4 ---
            Debug.Log($"'activeConfiner' encontrado! Atribuindo '{roomCollider.name}' ao BoundingShape2D.");
            activeConfiner.BoundingShape2D = roomCollider;
            StartZoomTransition(calculatedOrthographicSize);
        }
        else
        {
            // --- PONTO DE VERIFICAÇÃO DE FALHA ---
            Debug.LogError($"FALHA em ActivateRoom para '{gameObject.name}': 'activeConfiner' é NULO. A câmera não foi encontrada ou não tem o componente Confiner2D.");
        }
    }


    private void SetFollowMode()
    {
        // Se o jogador sai para o modo livre, esta sala não pode mais ser a ativa.
        if (currentActiveRoom == this) currentActiveRoom = null;

        InitializeCameraReferences();
        if (activeConfiner != null)
        {
            activeConfiner.BoundingShape2D = null;
            StartZoomTransition(followOrthographicSize);
            Debug.Log("Modo 'Follow' ativado pela porta/saída.");
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
            // --- PONTO DE VERIFICAÇÃO 5 ---
            Debug.Log("Tentando inicializar referências da câmera...");
            GameObject vcamObject = GameObject.FindGameObjectWithTag("VirtualCamera");
            if (vcamObject != null)
            {
                Debug.Log("Objeto com a tag 'VirtualCamera' foi encontrado com sucesso!", vcamObject);
                activeVirtualCamera = vcamObject.GetComponent<CinemachineCamera>();
                activeConfiner = vcamObject.GetComponent<CinemachineConfiner2D>();
            }
            else
            {
                // --- PONTO DE VERIFICAÇÃO DE FALHA GRAVE ---
                Debug.LogError("ERRO CRÍTICO: Nenhum objeto com a tag 'VirtualCamera' foi encontrado na cena.");
            }
        }
    }

}