using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Painéis")]
    [Tooltip("Arraste aqui o painel da Maleta que é filho deste objeto.")]
    public GameObject briefcasePanel; // A sua "Maleta"

    // Adicione outros painéis aqui no futuro (ex: statsPanel, descriptionPanel)

    // Chamado quando o Panel Inventário é ativado
    void OnEnable()
    {
        // Garante que, ao abrir o inventário, a maleta seja o primeiro painel a ser mostrado.
        if (briefcasePanel != null)
        {
            briefcasePanel.SetActive(true);
        }
    }

    // Você pode adicionar funções aqui para trocar de abas no futuro
    // public void ShowStatsPanel() { ... }
    // public void ShowBriefcasePanel() { ... }
}