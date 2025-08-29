using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    // DENTRO DO EndQuest()
    // Dentro do QuestManager.cs
    // Agora a função aceita um parâmetro para saber se a quest foi completada com sucesso ou não
    public void EndQuest(bool completedSuccessfully = true)
    {
        if (!IsQuestActive) return;
        IsQuestActive = false;
        UpdateQuestStatusUI();

        // Só salva os itens se a quest foi completada com sucesso
        if (completedSuccessfully)
        {
            Debug.Log("Quest finalizada com sucesso! Salvando itens.");
            FindAnyObjectByType<InventoryManager>()?.CommitTemporaryItems();
        }
        else
        {
            Debug.Log("Quest falhou! Itens temporários serão perdidos.");
            // Não faz nada, pois o RespawnManager já chamou o ClearTemporaryItems
        }
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