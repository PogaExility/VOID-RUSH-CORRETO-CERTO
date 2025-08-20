using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image itemImage;
    private RectTransform rectTransform;
    private ItemSO itemData;
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    private DropAreaUI dropArea;

    void Awake()
    {
        itemImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);

        // ===== INÍCIO DA CORREÇÃO =====
        // Usando o método novo e recomendado pela Unity
        dropArea = FindFirstObjectByType<DropAreaUI>();
        // ===== FIM DA CORREÇÃO =====
    }

    public void Render(ItemSO item, float cellSize)
    {
        itemData = item;
        itemImage.sprite = item.itemIcon;
        rectTransform.sizeDelta = new Vector2(item.width * cellSize, item.height * cellSize);
    }

    public ItemSO GetItemData()
    {
        return itemData;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;

        if (dropArea != null)
        {
            dropArea.Show();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        // A lógica de reposicionamento foi removida. O sistema de eventos cuidará disso.
        // Se soltar em um local inválido, o item permanecerá no inventário lógico.
        // A UI pode redesenhar tudo para corrigir a posição visual se necessário.

        canvasGroup.blocksRaycasts = true;

        if (dropArea != null)
        {
            dropArea.Hide();
        }

        // Se o item não foi solto na lixeira ou de volta na maleta,
        // o `InventoryManager` ainda o tem na lista. Ao reabrir o inventário,
        // o `RedrawAllItems` irá colocá-lo de volta no lugar certo.
        if (eventData.pointerEnter == null ||
            (eventData.pointerEnter.GetComponent<DropAreaUI>() == null &&
             eventData.pointerEnter.GetComponentInParent<InventoryGridView>() == null))
        {
            // Opcional: Forçar um redesenho para "snap back" imediato.
            // FindFirstObjectByType<InventoryGridView>()?.RedrawAllItems();
        }
    }
}