using UnityEngine;
using UnityEngine.EventSystems;

public class SlotView : MonoBehaviour, IDropHandler
{
    // A CORRE��O: "private set" esconde do Inspector e protege a vari�vel.
    public int slotIndex { get; private set; }

    public void Initialize(int index) => this.slotIndex = index;

    public void OnDrop(PointerEventData eventData)
    {
        var itemView = eventData.pointerDrag.GetComponent<ItemView>();
        if (itemView != null)
        {
            InventoryManager.Instance.SwapSlots(itemView.originSlotIndex, this.slotIndex);
        }
    }
}