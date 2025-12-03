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
        // 1. O Player sumiu? (Troca de cena/Destroy)
        if (virtualCamera.Follow == null)
        {
            FindAndSetupPlayer();
            return;
        }

        Vector3 currentPlayerPos = virtualCamera.Follow.position;

        // 2. DETECÇÃO DE TELEPORTE / RESPAWN (Instantâneo)
        // Se a distância entre o frame anterior e este for grande, assumimos que foi um Respawn.
        float distanceMoved = Vector3.Distance(currentPlayerPos, lastPlayerPosition);

        if (distanceMoved > teleportThreshold)
        {
            // Força atualização imediata com "Corte Seco" (true)
            ForceRoomUpdate(virtualCamera.Follow.gameObject, true);
        }

        // Atualiza a última posição para o próximo frame
        lastPlayerPosition = currentPlayerPos;

        // 3. Verificação Periódica (Lenta)
        // Garante que se o player sair andando dos limites (glitch), a câmera corrija.
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

    // Agora aceita o parâmetro 'isTeleport'
    // Substitua TODAS as versões anteriores de ForceRoomUpdate por esta única função
    private void ForceRoomUpdate(GameObject player, bool isTeleport = false)
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();

        // Sem collider no player, impossível calcular a posição exata
        if (playerCollider == null) return;

        // Busca todas as salas
        RoomBoundary[] allRooms = FindObjectsByType<RoomBoundary>(FindObjectsSortMode.None);

        foreach (var room in allRooms)
        {
            Collider2D roomCol = room.GetComponent<Collider2D>();
            if (roomCol == null) continue;

            // 1. Otimização: Checagem rápida de caixa (Bounds)
            if (roomCol.bounds.Contains(player.transform.position))
            {
                // 2. Precisão: O ponto exato está dentro do polígono? (OverlapPoint)
                // Isso impede que a câmera pegue a sala vizinha se estivermos na parede
                if (roomCol.OverlapPoint(player.transform.position))
                {
                    // Encontrou! Ativa a sala.
                    // Se isTeleport for true, o RoomBoundary fará o "Corte Seco" e resetará a câmera.
                    room.ActivateRoom(playerCollider, isTeleport);
                    return;
                }
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