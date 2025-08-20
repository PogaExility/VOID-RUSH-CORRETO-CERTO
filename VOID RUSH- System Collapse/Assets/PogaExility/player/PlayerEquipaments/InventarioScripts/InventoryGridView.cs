using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridView : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("Arraste aqui o objeto que cont�m o InventoryManager (ex: o Jogador).")]
    public InventoryManager inventoryManager;

    [Header("Configura��o Visual")]
    [Tooltip("O prefab da C�LULA/SLOT semi-transparente.")]
    public GameObject slotPrefab;
    [Tooltip("O prefab da IMAGEM do item que ficar� sobre a grade.")]
    public InventoryItemView itemPrefab;

    [Header("Containers da UI")]
    [Tooltip("O objeto que conter� o grid de c�lulas (deve ter um Grid Layout Group).")]
    public RectTransform slotContainer;
    [Tooltip("O objeto que conter� as imagens dos itens (N�O deve ter um Grid Layout Group).")]
    public RectTransform itemContainer;

    private Dictionary<ItemSO, InventoryItemView> itemsInView = new Dictionary<ItemSO, InventoryItemView>();
    private bool isGridGenerated = false; // Flag para garantir que o grid de fundo seja criado s� uma vez

    void Awake()
    {
        if (inventoryManager != null)
        {
            // Se inscreve nos eventos UMA �NICA VEZ
            inventoryManager.OnItemAdded += AddItemView;
            inventoryManager.OnItemRemoved += RemoveItemView;
        }
        else
        {
            Debug.LogError("Refer�ncia ao InventoryManager n�o foi definida no InventoryGridView!");
        }
    }

    void OnDestroy()
    {
        // Cancela a inscri��o ao ser destru�do para evitar erros
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= AddItemView;
            inventoryManager.OnItemRemoved -= RemoveItemView;
        }
    }

    // Chamado quando o objeto do invent�rio � ativado
    void OnEnable()
    {
        // A grade de fundo � gerada apenas na primeira vez que o invent�rio abre
        if (!isGridGenerated)
        {
            GenerateSlotGrid();
            isGridGenerated = true;
        }

        // Os itens visuais s�o redesenhados toda vez que o invent�rio abre
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
        // Limpa apenas as imagens dos ITENS, n�o a grade de fundo
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