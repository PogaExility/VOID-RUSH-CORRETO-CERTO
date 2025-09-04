using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public TextMeshProUGUI countText;

    [HideInInspector] public int originSlotIndex;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Garante que o CanvasGroup exista para controlar os raycasts
        if (!TryGetComponent<CanvasGroup>(out canvasGroup))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Render(int fromSlot, ItemSO itemData, int count)
    {
        this.originSlotIndex = fromSlot;
        this.icon.sprite = itemData.itemIcon;
        this.countText.text = count > 1 ? count.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Pede a referência do Canvas diretamente para o chefe. É 100% seguro.
        Transform canvasTransform = InventoryManager.Instance.GetMainCanvasTransform();

        // Linha de defesa. Isso nunca deve acontecer se a montagem estiver certa.
        if (canvasTransform == null)
        {
            Debug.LogError("FATAL: Referência do 'Main Canvas Transform' não está configurada no Inspector do InventoryManager!");
            return;
        }

        transform.SetParent(canvasTransform);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) => transform.position = Input.mousePosition;

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<SlotView>() == null)
        {
            InventoryManager.Instance.OnInventoryChanged?.Invoke();
        }
    }
}