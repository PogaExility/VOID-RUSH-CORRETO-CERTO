using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image itemIcon;
    public Text stackText;

    private ItemSO itemData;
    private int gridX, gridY;
    private InventoryGridView gridView;

    void Awake()
    {
        // Garante que o Parent e Children estão com os Pivots e Anchors corretos
        var rt = GetComponent<RectTransform>();
        (rt.pivot, rt.anchorMin, rt.anchorMax) = (new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

        // Assume que `itemIcon` e `stackText` são filhos diretos
        itemIcon.GetComponent<RectTransform>().raycastTarget = false;
    }

    public void Render(ItemSO item, int x, int y)
    {
        itemData = item;
        gridX = x;
        gridY = y;
        itemIcon.sprite = item.itemIcon;
    }

    public void SetStackCount(int count)
    {
        stackText.text = count > 1 ? count.ToString() : "";
        stackText.gameObject.SetActive(count > 1);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gridView == null) gridView = FindObjectOfType<InventoryGridView>();
        gridView.BeginHoldingItem(gridX, gridY, eventData);
    }

    public void OnDrag(PointerEventData eventData) { /* Controlado pelo GridView */ }
    public void OnEndDrag(PointerEventData eventData) { /* Controlado pelo GridView */ }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            FindObjectOfType<InventoryManager>().UseItem(gridX, gridY);
        }
    }
}