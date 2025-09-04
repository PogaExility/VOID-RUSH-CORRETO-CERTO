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

    private Canvas mainCanvas; // <<-- Vamos guardar a refer�ncia do Canvas
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Guardamos a refer�ncia do Canvas para a convers�o matem�tica
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

    // >> A CORRE��O FINAL EST� AQUI <<
    public void OnDrag(PointerEventData eventData)
    {
        // Esta � a forma profissional de fazer um objeto de UI seguir o mouse.
        // Ele converte a posi��o da tela do mouse para a posi��o local dentro do Canvas,
        // respeitando qualquer escala ou modo de renderiza��o.

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform, // O ret�ngulo de refer�ncia
            eventData.position,                   // A posi��o do mouse na tela
            mainCanvas.worldCamera,               // A c�mera da UI
            out Vector2 localPosition);           // A posi��o convertida que vamos receber

        // Usamos a posi��o local convertida em vez da posi��o bruta do mouse.
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
                // A vers�o anterior n�o tinha esta fun��o, mas vamos precisar dela
                InventoryUI.Instance.RequestRedraw();
            }
        }
        else
        {
            // O OnDrop do SlotView vai cuidar da l�gica, mas o redesenho final
            // garante que este objeto �rf�o seja destru�do.
            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.RequestRedraw();
            }
        }
    }
}