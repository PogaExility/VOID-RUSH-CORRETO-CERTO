using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridView : MonoBehaviour
{
    [Header("Referências")]
    public InventoryManager inventoryManager;

    [Header("Configuração Visual")]
    public InventorySlotView slotPrefab; // MUDOU DE GameObject PARA InventorySlotView
    public InventoryItemView itemPrefab;

    [Header("Containers da UI")]
    public RectTransform slotContainer;
    public RectTransform itemContainer;

    // Lista para guardar as referências de todas as células visuais
    private List<InventorySlotView> slotViews = new List<InventorySlotView>();
    private Dictionary<ItemSO, InventoryItemView> itemsInView = new Dictionary<ItemSO, InventoryItemView>();

    void Awake()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded += AddItemView;
            inventoryManager.OnItemRemoved += RemoveItemView;
        }
    }

    void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= AddItemView;
            inventoryManager.OnItemRemoved -= RemoveItemView;
        }
    }

    void OnEnable()
    {
        if (!isGridGenerated)
        {
            GenerateSlotGrid();
        }
        RedrawAllItems();
    }

    // ===== LÓGICA DE GERAR O GRID E GUARDAR REFERÊNCIAS =====
    private bool isGridGenerated = false;
    private void GenerateSlotGrid()
    {
        GridLayoutGroup gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null || slotPrefab == null) return;

        slotViews.Clear(); // Limpa a lista antes de gerar
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = inventoryManager.gridWidth;

        int totalCells = inventoryManager.gridWidth * inventoryManager.gridHeight;
        for (int i = 0; i < totalCells; i++)
        {
            InventorySlotView newSlot = Instantiate(slotPrefab, slotContainer);
            slotViews.Add(newSlot); // Adiciona a célula à nossa lista de controle
        }
        isGridGenerated = true;
    }

    // ===== LÓGICA DE ATUALIZAR AS CORES E DESENHAR OS ITENS =====
    private void RedrawAllItems()
    {
        // 1. Limpa todas as imagens de itens
        foreach (Transform child in itemContainer) Destroy(child.gameObject);
        itemsInView.Clear();

        // 2. Reseta a cor de todas as células para "vazio"
        foreach (var slot in slotViews) slot.SetState(false);

        // 3. Pede para o cérebro redesenhar cada item
        if (inventoryManager != null)
        {
            inventoryManager.RedrawAllItems();
        }
    }

    public void AddItemView(ItemSO item, int x, int y)
    {
        // Cria a imagem do item no ItemContainer
        float cellSize = slotContainer.GetComponent<GridLayoutGroup>().cellSize.x;
        InventoryItemView newItemView = Instantiate(itemPrefab, itemContainer);
        newItemView.Render(item, cellSize);
        RectTransform rt = newItemView.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
        itemsInView[item] = newItemView;

        // Atualiza a cor das células ocupadas
        UpdateSlotColors(x, y, item.width, item.height, true);
    }

    public void RemoveItemView(ItemSO item)
    {
        // Encontra a posição do item para saber quais células limpar
        if (inventoryManager.FindItemPosition(item, out int x, out int y))
        {
            // Reseta a cor das células para "vazio"
            UpdateSlotColors(x, y, item.width, item.height, false);
        }

        // Remove a imagem do item
        if (itemsInView.ContainsKey(item))
        {
            if (itemsInView[item] != null) Destroy(itemsInView[item].gameObject);
            itemsInView.Remove(item);
        }
    }

    // Função auxiliar para mudar a cor das células
    private void UpdateSlotColors(int startX, int startY, int width, int height, bool isOccupied)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (startY + y) * inventoryManager.gridWidth + (startX + x);
                if (index < slotViews.Count)
                {
                    slotViews[index].SetState(isOccupied);
                }
            }
        }
    }
}