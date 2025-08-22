using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DropAreaUI : MonoBehaviour, IDropHandler
{
    [Header("Refer�ncias")]
    [Tooltip("Arraste aqui o objeto que cont�m o InventoryManager (ex: o Jogador).")]
    public InventoryManager inventoryManager;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
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

    // Chamada automaticamente quando um item � solto sobre esta �rea
    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemView itemView = eventData.pointerDrag.GetComponent<InventoryItemView>();
        if (itemView != null)
        {
            // Pega o item que est� sendo segurado pelo c�rebro e o joga fora
            if (inventoryManager.heldItem != null)
            {
                inventoryManager.DropHeldItem();
            }
        }
    }
}