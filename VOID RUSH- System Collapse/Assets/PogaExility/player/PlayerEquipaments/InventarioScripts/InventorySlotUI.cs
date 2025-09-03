using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Componentes Visuais")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;

    // Referências
    private InventoryManager inventoryManager;
    private int slotIndex;
    private InventoryUI inventoryUI;

    public void Initialize(InventoryManager manager, InventoryUI uiController, int index)
    {
        inventoryManager = manager;
        inventoryUI = uiController;
        slotIndex = index;
    }

    public void Refresh()
    {
        if (inventoryManager == null) return;
        InventorySlot dataSlot = inventoryManager.GetBackpackSlot(slotIndex);
        bool hasItem = dataSlot != null && dataSlot.item != null && dataSlot.count > 0;

        // Atualizado para a nova função helper
        bool isHeldByMouse = inventoryUI.IsItemHeld() && inventoryUI.GetOriginIndex() == slotIndex;
        var tempColor = iconImage.color;
        tempColor.a = isHeldByMouse ? 0f : 1f; // Fica totalmente invisível se estiver no mouse
        iconImage.color = tempColor;

        if (hasItem && !isHeldByMouse)

        {
            iconImage.enabled = true;
            iconImage.sprite = dataSlot.item.itemIcon;
            bool showCount = dataSlot.count > 1;
            countText.enabled = showCount;
            if (showCount) countText.text = dataSlot.count.ToString();
        }
        else
        {
            iconImage.enabled = false;
            iconImage.enabled = false;
            countText.enabled = false;
        }
    }

    // LÓGICA DO CLICAR-E-CLICAR
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        inventoryUI.OnSlotClicked(slotIndex);
    }

    // LÓGICA DO SEGURAR-E-ARRASTAR
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        inventoryUI.OnSlotBeginDrag(slotIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        inventoryUI.OnSlotEndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        inventoryUI.OnSlotDrop(slotIndex);
    }
}