using UnityEngine;
using UnityEngine.EventSystems;

public class DropAreaUI : MonoBehaviour, IDropHandler
{
    [Header("Referências")]
    [Tooltip("Arraste aqui o objeto que contém o InventoryManager (ex: o Jogador).")]
    public InventoryManager inventoryManager;

    private CanvasGroup canvasGroup; // Usado para controlar a visibilidade e interação

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Começa invisível e não interativo
        Hide();
    }

    // Função pública para ser chamada quando o arrasto de um item COMEÇA
    public void Show()
    {
        canvasGroup.alpha = 1f; // Torna visível
        canvasGroup.blocksRaycasts = true; // Permite que detecte o "soltar" do mouse
    }

    // Função pública para ser chamada quando o arrasto de um item TERMINA
    public void Hide()
    {
        canvasGroup.alpha = 0f; // Torna invisível
        canvasGroup.blocksRaycasts = false; // Impede que detecte cliques quando invisível
    }

    // Chamada automaticamente quando um item é solto sobre esta área
    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemView itemView = eventData.pointerDrag.GetComponent<InventoryItemView>();

        if (itemView != null)
        {
            // Avisa ao cérebro para "dropar" este item
            inventoryManager.DropItemFromUI(itemView.GetItemData());
        }
    }
}