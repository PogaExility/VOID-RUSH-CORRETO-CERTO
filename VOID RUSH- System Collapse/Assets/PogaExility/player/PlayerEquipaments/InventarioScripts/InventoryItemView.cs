using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image), typeof(RectTransform), typeof(CanvasGroup))]
public class InventoryItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image itemImage;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private ItemSO itemData;
    private Transform originalParent;
    private Vector2 originalPosition;

    private DropAreaUI dropArea; // Referência para a lixeira

    void Awake()
    {
        itemImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
    }

    public void Render(ItemSO item, float cellSize) { itemData = item; itemImage.sprite = item.itemIcon; rectTransform.sizeDelta = new Vector2(item.width * cellSize, item.height * cellSize); }
    public ItemSO GetItemData() { return itemData; }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Encontra a lixeira no momento do arrasto
        if (dropArea == null) dropArea = FindFirstObjectByType<DropAreaUI>();

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;

        if (dropArea != null) dropArea.Show(); // MOSTRA A LIXEIRA
    }

    public void OnDrag(PointerEventData eventData) { transform.position = eventData.position; }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        // Se não foi solto em uma área de drop válida (como a lixeira), volta para a posição original
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<DropAreaUI>() == null)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }

        if (dropArea != null) dropArea.Hide(); // ESCONDE A LIXEIRA
    }
}