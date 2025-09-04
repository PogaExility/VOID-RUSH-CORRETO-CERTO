using UnityEngine;
using UnityEngine.EventSystems;

public class SlotView : MonoBehaviour, IDropHandler
{
    // A CORREÇÃO ESTÁ AQUI:
    // "public get" = outros scripts podem ler este valor.
    // "private set" = SÓ ESTE SCRIPT PODE MUDAR ESTE VALOR (através do Initialize).
    // Isso esconde a variável do Inspector e a protege de mudanças externas.
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