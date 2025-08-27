using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Referências de UI")]
    public TextMeshProUGUI questStatusText;

    public bool IsQuestActive { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        UpdateQuestStatusUI();
    }

    public void StartQuest()
    {
        if (IsQuestActive) return;
        IsQuestActive = true;
        UpdateQuestStatusUI();
        Debug.Log("Quest iniciada!");
    }

    public void EndQuest()
    {
        if (!IsQuestActive) return;
        IsQuestActive = false;
        UpdateQuestStatusUI();
        Debug.Log("Quest finalizada!");

        FindAnyObjectByType<InventoryManager>()?.CommitTemporaryItems();
    }

    private void UpdateQuestStatusUI()
    {
        if (questStatusText != null)
        {
            questStatusText.text = $"Quest is {(IsQuestActive ? "True" : "False")}";
            questStatusText.gameObject.SetActive(true); // Garante que o texto esteja visível
        }
    }
}