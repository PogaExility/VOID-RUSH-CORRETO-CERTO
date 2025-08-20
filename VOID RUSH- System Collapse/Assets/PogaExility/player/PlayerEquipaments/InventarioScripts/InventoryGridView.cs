using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridView : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste aqui o objeto que contém o InventoryManager (ex: o Jogador).")]
    public InventoryManager inventoryManager;

    [Header("Configuração Visual")]
    [Tooltip("O prefab da CÉLULA/SLOT semi-transparente.")]
    public GameObject slotPrefab;
    [Tooltip("O prefab da IMAGEM do item que ficará sobre a grade.")]
    public InventoryItemView itemPrefab;

    [Header("Containers da UI")]
    [Tooltip("O objeto que conterá o grid de células (deve ter um Grid Layout Group).")]
    public RectTransform slotContainer;
    [Tooltip("O objeto que conterá as imagens dos itens (NÃO deve ter um Grid Layout Group).")]
    public RectTransform itemContainer;

    private Dictionary<ItemSO, InventoryItemView> itemsInView = new Dictionary<ItemSO, InventoryItemView>();
    private bool isGridGenerated = false; // Flag para garantir que o grid de fundo seja criado só uma vez

    void Awake()
    {
        if (inventoryManager != null)
        {
            // Se inscreve nos eventos UMA ÚNICA VEZ
            inventoryManager.OnItemAdded += AddItemView;
            inventoryManager.OnItemRemoved += RemoveItemView;
        }
        else
        {
            Debug.LogError("Referência ao InventoryManager não foi definida no InventoryGridView!");
        }
    }

    void OnDestroy()
    {
        // Cancela a inscrição ao ser destruído para evitar erros
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= AddItemView;
            inventoryManager.OnItemRemoved -= RemoveItemView;
        }
    }

    // Chamado quando o objeto do inventário é ativado
    void OnEnable()
    {
        // A grade de fundo é gerada apenas na primeira vez que o inventário abre
        if (!isGridGenerated)
        {
            GenerateSlotGrid();
            isGridGenerated = true;
        }

        // Os itens visuais são redesenhados toda vez que o inventário abre
        RedrawAllItems();
    }

    private void GenerateSlotGrid()
    {
        GridLayoutGroup gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            Debug.LogError("O Slot Container precisa de um componente Grid Layout Group!");
            return;
        }

        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = inventoryManager.gridWidth;

        int totalCells = inventoryManager.gridWidth * inventoryManager.gridHeight;
        for (int i = 0; i < totalCells; i++)
        {
            Instantiate(slotPrefab, slotContainer);
        }
    }

    private void RedrawAllItems()
    {
        // Limpa apenas as imagens dos ITENS, não a grade de fundo
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
        itemsInView.Clear();

        if (inventoryManager != null)
        {
            inventoryManager.RedrawAllItems();
        }
    }

    public void AddItemView(ItemSO item, int x, int y)
    {
        if (itemPrefab == null) return;

        float cellSize = slotContainer.GetComponent<GridLayoutGroup>().cellSize.x;

        InventoryItemView newItemView = Instantiate(itemPrefab, itemContainer);
        newItemView.Render(item, cellSize);

        RectTransform rt = newItemView.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);

        itemsInView[item] = newItemView;
    }

    public void RemoveItemView(ItemSO item)
    {
        if (itemsInView.ContainsKey(item))
        {
            Destroy(itemsInView[item].gameObject);
            itemsInView.Remove(item);
        }
    }
}