using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public event Action OnInventoryChanged;

    [Range(1, 20)] public int gridWidth = 10;
    [Range(1, 20)] public int gridHeight = 6;
    [SerializeField] private List<InventorySlot> backpackSlots = new();
    [SerializeField] private Transform mainCanvasTransform;
    public Transform GetMainCanvasTransform()
    {
        return mainCanvasTransform;
    }
    void Awake()
    {
        Instance = this;
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        backpackSlots.Clear();
        for (int i = 0; i < gridWidth * gridHeight; i++)
            backpackSlots.Add(new InventorySlot());
    }

    public void SwapSlots(int from, int to)
    {
        (backpackSlots[from], backpackSlots[to]) = (backpackSlots[to], backpackSlots[from]);
        OnInventoryChanged?.Invoke();
    }
    public int GetBackpackSize()
    {
        return gridWidth * gridHeight;
    }
    public InventorySlot GetBackpackSlot(int index) => backpackSlots[index];
    public int TryAddItem(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return amount;

        int amountLeft = amount;

        // 1. Tenta empilhar
        for (int i = 0; i < backpackSlots.Count && amountLeft > 0; i++)
        {
            var slot = backpackSlots[i];
            if (slot.item == item && slot.count < item.maxStack)
            {
                int spaceAvailable = item.maxStack - slot.count;
                int amountToAdd = Mathf.Min(amountLeft, spaceAvailable);
                slot.count += amountToAdd;
                amountLeft -= amountToAdd;
            }
        }

        // 2. Procura slots vazios
        if (amountLeft > 0)
        {
            for (int i = 0; i < backpackSlots.Count && amountLeft > 0; i++)
            {
                var slot = backpackSlots[i];
                if (slot.item == null)
                {
                    int amountToAdd = Mathf.Min(amountLeft, item.maxStack);
                    slot.Set(item, amountToAdd);
                    amountLeft -= amountToAdd;
                }
            }
        }

        // Se conseguiu adicionar pelo menos um item, avisa a UI
        if (amountLeft < amount)
        {
            OnInventoryChanged?.Invoke();
        }

        return amountLeft;
    }
}