using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InventoryGridView : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    [Header("Referências")]
    public InventoryManager inventoryManager;
    public Canvas mainCanvas;

    [Header("Containers (Pivot e Anchor Top-Left)")]
    public RectTransform slotContainer;
    public RectTransform itemContainer;

    [Header("Prefabs")]
    public GameObject slotPrefab;
    public InventoryItemView itemPrefab;

    [Header("UI Fantasma")]
    public InventoryItemView ghostItemView;

    public List<InventorySlotView> slotViews = new List<InventorySlotView>();
    private Dictionary<string, InventoryItemView> itemsInView = new Dictionary<string, InventoryItemView>();
    private Vector2 dragOffset;

    void Awake()
    {
        // Garante que o Pivot e Anchors estão corretos para o cálculo
        (slotContainer.pivot, slotContainer.anchorMin, slotContainer.anchorMax) = (new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        (itemContainer.pivot, itemContainer.anchorMin, itemContainer.anchorMax) = (new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
    }

    void OnEnable()
    {
        GenerateSlotGrid();
        inventoryManager.OnItemAdded += RedrawSlot;
        inventoryManager.OnItemRemoved += (item) => RedrawAllItems(); // Simples, redesenha tudo na remoção
        inventoryManager.OnItemHeld += OnManagerItemHeld;
        RedrawAllItems();
    }

    void OnDisable()
    {
        inventoryManager.OnItemAdded -= RedrawSlot;
        inventoryManager.OnItemRemoved -= (item) => RedrawAllItems();
        inventoryManager.OnItemHeld -= OnManagerItemHeld;
    }

    void Update()
    {
        if (ghostItemView.gameObject.activeSelf)
        {
            ghostItemView.GetComponent<RectTransform>().position = (Vector2)Input.mousePosition - dragOffset;
            UpdateSlotPreview();
        }
    }

    private void GenerateSlotGrid()
    {
        if (slotContainer.childCount > 0) return; // Já gerado

        var gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        for (int i = 0; i < inventoryManager.gridWidth * inventoryManager.gridHeight; i++)
        {
            var slotGO = Instantiate(slotPrefab, slotContainer);
            slotGO.name = $"Slot_{i % inventoryManager.gridWidth}_{i / inventoryManager.gridWidth}";
            var slotView = slotGO.GetComponent<InventorySlotView>();
            slotViews.Add(slotView);
        }
    }

    public void RedrawAllItems()
    {
        foreach (var itemView in itemsInView.Values)
        {
            if (itemView != null) Destroy(itemView.gameObject);
        }
        itemsInView.Clear();

        for (int y = 0; y < inventoryManager.gridHeight; y++)
        {
            for (int x = 0; x < inventoryManager.gridWidth; x++)
            {
                RedrawSlot(inventoryManager.GetItemAt(x, y), x, y, false);
            }
        }
    }

    private void RedrawSlot(ItemSO item, int x, int y, bool isEquipSlot)
    {
        if (isEquipSlot) return; // Ignora slots de equipamento por enquanto

        string key = $"{x},{y}";
        // Remove a view antiga se existir
        if (itemsInView.ContainsKey(key) && itemsInView[key] != null)
        {
            Destroy(itemsInView[key].gameObject);
            itemsInView.Remove(key);
        }

        // Se há um item, cria a nova view
        if (item != null)
        {
            var itemView = Instantiate(itemPrefab, itemContainer);
            itemView.Render(item, x, y);
            itemView.SetStackCount(inventoryManager.GetCountAt(x, y));

            var gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
            Vector2 position = new Vector2(x * (gridLayout.cellSize.x + gridLayout.spacing.x), -y * (gridLayout.cellSize.y + gridLayout.spacing.y));
            itemView.GetComponent<RectTransform>().anchoredPosition = position;

            itemsInView[key] = itemView;
        }
    }

    private void OnManagerItemHeld(ItemSO item)
    {
        if (item != null)
        {
            ghostItemView.gameObject.SetActive(true);
            ghostItemView.Render(item, -1, -1);
            ghostItemView.SetStackCount(1);
        }
        else
        {
            ghostItemView.gameObject.SetActive(false);
            ResetAllSlotPreviews();
        }
    }

    public void BeginHoldingItem(int x, int y, PointerEventData eventData)
    {
        inventoryManager.StartHoldingItem(x, y);

        // Calcula o offset do Top-Left
        var itemView = itemsInView[$"{x},{y}"];
        var rt = itemView.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners); // 0=BL, 1=TL, 2=TR, 3=BR
        Vector2 tlWorldPos = corners[1];
        dragOffset = eventData.position - (Vector2)RectTransformUtility.WorldToScreenPoint(mainCanvas.worldCamera, tlWorldPos);
    }

    private void UpdateSlotPreview()
    {
        ResetAllSlotPreviews();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(slotContainer, Input.mousePosition, mainCanvas.worldCamera, out localPoint);

        var gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        int x = Mathf.FloorToInt(localPoint.x / (gridLayout.cellSize.x + gridLayout.spacing.x));
        int y = Mathf.FloorToInt(-localPoint.y / (gridLayout.cellSize.y + gridLayout.spacing.y));

        if (x < 0 || x >= inventoryManager.gridWidth || y < 0 || y >= inventoryManager.gridHeight) return;

        ItemSO targetItem = inventoryManager.GetItemAt(x, y);
        bool canPlace = targetItem == null || (targetItem == inventoryManager.heldItem && inventoryManager.GetCountAt(x, y) < targetItem.maxStack);

        slotViews[y * inventoryManager.gridWidth + x].SetPreviewColor(canPlace);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryManager.heldItem == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(slotContainer, eventData.position, mainCanvas.worldCamera, out localPoint);

        var gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        int x = Mathf.FloorToInt(localPoint.x / (gridLayout.cellSize.x + gridLayout.spacing.x));
        int y = Mathf.FloorToInt(-localPoint.y / (gridLayout.cellSize.y + gridLayout.spacing.y));

        if (x < 0 || x >= inventoryManager.gridWidth || y < 0 || y >= inventoryManager.gridHeight)
        {
            inventoryManager.DropHeldItem(); // Clicou fora, tenta devolver
            return;
        }

        if (inventoryManager.PlaceHeldItemStack(x, y))
        {
            // Diminui a pilha "na mão" (neste modelo, sempre 1, então zera)
            inventoryManager.DropHeldItem(); // Lógica de drop agora decide se devolve ou some
        }
    }

    public void OnDrop(PointerEventData eventData) { /* Usamos IPointerClickHandler para mais controle */ }
    private void ResetAllSlotPreviews()
    {
        foreach (var slot in slotViews) slot.ResetColor();
    }
}