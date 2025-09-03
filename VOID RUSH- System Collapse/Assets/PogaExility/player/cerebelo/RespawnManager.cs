using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Configura��es de Penalidade")]
    public int moneyPenalty = 50;
    [Header("Refer�ncias de UI (Opcional)")]
    public TextMeshProUGUI penaltyText;

    // Guarda a refer�ncia do SCRIPT do checkpoint ativo, n�o apenas a posi��o
    private Checkpoint activeCheckpoint;

    // Guarda a posi��o e cena iniciais como fallback
    private Vector3 initialSpawnPosition;
    private string initialSpawnSceneName;

    private InventoryManager inventoryManager;

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

    // --- ESTA � A FUN��O CORRETA ---
    // Ela recebe o Checkpoint inteiro como argumento.
    public void SetNewCheckpoint(Checkpoint newCheckpoint)
    {
        if (newCheckpoint == activeCheckpoint) return;

        // Se j� existia um checkpoint ativo, manda ele se desativar.
        if (activeCheckpoint != null)
        {
            activeCheckpoint.Deactivate();
        }

        // Guarda e ativa o novo checkpoint.
        activeCheckpoint = newCheckpoint;
        if (activeCheckpoint != null)
        {
            activeCheckpoint.Activate();
        }

       // inventoryManager?.CommitTemporaryItems();
        Debug.Log($"Novo checkpoint ativado na cena '{SceneManager.GetActiveScene().name}'");
    }

    public void SetReturnPoint(Vector3 returnPosition, string returnSceneName)
    {
        if (activeCheckpoint != null)
        {
            activeCheckpoint.Deactivate();
            activeCheckpoint = null;
        }
        initialSpawnPosition = returnPosition;
        initialSpawnSceneName = returnSceneName;
       // inventoryManager?.CommitTemporaryItems();
        Debug.Log($"Ponto de retorno salvo na cena '{returnSceneName}'.");
    }

    // Dentro do RespawnManager.cs
    public void RespawnPlayer(Transform playerTransform)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        string checkpointSceneName = activeCheckpoint != null ? activeCheckpoint.gameObject.scene.name : initialSpawnSceneName;

        // APLICA PENALIDADES APENAS SE A QUEST ESTIVER ATIVA
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestActive)
        {
            //inventoryManager?.ClearTemporaryItems();
            QuestManager.Instance.EndQuest(false);

            Debug.Log($"Jogador perdeu {moneyPenalty} de dinheiro.");
            if (penaltyText != null) StartCoroutine(ShowPenaltyMessage());

            SceneManager.LoadScene(checkpointSceneName);
        }
        else // Se a quest N�O est� ativa, apenas faz o respawn normal
        {
            playerTransform.position = activeCheckpoint != null ? activeCheckpoint.transform.position : initialSpawnPosition;
            Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    private System.Collections.IEnumerator ShowPenaltyMessage()
    {
        penaltyText.text = $"-{moneyPenalty} Dinheiro";
        penaltyText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        penaltyText.gameObject.SetActive(false);
    }
}