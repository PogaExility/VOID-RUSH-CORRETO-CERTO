using UnityEngine;

using UnityEngine.UI;

using TMPro;

// Futuramente, adicionaremos interfaces aqui para Drag & Drop, como:

// using UnityEngine.EventSystems;

// public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler ...

public class InventorySlotUI : MonoBehaviour

{

    [Header("Componentes Visuais (arraste do prefab)")]

    [SerializeField] private Image iconImage;

    [SerializeField] private TextMeshProUGUI countText;

    [SerializeField] private GameObject highlightOverlay; // Opcional: para feedback visual

    // Dados de refer�ncia (preenchidos pelo InventoryUI)

    private InventoryManager inventoryManager;

    private int slotIndex;

    /// <summary>

    /// Configura este slot visual com sua identidade. Chamado uma �nica vez pelo InventoryUI.

    /// </summary>

    public void Initialize(InventoryManager manager, int index)

    {

        inventoryManager = manager;

        slotIndex = index;

    }

    /// <summary>

    /// L� os dados do InventoryManager e atualiza o visual deste slot.

    /// Esta � a fun��o principal que � chamada sempre que o slot precisa ser redesenhado.

    /// </summary>

    public void Refresh()

    {

        if (inventoryManager == null) return; // Seguran�a

        InventorySlot dataSlot = inventoryManager.GetBackpackSlot(slotIndex);

        bool hasItem = dataSlot != null && dataSlot.item != null && dataSlot.count > 0;

        if (hasItem)

        {

            // Ativa os componentes visuais

            iconImage.enabled = true;

            iconImage.sprite = dataSlot.item.itemIcon;

            // Mostra a contagem apenas se for maior que 1

            bool showCount = dataSlot.count > 1;

            countText.enabled = showCount;

            if (showCount)

            {

                countText.text = dataSlot.count.ToString();

            }

        }

        else

        {

            // Slot vazio: desativa tudo

            iconImage.enabled = false;

            iconImage.sprite = null; // Libera a refer�ncia do sprite

            countText.enabled = false;

        }

    }

    // --- M�TODOS FUTUROS PARA INTERA��O ---

    // Exemplo de como o tooltip funcionaria aqui:

    // public void OnPointerEnter(PointerEventData eventData)

    // {

    //     InventorySlot dataSlot = inventoryManager.GetBackpackSlot(slotIndex);

    //     if (dataSlot.item != null)

    //     {

    //         TooltipManager.Instance.Show(dataSlot.item, transform.position);

    //     }

    // }

    //

    // public void OnPointerExit(PointerEventData eventData)

    // {

    //     TooltipManager.Instance.Hide();

    // }

}