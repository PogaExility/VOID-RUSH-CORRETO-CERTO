using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para gerenciar cenas
using TMPro; // Necessário para o TextMeshPro

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Configurações de Penalidade")]
    [Tooltip("A quantidade de dinheiro que o jogador perde ao morrer em uma missão.")]
    public int moneyPenalty = 50;

    [Header("Referências de UI (Opcional)")]
    [Tooltip("Arraste o TextMeshPro que mostrará a mensagem de perda de dinheiro.")]
    public TextMeshProUGUI penaltyText;

    // --- Variáveis de Estado ---
    private Vector3 currentCheckpointPosition;
    private string currentCheckpointSceneName; // Guardamos o NOME da cena do checkpoint

    // Referência para os outros sistemas
    private InventoryManager inventoryManager;
    // private MoneySystem moneySystem; // (Quando você tiver um, vamos conectar aqui)

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (penaltyText != null) penaltyText.gameObject.SetActive(false);
    }

    void Start()
    {
        // Encontra o jogador e define o checkpoint inicial
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentCheckpointPosition = player.transform.position;
            currentCheckpointSceneName = SceneManager.GetActiveScene().name;

            // Pega a referência do InventoryManager do jogador
            inventoryManager = player.GetComponent<InventoryManager>();
        }
    }

    // Chamado pelo script Checkpoint.cs
    public void SetNewCheckpoint(Vector3 newPosition, string sceneName)
    {
        currentCheckpointPosition = newPosition;
        currentCheckpointSceneName = sceneName;

        // Avisa o InventoryManager para tornar todos os itens temporários em permanentes
        if (inventoryManager != null)
        {
            inventoryManager.CommitTemporaryItems();
        }

        // Salva o jogo aqui, se você tiver um sistema de save
        Debug.Log($"Novo checkpoint ativado na cena '{sceneName}'");
    }

    public void RespawnPlayer(Transform playerTransform)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Verifica se o jogador morreu em uma cena "de missão" (diferente da do checkpoint)
        if (currentSceneName != currentCheckpointSceneName)
        {
            // --- APLICA AS PENALIDADES ---

            // 1. Perde itens temporários
            if (inventoryManager != null)
            {
                inventoryManager.ClearTemporaryItems();
            }

            // 2. Perde dinheiro
            // if (moneySystem != null) { moneySystem.LoseMoney(moneyPenalty); }
            Debug.Log($"Jogador perdeu {moneyPenalty} de dinheiro.");

            // Mostra a mensagem na UI (opcional)
            if (penaltyText != null)
            {
                penaltyText.text = $"-{moneyPenalty} Dinheiro";
                StartCoroutine(ShowPenaltyMessage());
            }

            // AQUI VOCÊ DEVE RECARREGAR A CENA DO CHECKPOINT
            // SceneManager.LoadScene(currentCheckpointSceneName);
            // NOTA: A lógica de carregar a cena pode ser mais complexa,
            // pode ser necessário esperar o fade terminar.
        }

        // Move o jogador para a posição do checkpoint
        playerTransform.position = currentCheckpointPosition;

        // Reseta a velocidade
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private System.Collections.IEnumerator ShowPenaltyMessage()
    {
        penaltyText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f); // Mostra por 3 segundos
        penaltyText.gameObject.SetActive(false);
    }
}