using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI; // <<-- Adicionado

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("CONFIGURAÇÃO DE DADOS")]
    [Range(1, 20)] public int gridWidth = 10;
    [Range(1, 20)] public int gridHeight = 6;
    [SerializeField] private List<InventorySlot> backpackSlots = new();

    [Header("REFERÊNCIAS DA UI")]
    [SerializeField] private GameObject slotViewPrefab;
    [SerializeField] private GameObject itemViewPrefab;
    [SerializeField] private Transform backpackPanel;
    [SerializeField] private Transform mainCanvasTransform;

    [Header("ITENS DE TESTE")]
    [SerializeField] private List<ItemSO> startingItems;

    private List<GameObject> activeItemViews = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializeInventory();
    }

    void Start()
    {
        CreateGrid();

        if (startingItems != null)
        {
            foreach (var item in startingItems)
            {
                TryAddItem(item, 1, false); // Adiciona sem redesenhar
            }
        }

        Redraw(); // Desenho único e final no final do Start
    }

    // --- LÓGICA DE DADOS ---
    private void InitializeInventory()
    {
        int requiredSize = gridWidth * gridHeight;
        if (backpackSlots.Count == requiredSize) return;
        backpackSlots.Clear();
        for (int i = 0; i < requiredSize; i++)
            backpackSlots.Add(new InventorySlot());
    }

    public void SwapSlots(int from, int to)
    {
        (backpackSlots[from], backpackSlots[to]) = (backpackSlots[to], backpackSlots[from]);
        Redraw(); // Sempre redesenha após uma troca
    }

    public int TryAddItem(ItemSO item, int amount)
    {
        int amountLeft = amount;

        // Empilha em existentes
        for (int i = 0; i < backpackSlots.Count && amountLeft > 0; i++)
        {
            var slot = backpackSlots[i];
            if (slot.item == item && slot.count < item.maxStack)
            {
                int add = Mathf.Min(amountLeft, item.maxStack - slot.count);
                slot.count += add;
                amountLeft -= add;
            }
        }

        // Coloca em vazios
        if (amountLeft > 0)
        {
            for (int i = 0; i < backpackSlots.Count && amountLeft > 0; i++)
            {
                // AQUI ESTÁ A CORREÇÃO, USANDO backpackSlots[i] DIRETAMENTE
                if (backpackSlots[i].item == null)
                {
                    int add = Mathf.Min(amountLeft, item.maxStack);
                    backpackSlots[i].Set(item, add);
                    amountLeft -= add;
                }
            }
        }

        if (amountLeft < amount && redraw)
        {
            Redraw();
        }
        return amountLeft;
    }
    // --- LÓGICA DE UI (AGORA MORA AQUI) ---
    private void CreateGrid()
    {
        for (int i = 0; i < GetBackpackSize(); i++)
        {
            var slotGO = Instantiate(slotViewPrefab, backpackPanel);
            slotGO.GetComponent<SlotView>().Initialize(i);
        }
    }

    private void Redraw()
    {
        foreach (var view in activeItemViews) Destroy(view);
        activeItemViews.Clear();

        for (int i = 0; i < GetBackpackSize(); i++)
        {
            Transform slotTransform = backpackPanel.GetChild(i);
            var data = GetBackpackSlot(i);

            if (data != null && data.item != null)
            {
                var itemGO = Instantiate(itemViewPrefab, slotTransform);
                var itemView = itemGO.GetComponent<ItemView>();
                itemView.Render(i, data.item, data.count);
                activeItemViews.Add(itemGO);
            }
        }
    }

    public int GetBackpackSize() => gridWidth * gridHeight;
    public InventorySlot GetBackpackSlot(int index) => backpackSlots[index];
    public Transform GetMainCanvasTransform() => mainCanvasTransform;
}