using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class InventoryGridView : MonoBehaviour, IPointerClickHandler
{
    [Header("Referências")]
    public InventoryManager inventoryManager;
    public RectTransform slotContainer;
    public RectTransform itemContainer;

    [Header("Prefabs")]
    public GameObject slotPrefab;
    public InventoryItemView itemPrefab;

    private Dictionary<ItemSO, InventoryItemView> itemsInView = new Dictionary<ItemSO, InventoryItemView>();
    private InventoryItemView heldItemView;
    private DropAreaUI dropArea; // Referência para a lixeira
    private bool isGridGenerated = false;

    void Awake()
    {
        GetComponent<Image>().raycastTarget = true;

        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded += AddItemView;
            inventoryManager.OnItemRemoved += RemoveItemView;
            inventoryManager.OnItemHeld += StartHoldingItemView;
        }
        else
        {
            Debug.LogError("ERRO: A referência ao 'InventoryManager' não foi definida no Inspector!", this.gameObject);
        }
    }

    void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= AddItemView;
            inventoryManager.OnItemRemoved -= RemoveItemView;
            inventoryManager.OnItemHeld -= StartHoldingItemView;
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

    void Update()
    {
        if (heldItemView != null)
        {
            heldItemView.transform.position = Input.mousePosition;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (heldItemView != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemContainer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            float cellSize = slotContainer.GetComponent<GridLayoutGroup>().cellSize.x;
            int x = Mathf.FloorToInt(localPoint.x / cellSize);
            int y = Mathf.FloorToInt(-localPoint.y / cellSize);

            if (inventoryManager.PlaceHeldItem(x, y))
            {
                Destroy(heldItemView.gameObject);
                heldItemView = null;
                if (dropArea != null) dropArea.Hide(); // Esconde a lixeira ao colocar o item
            }
            else
            {
                Debug.Log($"Não foi possível colocar o item na posição ({x},{y}).");
            }
        }
    }

    private void GenerateSlotGrid()
    {
        if (slotContainer == null || slotPrefab == null || inventoryManager == null) return;

        GridLayoutGroup gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null) return;

        foreach (Transform child in slotContainer) Destroy(child.gameObject);

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = inventoryManager.gridWidth;

        int totalCells = inventoryManager.gridWidth * inventoryManager.gridHeight;
        for (int i = 0; i < totalCells; i++)
        {
            Instantiate(slotPrefab, slotContainer);
        }
        isGridGenerated = true;
    }

    private void RedrawAllItems()
    {
        foreach (Transform child in itemContainer) Destroy(child.gameObject);
        itemsInView.Clear();

        if (inventoryManager != null)
        {
            inventoryManager.RedrawAllItems();
        }
    }

    private void StartHoldingItemView(ItemSO item)
    {
        if (dropArea == null) dropArea = FindFirstObjectByType<DropAreaUI>();

        if (heldItemView != null) Destroy(heldItemView.gameObject);

        heldItemView = Instantiate(itemPrefab, transform.root);
        float cellSize = slotContainer.GetComponent<GridLayoutGroup>().cellSize.x;
        heldItemView.Render(item, cellSize);

        if (heldItemView.GetComponent<CanvasGroup>() != null)
        {
            heldItemView.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        if (dropArea != null) dropArea.Show();
    }

    public void AddItemView(ItemSO item, int x, int y)
    {
        if (itemPrefab == null || itemContainer == null || slotContainer == null) return;

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
            if (itemsInView[item] != null) Destroy(itemsInView[item].gameObject);
            itemsInView.Remove(item);
        }
    }
}