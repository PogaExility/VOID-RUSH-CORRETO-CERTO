using UnityEngine;
using UnityEngine.EventSystems;

public class DropAreaUI : MonoBehaviour, IDropHandler
{
    [Header("Refer�ncias")]
    [Tooltip("Arraste aqui o objeto que cont�m o InventoryManager (ex: o Jogador).")]
    public InventoryManager inventoryManager;

    private CanvasGroup canvasGroup; // Usado para controlar a visibilidade e intera��o

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Come�a invis�vel e n�o interativo
        Hide();
    }

    // Fun��o p�blica para ser chamada quando o arrasto de um item COME�A
    public void Show()
    {
        canvasGroup.alpha = 1f; // Torna vis�vel
        canvasGroup.blocksRaycasts = true; // Permite que detecte o "soltar" do mouse
    }

    // Fun��o p�blica para ser chamada quando o arrasto de um item TERMINA
    public void Hide()
    {
        canvasGroup.alpha = 0f; // Torna invis�vel
        canvasGroup.blocksRaycasts = false; // Impede que detecte cliques quando invis�vel
    }

    // Chamada automaticamente quando um item � solto sobre esta �rea
    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemView itemView = eventData.pointerDrag.GetComponent<InventoryItemView>();

        if (itemView != null)
        {
            // Avisa ao c�rebro para "dropar" este item
            inventoryManager.DropItemFromUI(itemView.GetItemData());
        }
    }
}