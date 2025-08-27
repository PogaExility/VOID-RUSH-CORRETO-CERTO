using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Configurações de Penalidade")]
    public int moneyPenalty = 50;
    [Header("Referências de UI (Opcional)")]
    public TextMeshProUGUI penaltyText;

    // --- MUDANÇA IMPORTANTE: Guardamos o SCRIPT do checkpoint, não só a posição ---
    private Checkpoint activeCheckpoint;

    private Vector3 initialSpawnPosition;
    private string initialSpawnSceneName;

    private InventoryManager inventoryManager;
    public void SetReturnPoint(Vector3 returnPosition, string returnSceneName)
    {
        // Desativa o checkpoint ativo para que o ponto de retorno tenha prioridade
        if (activeCheckpoint != null)
        {
            activeCheckpoint.Deactivate();
            activeCheckpoint = null; // Limpa a referência
        }

        // Define a posição e a cena para onde o jogador deve voltar se morrer na missão
        initialSpawnPosition = returnPosition;
        initialSpawnSceneName = returnSceneName;

        // Garante que os itens atuais sejam salvos
        inventoryManager?.CommitTemporaryItems();

        Debug.Log($"Ponto de retorno definido para a cena '{returnSceneName}'.");
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);

        if (penaltyText != null) penaltyText.gameObject.SetActive(false);
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            initialSpawnPosition = player.transform.position;
            initialSpawnSceneName = SceneManager.GetActiveScene().name;
            inventoryManager = player.GetComponent<InventoryManager>();
        }
    }

    // --- A FUNÇÃO MAIS IMPORTANTE E CORRIGIDA ---
    public void SetNewCheckpoint(Checkpoint newCheckpoint)
    {
        // Se o checkpoint clicado já for o ativo, não faz nada.
        if (newCheckpoint == activeCheckpoint) return;

        // 1. DESATIVA O ANTIGO: Se já existia um checkpoint ativo, manda ele se desativar.
        if (activeCheckpoint != null)
        {
            activeCheckpoint.Deactivate();
        }

        // 2. ATUALIZA PARA O NOVO: Guarda a referência do novo checkpoint como o ativo.
        activeCheckpoint = newCheckpoint;

        // 3. ATIVA O NOVO: Manda o novo checkpoint se ativar.
        if (activeCheckpoint != null)
        {
            activeCheckpoint.Activate();
        }

        // Avisa o InventoryManager para tornar os itens temporários em permanentes
        inventoryManager?.CommitTemporaryItems();

        Debug.Log($"Novo checkpoint ativado na cena '{SceneManager.GetActiveScene().name}'");
    }

    public void RespawnPlayer(Transform playerTransform)
    {
        // Pega a posição e cena do checkpoint ativo, ou a inicial se nenhum foi ativado
        Vector3 respawnPosition = activeCheckpoint != null ? activeCheckpoint.transform.position : initialSpawnPosition;
        string checkpointSceneName = activeCheckpoint != null ? activeCheckpoint.gameObject.scene.name : initialSpawnSceneName;

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName != checkpointSceneName)
        {
            // --- APLICA PENALIDADES ---
            inventoryManager?.ClearTemporaryItems();
            Debug.Log($"Jogador perdeu {moneyPenalty} de dinheiro.");
            if (penaltyText != null) StartCoroutine(ShowPenaltyMessage());

            // AQUI A LÓGICA DE RECARREGAR A CENA
        }

        playerTransform.position = respawnPosition;
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private System.Collections.IEnumerator ShowPenaltyMessage()
    {
        penaltyText.text = $"-{moneyPenalty} Dinheiro";
        penaltyText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        penaltyText.gameObject.SetActive(false);
    }
}