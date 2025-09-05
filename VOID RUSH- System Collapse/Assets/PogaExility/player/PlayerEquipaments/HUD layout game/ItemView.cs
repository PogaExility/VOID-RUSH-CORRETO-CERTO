using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public TextMeshProUGUI countText;

    [HideInInspector] public int originSlotIndex;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Render(int fromSlot, ItemSO itemData, int count)
    {
        this.originSlotIndex = fromSlot;
        this.icon.sprite = itemData.itemIcon;
        this.countText.text = count > 1 ? count.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Transform canvasTransform = InventoryManager.Instance.GetMainCanvasTransform();
        if (canvasTransform == null) return;

        transform.SetParent(canvasTransform);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) => transform.position = Input.mousePosition;

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<SlotView>() == null)
        {
            InventoryManager.Instance.RequestRedraw();
        }
    }
}