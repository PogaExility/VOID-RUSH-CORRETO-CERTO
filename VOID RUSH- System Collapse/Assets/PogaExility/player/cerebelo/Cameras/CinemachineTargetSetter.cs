using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera), typeof(CinemachineConfiner2D))]
public class CinemachineTargetSetter : MonoBehaviour
{
    [Header("Configurações de Rastreamento")]
    [Tooltip("Tag usada para identificar o Player na cena.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Distância mínima num único frame para considerar como Teleporte/Respawn.")]
    [SerializeField] private float teleportThreshold = 5.0f; // <--- NOVO

    [Tooltip("Intervalo entre verificações lentas (segurança).")]
    [SerializeField] private float checkInterval = 0.5f;

    // Estado interno
    private float nextCheckTime = 0f;
    private Vector3 lastPlayerPosition; // <--- NOVO
    private CinemachineCamera virtualCamera;
    private CinemachineConfiner2D confiner;
    private RoomBoundary currentActiveRoom;

    void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        confiner = GetComponent<CinemachineConfiner2D>();

        if (virtualCamera == null) Debug.LogError("[TargetSetter] CinemachineCamera não encontrado!", this);
        if (confiner == null) Debug.LogError("[TargetSetter] CinemachineConfiner2D não encontrado!", this);
    }

    void Start()
    {
        // Tenta configurar o player imediatamente no início da cena
        FindAndSetupPlayer();
    }

    // TROQUE o 'void Update()' por este método.
    // LateUpdate roda após o movimento do player, evitando tremedeira.
    void LateUpdate()
    {
        // 1. O Player sumiu? (Troca de cena/Destroy)
        if (virtualCamera.Follow == null)
        {
            FindAndSetupPlayer();
            return;
        }

        // MUDANÇA IMPORTANTE:
        // Removi completamente a lógica de "distanceMoved > teleportThreshold".
        // Isso parará os teleportes malucos da câmera.
        // Agora, o corte seco (reset da câmera) só acontece se o GameOverManager mandar.

        // 2. Verificação Periódica de Segurança (Lenta)
        // Garante que se o Confiner perder a referência, tentamos achar a sala novamente.
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            ValidatePlayerBounds();
        }
    }

    private void FindAndSetupPlayer()
    {
        GameObject playerTarget = GameObject.FindGameObjectWithTag(playerTag);

        if (playerTarget != null)
        {
            virtualCamera.Follow = playerTarget.transform;
            lastPlayerPosition = playerTarget.transform.position; // Reseta a posição

            // Como acabamos de achar o player, forçamos o setup da sala inicial (Instantâneo)
            ForceRoomUpdate(playerTarget, true);
        }
    }

    public void ForceRoomUpdate(GameObject player, bool isTeleport = false)
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();

        if (playerCollider == null) return;

        // Otimização: FindObjectsByType pode ser pesado, mas necessário aqui.
        RoomBoundary[] allRooms = FindObjectsByType<RoomBoundary>(FindObjectsSortMode.None);

        foreach (var room in allRooms)
        {
            Collider2D roomCol = room.GetComponent<Collider2D>();
            if (roomCol == null) continue;

            // Verifica se o player está realmente dentro desta sala
            if (roomCol.OverlapPoint(player.transform.position))
            {
                // CORREÇÃO DO TRAVAMENTO:
                // Se já estamos nesta sala e NÃO é um teleporte (respawn), 
                // não fazemos nada. Isso impede que o ValidatePlayerBounds 
                // fique resetando a câmera a cada 0.5s causando "lags".
                if (room == currentActiveRoom && !isTeleport)
                {
                    return;
                }

                // Atualiza a sala atual e ativa
                currentActiveRoom = room;
                room.ActivateRoom(playerCollider, isTeleport);
                return;
            }
        }
    }


    private void ValidatePlayerBounds()
    {
        // ERRO CRÍTICO: Confiner perdeu a referência do shape.
        // Precisamos achar uma sala urgentemente e cortar para ela (Instant = true).
        if (confiner.BoundingShape2D == null)
        {
            ForceRoomUpdate(virtualCamera.Follow.gameObject, true);
            return;
        }

        // ERRO DE FÍSICA: Player atravessou a parede ou saiu do trigger.
        // Bounds.Contains é uma verificação rápida.
        if (!confiner.BoundingShape2D.bounds.Contains(virtualCamera.Follow.position))
        {
            // Tenta achar a sala certa suavemente (Instant = false)
            ForceRoomUpdate(virtualCamera.Follow.gameObject, false);
        }
    }

  
}