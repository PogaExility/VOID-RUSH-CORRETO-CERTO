using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Não esqueça esta linha!

// O CanvasGroup é tão essencial que podemos pedir para a Unity garanti-lo.
[RequireComponent(typeof(CanvasGroup))]
public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // -- REFERÊNCIAS DO PREFAB (públicas para o Inspector) --
    public Image icon;
    public TextMeshProUGUI countText;

    // -- DADOS DE ESTADO (controlados pelo sistema) --
    // Nós escondemos o index, pois ele só é definido pelo código, não pelo dev no inspector
    [HideInInspector] public int originSlotIndex;

    // -- COMPONENTES INTERNOS (privados) --
    private CanvasGroup canvasGroup;

    // Awake é para pegar componentes que estão no MESMO objeto.
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // A função de desenhar, que é chamada pelo InventoryUI.
    public void Render(int fromSlot, ItemSO itemData, int count)
    {
        this.originSlotIndex = fromSlot;
        this.icon.sprite = itemData.itemIcon;
        this.countText.text = count > 1 ? count.ToString() : "";
    }

    // --- LÓGICA DE INTERAÇÃO ---

    // Quando o arraste COMEÇA:
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Pede a referência do Canvas diretamente para o chefe (InventoryManager).
        // Esta é a forma mais segura e explícita.
        Transform mainCanvasTransform = InventoryManager.Instance.GetMainCanvasTransform();

        // Linha de defesa contra setup errado
        if (mainCanvasTransform == null)
        {
            Debug.LogError("FATAL: Referência do 'Main Canvas Transform' não está configurada no Inspector do InventoryManager!");
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
        // O OnDrop do SlotView vai cuidar da troca se soltar em um slot válido.

        // O trabalho do ItemView aqui é apenas se limpar e avisar ao sistema para se redesenhar caso
        // o jogador tenha soltado o item fora de qualquer slot (devolvendo-o ao seu lugar).

        // 1. Permite que o item seja clicável novamente no futuro.
        canvasGroup.blocksRaycasts = true;

        // 2. Se o mouse NÃO foi solto sobre um objeto da UI com o script SlotView,
        // então o jogador soltou o item "no vácuo".
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<SlotView>() == null)
        {
            // 3. Pede ao chefe (InventoryManager) para forçar um redesenho.
            // O redesenho vai DESTRUIR este ItemView "órfão" e criar um novo
            // no seu slot de origem, efetivamente devolvendo-o.
            InventoryManager.Instance.RequestRedraw();
        }
    }
}