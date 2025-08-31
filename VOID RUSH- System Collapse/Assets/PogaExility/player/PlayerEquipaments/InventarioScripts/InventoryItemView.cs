using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class InventoryItemView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Overlay (opcional)")]
    public Image hoverOverlay;

    private RectTransform rectTransform;    // caixa do grid (NÃO gira)
    private CanvasGroup canvasGroup;

    // sprite no FILHO para girar sem deformar a caixa
    private Image spriteImage;
    private RectTransform spriteRT;

    private ItemSO itemData;
    private InventoryManager inventoryManager;
    private InventoryGridView gridView;
    private DropAreaUI dropArea;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // âncora/pivô top-left para casar com o grid
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 1f);

        if (hoverOverlay != null) hoverOverlay.gameObject.SetActive(false);

        inventoryManager = FindFirstObjectByType<InventoryManager>();
        gridView = FindFirstObjectByType<InventoryGridView>();

        // GARANTE que o pai NÃO desenha nada (evita fundo branco)
        var parentImg = GetComponent<Image>();
        if (parentImg != null) parentImg.enabled = false;

        EnsureSpriteChild();
    }

    // cria/pega um filho "Sprite" com Image (é ele que gira)
    private void EnsureSpriteChild()
    {
        var child = transform.Find("Sprite");
        if (child == null)
        {
            var go = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            child = go.transform;
        }

        spriteRT = child as RectTransform;
        spriteImage = child.GetComponent<Image>();

        // o filho ocupa a caixa toda; preserveAspect evita achatamento
        spriteRT.anchorMin = Vector2.zero;
        spriteRT.anchorMax = Vector2.one;
        spriteRT.offsetMin = Vector2.zero;
        spriteRT.offsetMax = Vector2.zero;

        spriteImage.preserveAspect = true;
        spriteImage.raycastTarget = true;   // o filho recebe o raycast

        // adiciona um proxy que repassa os eventos do filho para ESTE script (no pai)
        var proxy = child.GetComponent<PointerForwarder>();
        if (proxy == null) proxy = child.gameObject.AddComponent<PointerForwarder>();
        proxy.parent = this;
    }

    /// <summary>Render com steps 0..3 (0°,90°,180°,270°) sem achatar.</summary>
    public void Render(ItemSO item, float cellSize, int rotationSteps)
    {
        itemData = item;

        spriteImage.sprite = item.itemIcon;
        spriteImage.preserveAspect = true;

        // Troca LxA só quando 90/270 (steps ímpares)
        bool swap = (rotationSteps & 1) == 1;
        int w = swap ? item.height : item.width;
        int h = swap ? item.width : item.height;
        rectTransform.sizeDelta = new Vector2(w * cellSize, h * cellSize);

        // Gira o FILHO (não o pai) para não distorcer
        float angle = -90f * (rotationSteps & 3);
        spriteRT.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    public ItemSO GetItemData() => itemData;

    // ---------- UI events (recebidos via proxy do filho) ----------
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverOverlay != null) hoverOverlay.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverOverlay != null) hoverOverlay.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && inventoryManager.heldItem == null)
            gridView?.BeginHoldingItem(this, eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryManager.heldItem == null)
            gridView?.BeginHoldingItem(this, eventData);

        if (dropArea == null) dropArea = FindFirstObjectByType<DropAreaUI>();
        dropArea?.Show();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // o movimento do ghost é feito no GridView
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventoryManager.heldItem != null)
            gridView?.OnPointerClick(eventData);

        dropArea?.Hide();
    }

    // ===== Proxy colado no filho "Sprite" =====
    private class PointerForwarder : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [HideInInspector] public InventoryItemView parent;

        public void OnPointerEnter(PointerEventData e) => parent?.OnPointerEnter(e);
        public void OnPointerExit(PointerEventData e) => parent?.OnPointerExit(e);
        public void OnPointerClick(PointerEventData e) => parent?.OnPointerClick(e);
        public void OnBeginDrag(PointerEventData e) => parent?.OnBeginDrag(e);
        public void OnDrag(PointerEventData e) => parent?.OnDrag(e);
        public void OnEndDrag(PointerEventData e) => parent?.OnEndDrag(e);
    }
}
