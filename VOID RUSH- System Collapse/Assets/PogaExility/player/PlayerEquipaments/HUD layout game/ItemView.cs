using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public TextMeshProUGUI countText;

    [HideInInspector] public int originSlotIndex;

    private Canvas mainCanvas; // <<-- Vamos guardar a referência do Canvas
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Guardamos a referência do Canvas para a conversão matemática
        mainCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

    }

    public void Render(int fromSlot, ItemSO itemData, int count)
    {
        this.originSlotIndex = fromSlot;
        this.icon.sprite = itemData.itemIcon;
        this.countText.text = count > 1 ? count.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(mainCanvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    // >> A CORREÇÃO FINAL ESTÁ AQUI <<
    public void OnDrag(PointerEventData eventData)
    {
        // Esta é a forma profissional de fazer um objeto de UI seguir o mouse.
        // Ele converte a posição da tela do mouse para a posição local dentro do Canvas,
        // respeitando qualquer escala ou modo de renderização.

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform, // O retângulo de referência
            eventData.position,                   // A posição do mouse na tela
            mainCanvas.worldCamera,               // A câmera da UI
            out Vector2 localPosition);           // A posição convertida que vamos receber

        // Usamos a posição local convertida em vez da posição bruta do mouse.
        transform.localPosition = localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<SlotView>() == null)
        {
            // O InventoryUI precisa redesenhar. Precisamos garantir que ele existe.
            if (InventoryUI.Instance != null)
            {
                // A versão anterior não tinha esta função, mas vamos precisar dela
                InventoryUI.Instance.RequestRedraw();
            }
        }
        else
        {
            // O OnDrop do SlotView vai cuidar da lógica, mas o redesenho final
            // garante que este objeto órfão seja destruído.
            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.RequestRedraw();
            }
        }
    }
}