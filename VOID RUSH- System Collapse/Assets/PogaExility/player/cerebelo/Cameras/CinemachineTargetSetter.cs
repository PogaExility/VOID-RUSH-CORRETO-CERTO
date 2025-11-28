using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera), typeof(CinemachineConfiner2D))]
public class CinemachineTargetSetter : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    // Intervalo para verificar se o player saiu dos limites (segurança contra bugs de colisão/respawn)
    [SerializeField] private float checkInterval = 0.5f;
    private float nextCheckTime = 0f;

    private CinemachineCamera virtualCamera;
    private CinemachineConfiner2D confiner;

    void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        confiner = GetComponent<CinemachineConfiner2D>();
    }

    void Start()
    {
        FindAndSetupPlayer();
    }

    void Update()
    {
        // CASO 1: Player morreu/foi destruído (perdeu o Follow)
        if (virtualCamera.Follow == null)
        {
            FindAndSetupPlayer();
        }

        // CASO 2: Verificação periódica. Se o player existe, checa se ele ainda está dentro da sala atual.
        if (virtualCamera.Follow != null && Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            EnsurePlayerIsInsideBounds();
        }
    }

    private void FindAndSetupPlayer()
    {
        GameObject playerTarget = GameObject.FindGameObjectWithTag(playerTag);

        if (playerTarget != null)
        {
            virtualCamera.Follow = playerTarget.transform;
            // Força a busca pela sala onde o player nasceu/renasceu
            FindRoomForPlayer(playerTarget);
        }
    }

    private void EnsurePlayerIsInsideBounds()
    {
        // Se o confiner ainda não tem forma, ou se o player saiu da forma atual...
        if (confiner.BoundingShape2D == null || !confiner.BoundingShape2D.bounds.Contains(virtualCamera.Follow.position))
        {
            // ...tenta achar a sala correta novamente.
            FindRoomForPlayer(virtualCamera.Follow.gameObject);
        }
    }

    private void FindRoomForPlayer(GameObject player)
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null) return;

        // Procura em todas as salas da cena
        RoomBoundary[] allRooms = FindObjectsByType<RoomBoundary>(FindObjectsSortMode.None);

        foreach (var room in allRooms)
        {
            Collider2D roomCol = room.GetComponent<Collider2D>();

            // Se o player estiver dentro dos limites desta sala
            if (roomCol != null && roomCol.bounds.Contains(player.transform.position))
            {
                Debug.Log($"Cinemachine: Player encontrado em '{room.name}'. Ativando sala.");
                room.ActivateRoom(playerCollider);
                return;
            }
        }
    }
}