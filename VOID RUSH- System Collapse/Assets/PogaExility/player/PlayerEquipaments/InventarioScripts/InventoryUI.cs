using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Pain�is")]
    [Tooltip("Arraste aqui o painel da Maleta que � filho deste objeto.")]
    public GameObject briefcasePanel; // A sua "Maleta"

    // Adicione outros pain�is aqui no futuro (ex: statsPanel, descriptionPanel)

    // Chamado quando o Panel Invent�rio � ativado
    void OnEnable()
    {
        // Garante que, ao abrir o invent�rio, a maleta seja o primeiro painel a ser mostrado.
        if (briefcasePanel != null)
        {
            briefcasePanel.SetActive(true);
        }
    }

    // Voc� pode adicionar fun��es aqui para trocar de abas no futuro
    // public void ShowStatsPanel() { ... }
    // public void ShowBriefcasePanel() { ... }
}