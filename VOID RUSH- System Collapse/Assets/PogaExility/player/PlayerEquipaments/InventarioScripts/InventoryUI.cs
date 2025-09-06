using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Adicione esta linha no topo

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("REFERÊNCIAS DA UI")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform backpackPanel;

    [Header("ÍCONE DO MOUSE")]
    [SerializeField] private Image heldItemIcon;
    [SerializeField] private TextMeshProUGUI heldItemCountText;

    private List<ItemView> itemViewPool = new List<ItemView>();

    void Awake() => Instance = this;
    void Start()
    {
        CreateGridAndPool();
        InventoryManager.Instance.OnInventoryChanged += Redraw;
        Redraw();
        // Garante que o ícone fantasma comece desligado
        heldItemIcon.gameObject.SetActive(false);
    }
    private void OnDestroy() => InventoryManager.Instance.OnInventoryChanged -= Redraw;

    void Update()
    {
        // O ícone do mouse segue o mouse
        if (heldItemIcon.gameObject.activeInHierarchy)
            heldItemIcon.transform.position = Input.mousePosition;
    }

    void CreateGridAndPool()
    {
        // >> LINHA DE SEGURANÇA <<
        if (backpackPanel == null)
        {
            Debug.LogError("FATAL: A referência 'Backpack Panel' está faltando no Inspector do InventoryUI!", this);
            return; // Impede a execução e a quebra do jogo.
        }

        for (int i = 0; i < InventoryManager.Instance.GetSize(); i++)
        {
            // O 'backpackPanel' agora está garantido de não ser nulo.
            var slot = Instantiate(slotPrefab, backpackPanel);
            var slotView = slot.GetComponent<SlotView>();
            slotView.Initialize(i);

            var item = Instantiate(itemPrefab, slot.transform);
            var itemView = item.GetComponent<ItemView>();
            itemView.Initialize(i);
            itemViewPool.Add(itemView);
        }
    }

    void Redraw()
    {
        // 1. Redesenha a grade do inventário
        for (int i = 0; i < itemViewPool.Count; i++)
        {
            var data = InventoryManager.Instance.GetBackpackSlot(i);
            var view = itemViewPool[i];

            if (data.item != null)
            {
                view.gameObject.SetActive(true);
                // >> CORREÇÃO: Envia o ItemSO completo, não só a sprite <<
                view.Render(data.item, data.count);
            }
            else
            {
                view.gameObject.SetActive(false);
            }
        }

        // 2. Redesenha o ícone que está no mouse
        var heldItemData = InventoryManager.Instance.GetHeldItem();
        bool isHoldingItem = heldItemData.item != null;
        heldItemIcon.gameObject.SetActive(isHoldingItem);
        if (isHoldingItem)
        {
            heldItemIcon.sprite = heldItemData.item.itemIcon;
            heldItemCountText.text = heldItemData.count > 1 ? heldItemData.count.ToString() : "";
        }
    }
}