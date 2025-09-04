using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // N�o esque�a esta linha!

// O CanvasGroup � t�o essencial que podemos pedir para a Unity garanti-lo.
[RequireComponent(typeof(CanvasGroup))]
public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // -- REFER�NCIAS DO PREFAB (p�blicas para o Inspector) --
    public Image icon;
    public TextMeshProUGUI countText;

    // -- DADOS DE ESTADO (controlados pelo sistema) --
    // N�s escondemos o index, pois ele s� � definido pelo c�digo, n�o pelo dev no inspector
    [HideInInspector] public int originSlotIndex;

    // -- COMPONENTES INTERNOS (privados) --
    private CanvasGroup canvasGroup;

    // Awake � para pegar componentes que est�o no MESMO objeto.
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // A fun��o de desenhar, que � chamada pelo InventoryUI.
    public void Render(int fromSlot, ItemSO itemData, int count)
    {
        this.originSlotIndex = fromSlot;
        this.icon.sprite = itemData.itemIcon;
        this.countText.text = count > 1 ? count.ToString() : "";
    }

    // --- L�GICA DE INTERA��O ---

    // Quando o arraste COME�A:
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Pede a refer�ncia do Canvas diretamente para o chefe (InventoryManager).
        // Esta � a forma mais segura e expl�cita.
        Transform mainCanvasTransform = InventoryManager.Instance.GetMainCanvasTransform();

        // Linha de defesa contra setup errado
        if (mainCanvasTransform == null)
        {
            Debug.LogError("FATAL: Refer�ncia do 'Main Canvas Transform' n�o est� configurada no Inspector do InventoryManager!");
            return;
        }

        // 1. Sequestra o item
        transform.SetParent(mainCanvasTransform);
        // 2. Garante que ele fique por cima de tudo
        transform.SetAsLastSibling();
        // 3. Faz o item ficar "transparente" para o mouse, para que o mouse possa ver os slots embaixo.
        canvasGroup.blocksRaycasts = false;
    }

    // Enquanto ARRASTA:
    public void OnDrag(PointerEventData eventData)
    {
        // O item sequestrado simplesmente segue o ponteiro do mouse.
        transform.position = eventData.position;
    }

    // Quando o arraste TERMINA:
    public void OnEndDrag(PointerEventData eventData)
    {
        // O OnDrop do SlotView vai cuidar da troca se soltar em um slot v�lido.

        // O trabalho do ItemView aqui � apenas se limpar e avisar ao sistema para se redesenhar caso
        // o jogador tenha soltado o item fora de qualquer slot (devolvendo-o ao seu lugar).

        // 1. Permite que o item seja clic�vel novamente no futuro.
        canvasGroup.blocksRaycasts = true;

        // 2. Se o mouse N�O foi solto sobre um objeto da UI com o script SlotView,
        // ent�o o jogador soltou o item "no v�cuo".
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<SlotView>() == null)
        {
            // 3. Pede ao chefe (InventoryManager) para for�ar um redesenho.
            // O redesenho vai DESTRUIR este ItemView "�rf�o" e criar um novo
            // no seu slot de origem, efetivamente devolvendo-o.
            InventoryManager.Instance.RequestRedraw();
        }
    }
}