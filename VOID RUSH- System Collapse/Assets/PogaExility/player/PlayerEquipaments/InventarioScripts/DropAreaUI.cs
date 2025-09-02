using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DropAreaUI : MonoBehaviour, IDropHandler
{
    public InventoryManager inventoryManager;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventoryManager != null && inventoryManager.heldItem != null)
        {
            inventoryManager.DropHeldItemToWorld();
        }
        Hide();
    }
}