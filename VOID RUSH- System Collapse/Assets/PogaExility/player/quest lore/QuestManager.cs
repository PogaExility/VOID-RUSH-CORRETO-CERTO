using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{

    public static QuestManager Instance { get; private set; }

    [Header("Refer�ncias de UI")]
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

    // DENTRO DO EndQuest()
    // Dentro do QuestManager.cs
    // Agora a fun��o aceita um par�metro para saber se a quest foi completada com sucesso ou n�o
    public void EndQuest(bool completedSuccessfully = true)
    {
        if (!IsQuestActive) return;
        IsQuestActive = false;
        UpdateQuestStatusUI();

        // S� salva os itens se a quest foi completada com sucesso
        if (completedSuccessfully)
        {
            Debug.Log("Quest finalizada com sucesso! Salvando itens.");
            FindAnyObjectByType<InventoryManager>()?.CommitTemporaryItems();
        }
        else
        {
            Debug.Log("Quest falhou! Itens tempor�rios ser�o perdidos.");
            // N�o faz nada, pois o RespawnManager j� chamou o ClearTemporaryItems
        }
    }

    private void UpdateQuestStatusUI()
    {
        if (questStatusText != null)
        {
            questStatusText.text = $"Quest is {(IsQuestActive ? "True" : "False")}";
            questStatusText.gameObject.SetActive(true); // Garante que o texto esteja vis�vel
        }
    }

 
}