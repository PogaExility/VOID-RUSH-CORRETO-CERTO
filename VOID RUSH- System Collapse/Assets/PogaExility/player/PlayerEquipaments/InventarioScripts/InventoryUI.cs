using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("REFERÊNCIAS")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject slotViewPrefab;
    [SerializeField] private GameObject itemViewPrefab;
    [SerializeField] private Transform backpackPanel;

    [HideInInspector] public Canvas mainCanvas;

    // >> A LISTA SEGURA QUE VAI GUARDAR OS SLOTS CRIADOS <<
    private List<SlotView> createdSlots = new List<SlotView>();
    private List<GameObject> activeItemViews = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        mainCanvas = GetComponentInParent<Canvas>();
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;
    }

    void Start()
    {
        CreateGrid();
        inventoryManager.OnInventoryChanged += Redraw;
        Redraw();
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged -= Redraw;
    }

    private void CreateGrid()
    {
        foreach (Transform child in backpackPanel) Destroy(child.gameObject);
        createdSlots.Clear(); // Limpa a lista antes de recriar

        for (int i = 0; i < inventoryManager.GetBackpackSize(); i++)
        {
            var slotGO = Instantiate(slotViewPrefab, backpackPanel);
            var slotView = slotGO.GetComponent<SlotView>();
            slotView.Initialize(i);
            createdSlots.Add(slotView); // Guarda a referência do slot criado
        }
    }

    // >> A FUNÇÃO REDRAW 100% CORRIGIDA <<
    private void Redraw()
    {
        foreach (var view in activeItemViews) Destroy(view);
        activeItemViews.Clear();

        // Itera pela lista de slots GARANTIDOS que existem
        for (int i = 0; i < createdSlots.Count; i++)
        {
            var data = inventoryManager.GetBackpackSlot(i);

            if (data != null && data.item != null)
            {
                // Pega o transform do slot da nossa lista segura, que não tem como dar erro.
                Transform slotTransform = createdSlots[i].transform;

                // CRIA O ITEMVIEW COMO FILHO DO SLOTVIEW CORRETO
                var itemGO = Instantiate(itemViewPrefab, slotTransform);
                var itemView = itemGO.GetComponent<ItemView>();
                itemView.Render(i, data.item, data.count);
                activeItemViews.Add(itemGO);
            }
        }
    }

    private void RedrawUI()
    {
        Debug.Log("--- REDRAWUI CHAMADO ---");

        foreach (var view in activeItemViews) Destroy(view);
        activeItemViews.Clear();

        Debug.Log($"Total de slots de DADOS a serem verificados: {inventoryManager.GetBackpackSize()}");

        for (int i = 0; i < inventoryManager.GetBackpackSize(); i++)
        {
            var data = inventoryManager.GetBackpackSlot(i);

            // >> A CONDIÇÃO SUSPEITA <<
            if (data != null && data.item != null)
            {
                // SE O ITEM EXISTE, ESTA MENSAGEM TEM QUE APARECER
                Debug.Log($"Slot {i}: ENCONTRADO ITEM! '{data.item.name}'. Tentando criar o visual...");

                Transform slotTransform = createdSlots[i].transform;
                var itemGO = Instantiate(itemViewPrefab, slotTransform);
                var itemView = itemGO.GetComponent<ItemView>();
                itemView.Render(i, data.item, data.count);
                activeItemViews.Add(itemGO);
            }
        }
    }
}