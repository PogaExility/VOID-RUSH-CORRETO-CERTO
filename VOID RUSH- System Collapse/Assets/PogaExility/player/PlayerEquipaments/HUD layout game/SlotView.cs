using UnityEngine;
using UnityEngine.EventSystems;

public class SlotView : MonoBehaviour, IDropHandler
{
    // A CORRE��O EST� AQUI:
    // "public get" = outros scripts podem ler este valor.
    // "private set" = S� ESTE SCRIPT PODE MUDAR ESTE VALOR (atrav�s do Initialize).
    // Isso esconde a vari�vel do Inspector e a protege de mudan�as externas.
    public int slotIndex { get; private set; }

    public void Initialize(int index)
    {
        this.slotIndex = index;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var itemView = eventData.pointerDrag.GetComponent<ItemView>();
        if (itemView != null)
        {
            InventoryManager.Instance.SwapSlots(itemView.originSlotIndex, this.slotIndex);
        }
    }
}