using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class InventoryGridView : MonoBehaviour, IPointerClickHandler
{
    [Header("Referências")]
    public InventoryManager inventoryManager;
    public RectTransform slotContainer;   // tem GridLayoutGroup
    public RectTransform itemContainer;   // itens fixos

    [Header("Prefabs")]
    public InventorySlotView slotPrefab;
    public InventoryItemView itemPrefab;

    private readonly List<InventorySlotView> slotViews = new();
    private readonly Dictionary<ItemSO, InventoryItemView> itemsInView = new();

    // guarda a rotação VISUAL (0..3) dos itens já colocados no grid
    private readonly Dictionary<ItemSO, int> itemVisualSteps = new();

    private InventoryItemView heldItemView;
    private int heldRotationSteps = 0;      // 0..3 (0/90/180/270)
    private Vector2 dragOffset = Vector2.zero; // em tela

    private bool isGridGenerated = false;
    private GridLayoutGroup gridLayout;
    private float cellSize = 0f;
    private Canvas mainCanvas;

    void Awake()
    {
        GetComponent<Image>().raycastTarget = true;

        gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        mainCanvas = GetComponentInParent<Canvas>();

        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded += AddItemView;
            inventoryManager.OnItemRemoved += RemoveItemView;
            inventoryManager.OnItemHeld += OnManagerItemHeld;
        }
    }

    void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= AddItemView;
            inventoryManager.OnItemRemoved -= RemoveItemView;
            inventoryManager.OnItemHeld -= OnManagerItemHeld;
        }
    }

    void OnEnable()
    {
        if (!isGridGenerated) GenerateSlotGrid();
        RedrawAllItems();
    }

    // ---------- Helpers de célula (considera padding/spacing/pivot) ----------
    private bool TryLocalToCell(Vector2 localInSlots, out int cx, out int cy)
    {
        var gl = gridLayout;
        var rt = slotContainer;
        var pad = gl.padding;

        float stepX = gl.cellSize.x + gl.spacing.x;
        float stepY = gl.cellSize.y + gl.spacing.y;

        float tlX = -rt.rect.width * rt.pivot.x + pad.left;
        float tlY = rt.rect.height * (1f - rt.pivot.y) - pad.top;

        float dx = localInSlots.x - tlX;
        float dy = tlY - localInSlots.y;

        if (dx < 0f || dy < 0f) { cx = cy = -1; return false; }

        cx = Mathf.FloorToInt(dx / stepX);
        cy = Mathf.FloorToInt(dy / stepY);

        if (cx < 0 || cx >= inventoryManager.gridWidth ||
            cy < 0 || cy >= inventoryManager.gridHeight) return false;

        return true;
    }

    private Vector2 CellTopLeftLocal(int x, int y)
    {
        var gl = gridLayout;
        var rt = slotContainer;
        var pad = gl.padding;

        float stepX = gl.cellSize.x + gl.spacing.x;
        float stepY = gl.cellSize.y + gl.spacing.y;

        float tlX = -rt.rect.width * rt.pivot.x + pad.left;
        float tlY = rt.rect.height * (1f - rt.pivot.y) - pad.top;

        return new Vector2(tlX + x * stepX, tlY - y * stepY);
    }

    // ---------- Segurar/arrastar/rotacionar ----------
    private void OnManagerItemHeld(ItemSO item, bool isRotatedFromManager)
    {
        if (heldItemView == null)
        {
            heldRotationSteps = isRotatedFromManager ? 1 : 0; // base
            heldItemView = Instantiate(itemPrefab, mainCanvas.transform);
            heldItemView.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        heldItemView.Render(item, cellSize, heldRotationSteps);

        if (cellSize > 0f)
            heldItemView.GetComponent<RectTransform>().position =
                (Vector2)Input.mousePosition - dragOffset;
    }

    public void BeginHoldingItem(InventoryItemView clickedItemView, PointerEventData eventData)
    {
        if (inventoryManager.heldItem != null) return;

        var rt = clickedItemView.GetComponent<RectTransform>();
        dragOffset = eventData.position - (Vector2)rt.position;

        // ao pegar do grid, se já tínhamos rotação visual salva, usa como base
        var data = clickedItemView.GetItemData();
        if (itemVisualSteps.TryGetValue(data, out int savedSteps))
            heldRotationSteps = savedSteps;

        inventoryManager.PickUpItemFromGrid(data);
    }

    void Update()
    {
        if (inventoryManager.heldItem == null) return;

        if (heldItemView != null)
        {
            heldItemView.GetComponent<RectTransform>().position =
                (Vector2)Input.mousePosition - dragOffset;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            heldRotationSteps = (heldRotationSteps + 1) & 3; // 0..3
            heldItemView.Render(inventoryManager.heldItem, cellSize, heldRotationSteps);
        }

        UpdateSlotPreview();
    }

    private void UpdateSlotPreview()
    {
        foreach (var s in slotViews) s.ResetColor();
        if (cellSize <= 0f || inventoryManager.heldItem == null || heldItemView == null) return;

        // usa o canto superior-esquerdo real do ghost
        var rt = heldItemView.GetComponent<RectTransform>();
        Vector3[] wc = new Vector3[4];
        rt.GetWorldCorners(wc); // 0=BL,1=TL,2=TR,3=BR
        Vector2 tlScreen = RectTransformUtility.WorldToScreenPoint(null, wc[1]);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                slotContainer, tlScreen, null, out Vector2 localPoint))
            return;

        if (!TryLocalToCell(localPoint, out int x, out int y)) return;

        var item = inventoryManager.heldItem;
        bool swap = (heldRotationSteps & 1) == 1;
        int w = swap ? item.height : item.width;
        int h = swap ? item.width : item.height;
        bool can = inventoryManager.CanPlaceItem(x, y, w, h);

        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int cx = x + i, cy = y + j;
                if (cx >= 0 && cx < inventoryManager.gridWidth &&
                    cy >= 0 && cy < inventoryManager.gridHeight)
                {
                    int idx = cy * inventoryManager.gridWidth + cx;
                    if (idx < slotViews.Count) slotViews[idx].SetPreviewColor(can);
                }
            }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryManager.heldItem == null || heldItemView == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // canto TL do ghost
        var rt = heldItemView.GetComponent<RectTransform>();
        Vector3[] wc = new Vector3[4];
        rt.GetWorldCorners(wc);
        Vector2 tlScreen = RectTransformUtility.WorldToScreenPoint(null, wc[1]);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                slotContainer, tlScreen, null, out Vector2 localPoint))
            return;

        if (!TryLocalToCell(localPoint, out int x, out int y)) return;

        // o Manager só entende “paridade” (deitado vs em pé). Sincroniza se precisar:
        int neededParity = (heldRotationSteps & 1);
        int managerParity = inventoryManager.isHeldItemRotated ? 1 : 0;
        if (neededParity != managerParity) inventoryManager.RotateHeldItem();

        // salva a rotação VISUAL que queremos manter após soltar
        itemVisualSteps[inventoryManager.heldItem] = heldRotationSteps;

        if (inventoryManager.PlaceHeldItem(x, y))
        {
            if (heldItemView != null) Destroy(heldItemView.gameObject);
            heldItemView = null;
        }
    }

    // ---------- Grid e render fixo ----------
    private void GenerateSlotGrid()
    {
        if (gridLayout == null || slotPrefab == null) return;

        slotViews.Clear();
        foreach (Transform c in slotContainer) Destroy(c.gameObject);

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = inventoryManager.gridWidth;

        cellSize = gridLayout.cellSize.x;

        int total = inventoryManager.gridWidth * inventoryManager.gridHeight;
        for (int i = 0; i < total; i++)
        {
            var s = Instantiate(slotPrefab, slotContainer).GetComponent<InventorySlotView>();
            slotViews.Add(s);
        }
        isGridGenerated = true;
    }

    private void RedrawAllItems()
    {
        foreach (var v in itemsInView.Values) if (v != null) Destroy(v.gameObject);
        itemsInView.Clear();
        foreach (var s in slotViews) s.SetState(false);
        inventoryManager?.RedrawAllItems();
    }

    public void AddItemView(ItemSO item, int x, int y, bool isRotated)
    {
        if (cellSize <= 0f) return;

        var v = Instantiate(itemPrefab, itemContainer);

        // usa steps salvos (0..3); se não houver, cai no bool do manager (0/1)
        int steps = itemVisualSteps.TryGetValue(item, out int s) ? s : (isRotated ? 1 : 0);
        v.Render(item, cellSize, steps);

        var rt = v.GetComponent<RectTransform>();
        rt.anchoredPosition = CellTopLeftLocal(x, y);

        itemsInView[item] = v;

        int w = (steps & 1) == 1 ? item.height : item.width;
        int h = (steps & 1) == 1 ? item.width : item.height;

        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = (y + j) * inventoryManager.gridWidth + (x + i);
                if (idx < slotViews.Count) slotViews[idx].SetState(true);
            }
    }

    public void RemoveItemView(ItemSO item)
    {
        if (itemsInView.TryGetValue(item, out var v))
        {
            if (v != null) Destroy(v.gameObject);
            itemsInView.Remove(item);
        }
        // opcional: manter a rotação visual no dicionário quando pegar?
        // se quiser limpar ao remover definitivamente:
        // itemVisualSteps.Remove(item);

        RedrawAllItems();
    }
}
