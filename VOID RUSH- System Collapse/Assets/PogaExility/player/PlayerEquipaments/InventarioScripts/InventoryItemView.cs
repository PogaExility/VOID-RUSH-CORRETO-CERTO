using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image itemIcon;
    public TextMeshProUGUI stackText;

    private ItemSO itemData;
    private int gridX, gridY;
    private InventoryGridView gridView;

    void Awake()
    {
        var rt = GetComponent<RectTransform>();
        (rt.pivot, rt.anchorMin, rt.anchorMax) = (new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        itemIcon.raycastTarget = false;
    }

    public void Render(ItemSO item, int x, int y)
    {
        itemData = item;
        gridX = x; gridY = y;
        itemIcon.sprite = item.itemIcon;   // campo correto no teu ItemSO
    }

    public void SetStackCount(int count)
    {
        stackText.text = count > 1 ? count.ToString() : "";
        stackText.gameObject.SetActive(count > 1);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gridView == null) gridView = FindFirstObjectByType<InventoryGridView>();
        gridView.BeginHoldingItem(gridX, gridY, eventData);
    }

    public void OnDrag(PointerEventData eventData) { /* movimento do ghost é controlado pelo GridView */ }
    public void OnEndDrag(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData)
    {
        // clique direito consome (poção etc.)
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var manager = FindFirstObjectByType<InventoryManager>();
            manager?.UseItem(gridX, gridY);
        }
    }
}
