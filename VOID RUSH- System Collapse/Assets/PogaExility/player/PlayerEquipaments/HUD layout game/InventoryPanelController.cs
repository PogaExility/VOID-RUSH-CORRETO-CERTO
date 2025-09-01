using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelController : MonoBehaviour
{
    public GameObject backpackPanel;
    public Button openBackpackButton;
    public SearchFilterBar searchFilterBar;

    void Awake()
    {
        openBackpackButton.onClick.AddListener(OpenBackpack);
        backpackPanel.SetActive(false); // Começa com a mochila fechada
    }

    public void OpenBackpack()
    {
        backpackPanel.SetActive(true);
        // TODO: Lógica para focar na barra de busca, se desejado
    }
}

// Este pode ser um script separado
public class SearchFilterBar : MonoBehaviour
{
    public InputField searchInput;
    public Button searchButton;
    public Button filterButton;

    void Awake()
    {
        searchButton.onClick.AddListener(ApplySearch);
        filterButton.onClick.AddListener(OpenFilterMenu);
    }

    private void ApplySearch()
    {
        string searchTerm = searchInput.text;
        Debug.Log("Buscando por: " + searchTerm);
        // TODO: Implementar a lógica de filtragem que esconde/mostra itens no InventoryGridView
    }

    private void OpenFilterMenu()
    {
        Debug.Log("Abrindo menu de filtros...");
        // TODO: Implementar um dropdown ou painel com as opções de filtro e ordenação
    }
}