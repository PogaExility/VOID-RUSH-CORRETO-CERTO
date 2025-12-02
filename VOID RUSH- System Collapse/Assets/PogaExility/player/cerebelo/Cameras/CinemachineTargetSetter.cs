using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera), typeof(CinemachineConfiner2D))]
public class CinemachineTargetSetter : MonoBehaviour
{
    [Header("Configurações de Rastreamento")]
    [Tooltip("Tag usada para identificar o Player na cena.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Intervalo (em segundos) entre as verificações de segurança. Valores menores são mais precisos, mas gastam mais CPU.")]
    [SerializeField] private float checkInterval = 0.5f;

    // Estado interno
    private float nextCheckTime = 0f;
    private CinemachineCamera virtualCamera;
    private CinemachineConfiner2D confiner;

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

    void Update()
    {
        // VERIFICAÇÃO 1: O Player sumiu?
        // Isso acontece em respawns, trocas de cena ou destruição do objeto.
        if (virtualCamera.Follow == null)
        {
            FindAndSetupPlayer();
            return;
        }

        // VERIFICAÇÃO 2: O Player saiu da área permitida?
        // Executa periodicamente para economizar processamento.
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
            // Reconecta a câmera ao player
            virtualCamera.Follow = playerTarget.transform;

            // Como acabamos de achar o player (talvez num respawn),
            // não sabemos em que sala ele está. Forçamos uma busca imediata.
            ForceRoomUpdate(playerTarget);
        }
    }

    private void ValidatePlayerBounds()
    {
        // Se o Confiner não tem nenhum colisor definido, o sistema está quebrado.
        // Precisamos achar uma sala urgentemente.
        if (confiner.BoundingShape2D == null)
        {
            ForceRoomUpdate(virtualCamera.Follow.gameObject);
            return;
        }

        // Se o Confiner tem forma, verificamos se o player ainda está dentro dela.
        // Bounds.Contains é uma verificação rápida (caixa retangular).
        // Se o player saiu (bug de colisão, teleporte), forçamos a atualização.
        if (!confiner.BoundingShape2D.bounds.Contains(virtualCamera.Follow.position))
        {
            // Debug.Log("[TargetSetter] Player fora dos limites! Buscando nova sala...");
            ForceRoomUpdate(virtualCamera.Follow.gameObject);
        }
    }

    private void ForceRoomUpdate(GameObject player)
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();

        // Sem collider no player, não dá para saber onde ele está.
        if (playerCollider == null) return;

        // Busca todas as salas (RoomBoundary) existentes na cena.
        // NOTA: FindObjectsByType pode ser pesado se houverem milhares de objetos,
        // mas como este método roda apenas em emergências (respawn/erro), é aceitável.
        RoomBoundary[] allRooms = FindObjectsByType<RoomBoundary>(FindObjectsSortMode.None);

        foreach (var room in allRooms)
        {
            Collider2D roomCol = room.GetComponent<Collider2D>();

            // Ignora salas mal configuradas
            if (roomCol == null) continue;

            // Verifica se a posição do player está dentro desta sala específica
            if (roomCol.bounds.Contains(player.transform.position))
            {
                // Encontramos a sala correta!
                // Chamamos ActivateRoom para que o RoomBoundary configure
                // o Zoom, o Confiner e os limites matemáticos corretamente.
                room.ActivateRoom(playerCollider);
                return; // Encerra o loop, trabalho feito.
            }
        }
    }
}