using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InventoryGridView : MonoBehaviour, IPointerClickHandler
{
    [Header("Referências")]
    public InventoryManager inventoryManager;
    public Canvas mainCanvas;
    public RectTransform slotContainer;   // tem GridLayoutGroup
    public RectTransform itemContainer;   // onde ficam os ícones
    public GameObject slotPrefab;      // prefab de slot (com Image opcional)
    public InventoryItemView itemPrefab;      // prefab do item

    private readonly List<InventorySlotView> slotViews = new();
    private readonly Dictionary<string, InventoryItemView> itemsInView = new();
    private InventoryItemView ghostItemView;
    private Vector2 dragOffset; // em tela

    void Awake()
    {
        // pivôs/âncoras top-left
        (slotContainer.pivot, slotContainer.anchorMin, slotContainer.anchorMax) = (new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        (itemContainer.pivot, itemContainer.anchorMin, itemContainer.anchorMax) = (new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        CreateGhostItem();
    }

    void OnEnable()
    {
        if (inventoryManager == null) inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (mainCanvas == null) mainCanvas = GetComponentInParent<Canvas>();

        if (slotContainer == null || itemContainer == null || itemPrefab == null)
        {
            Debug.LogError("[InventoryGridView] Faltam refs: "
                + (slotContainer == null ? "slotContainer " : "")
                + (itemContainer == null ? "itemContainer " : "")
                + (itemPrefab == null ? "itemPrefab " : ""));
            enabled = false; return;
        }

        var gl = slotContainer.GetComponent<GridLayoutGroup>();
        if (gl == null)
        {
            Debug.LogError("[InventoryGridView] slotContainer precisa de GridLayoutGroup.");
            enabled = false; return;
        }

        if (inventoryManager == null)
        {
            Debug.LogError("[InventoryGridView] InventoryManager não encontrado na cena.");
            enabled = false; return;
        }

        // 🔒 garante que a estrutura do inventário existe
        inventoryManager.EnsureGridExists();

        inventoryManager.OnBackpackItemChanged += RedrawSlot;
        inventoryManager.OnItemHeld += UpdateGhostItem;

        GenerateSlotGrid();

        // ⏳ adia 1 frame pra garantir que todos Awakes rodaram
        StartCoroutine(DeferredRedraw());
    }

    System.Collections.IEnumerator DeferredRedraw()
    {
        yield return null; // espera 1 frame
        RedrawAllItems();
    }

    void OnDisable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnBackpackItemChanged -= RedrawSlot;
            inventoryManager.OnItemHeld -= UpdateGhostItem;
        }
    }

    void Update()
    {
        if (ghostItemView != null && ghostItemView.gameObject.activeSelf)
            ghostItemView.GetComponent<RectTransform>().position = (Vector2)Input.mousePosition + dragOffset;
    }

    // ---------- Slots ----------
    private void GenerateSlotGrid()
    {
        if (slotContainer.childCount > 0) return;

        slotViews.Clear();
        for (int i = 0; i < inventoryManager.gridWidth * inventoryManager.gridHeight; i++)
        {
            var go = Instantiate(slotPrefab, slotContainer);
            var sv = go.GetComponent<InventorySlotView>();
            if (sv == null) sv = go.AddComponent<InventorySlotView>();
            slotViews.Add(sv);
        }
    }

    // ---------- Itens ----------
    private void RedrawAllItems()
    {
        if (inventoryManager == null) { Debug.LogError("[InventoryGridView] inventoryManager nulo."); return; }
        if (itemPrefab == null) { Debug.LogError("[InventoryGridView] itemPrefab nulo."); return; }
        if (itemContainer == null) { Debug.LogError("[InventoryGridView] itemContainer nulo."); return; }

        // 🔒 de novo por segurança, em caso de re-enable
        inventoryManager.EnsureGridExists();

        // limpa antigos
        foreach (Transform child in itemContainer) Destroy(child.gameObject);

        var gl = slotContainer.GetComponent<GridLayoutGroup>();
        if (gl == null) { Debug.LogError("[InventoryGridView] GridLayoutGroup ausente no slotContainer."); return; }

        int W = inventoryManager.gridWidth;
        int H = inventoryManager.gridHeight;

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var item = inventoryManager.GetItemAt(x, y);   // aqui era onde estourava se a grid não existisse
                int count = inventoryManager.GetCountAt(x, y);
                if (item == null || count <= 0) continue;

                var iv = Instantiate(itemPrefab, itemContainer);
                iv.Render(item, x, y);
                iv.SetStackCount(count);

                Vector2 pos = new Vector2(
                    x * (gl.cellSize.x + gl.spacing.x),
                   -y * (gl.cellSize.y + gl.spacing.y)
                );
                iv.GetComponent<RectTransform>().anchoredPosition = pos;
            }
    }


    // redesenha UMA célula quando o manager avisa
    private void RedrawSlot(ItemSO item, int x, int y)
    {
        if (itemContainer == null || slotContainer == null) return;

        // remove antigo
        string key = $"{x},{y}";
        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            var iv = itemContainer.GetChild(i).GetComponent<InventoryItemView>();
            if (iv != null && iv.name == key) Destroy(iv.gameObject);
        }

        if (item == null) return;

        var gl = slotContainer.GetComponent<GridLayoutGroup>();
        var view = Instantiate(itemPrefab, itemContainer);
        view.name = key;
        view.Render(item, x, y);
        view.SetStackCount(inventoryManager.GetCountAt(x, y));

        Vector2 pos = new Vector2(
            x * (gl.cellSize.x + gl.spacing.x),
           -y * (gl.cellSize.y + gl.spacing.y)
        );
        view.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    // ---------- Ghost / drag ----------
    private void CreateGhostItem()
    {
        if (itemPrefab == null || mainCanvas == null) return;
        ghostItemView = Instantiate(itemPrefab, mainCanvas.transform);
        ghostItemView.name = "GhostItem";
        var cg = ghostItemView.GetComponent<CanvasGroup>();
        if (!cg) cg = ghostItemView.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        ghostItemView.gameObject.SetActive(false);
    }

    private void UpdateGhostItem(ItemSO item, int amount)
    {
        if (ghostItemView == null) return;

        if (item != null)
        {
            ghostItemView.gameObject.SetActive(true);
            ghostItemView.Render(item, -1, -1);
            ghostItemView.SetStackCount(amount);
        }
        else ghostItemView.gameObject.SetActive(false);
    }

    public void BeginHoldingItem(int x, int y, PointerEventData eventData)
    {
        if (inventoryManager.heldItem != null) return;

        bool half = eventData.button == PointerEventData.InputButton.Right;
        inventoryManager.StartHoldingItem(x, y, half);

        // calcula offset do mouse relativo ao canto TL do item
        var itemViewRT = itemsInView[$"{x},{y}"].GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        itemViewRT.GetWorldCorners(corners);      // 1 = TopLeft
        Vector2 tlWorld = corners[1];
        Vector2 tlScreen = mainCanvas.worldCamera == null
            ? tlWorld
            : RectTransformUtility.WorldToScreenPoint(mainCanvas.worldCamera, tlWorld);

        dragOffset = tlScreen - eventData.position;
    }

    // ---------- Click/place/drop ----------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryManager.heldItem == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            slotContainer, eventData.position, mainCanvas.worldCamera, out Vector2 local);

        var gl = slotContainer.GetComponent<GridLayoutGroup>();
        int x = Mathf.FloorToInt(local.x / (gl.cellSize.x + gl.spacing.x));
        int y = Mathf.FloorToInt(-local.y / (gl.cellSize.y + gl.spacing.y));

        bool outside = x < 0 || x >= inventoryManager.gridWidth || y < 0 || y >= inventoryManager.gridHeight;
        bool overUI = eventData.pointerCurrentRaycast.gameObject != null &&
                       eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<InventoryGridView>() != null;

        if (outside || !overUI) inventoryManager.DropHeldItemToWorld();
        else inventoryManager.PlaceHeldItem(x, y);
    }
}
