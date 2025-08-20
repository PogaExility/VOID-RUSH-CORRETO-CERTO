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

        // ===== IN�CIO DA CORRE��O =====
        // Usando o m�todo novo e recomendado pela Unity
        dropArea = FindFirstObjectByType<DropAreaUI>();
        // ===== FIM DA CORRE��O =====
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
        // A l�gica de reposicionamento foi removida. O sistema de eventos cuidar� disso.
        // Se soltar em um local inv�lido, o item permanecer� no invent�rio l�gico.
        // A UI pode redesenhar tudo para corrigir a posi��o visual se necess�rio.

        canvasGroup.blocksRaycasts = true;

        if (dropArea != null)
        {
            dropArea.Hide();
        }

        // Se o item n�o foi solto na lixeira ou de volta na maleta,
        // o `InventoryManager` ainda o tem na lista. Ao reabrir o invent�rio,
        // o `RedrawAllItems` ir� coloc�-lo de volta no lugar certo.
        if (eventData.pointerEnter == null ||
            (eventData.pointerEnter.GetComponent<DropAreaUI>() == null &&
             eventData.pointerEnter.GetComponentInParent<InventoryGridView>() == null))
        {
            // Opcional: For�ar um redesenho para "snap back" imediato.
            // FindFirstObjectByType<InventoryGridView>()?.RedrawAllItems();
        }
    }
}