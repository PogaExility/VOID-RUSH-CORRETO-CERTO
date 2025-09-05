using UnityEngine;
using UnityEngine.EventSystems;

public class SlotView : MonoBehaviour, IPointerDownHandler
{
    public int slotIndex { get; private set; }

    public void Initialize(int index) => this.slotIndex = index;

    public void OnPointerDown(PointerEventData eventData)
    {
        InventoryManager.Instance.OnSlotClicked(this.slotIndex, eventData.button);
    }
}