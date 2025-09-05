using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // <<-- ADICIONE ESTA LINHA

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public event Action OnInventoryChanged;

    [Header("CONFIGURAÇÃO")]
    public int gridWidth = 10;
    public int gridHeight = 6;
    [SerializeField] private List<InventorySlot> backpackSlots = new();

    // O DADO DO ITEM QUE ESTÁ NO MOUSE
    private InventorySlot heldItem = new InventorySlot();

    void Awake() { Instance = this; InitializeInventory(); }

    private void InitializeInventory()
    {
        backpackSlots.Clear();
        for (int i = 0; i < gridWidth * gridHeight; i++)
            backpackSlots.Add(new InventorySlot());
    }

    public void OnSlotClicked(int index, PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Left)
        {
            var clickedSlot = backpackSlots[index];
            (heldItem.item, clickedSlot.item) = (clickedSlot.item, heldItem.item);
            (heldItem.count, clickedSlot.count) = (clickedSlot.count, heldItem.count);
        }
        else if (button == PointerEventData.InputButton.Right)
        {
            var clickedSlot = backpackSlots[index];
            if (heldItem.item == null && clickedSlot.item != null)
            {
                int half = Mathf.CeilToInt(clickedSlot.count / 2f);
                heldItem.Set(clickedSlot.item, half);
                clickedSlot.count -= half;
                if (clickedSlot.count <= 0) clickedSlot.Clear();
            }
            else if (heldItem.item != null && clickedSlot.item == null)
            {
                clickedSlot.Set(heldItem.item, 1);
                heldItem.count--;
                if (heldItem.count <= 0) heldItem.Clear();
            }
            else if (heldItem.item != null && clickedSlot.item == heldItem.item && clickedSlot.count < heldItem.item.maxStack)
            {
                clickedSlot.count++;
                heldItem.count--;
                if (heldItem.count <= 0) heldItem.Clear();
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public int TryAddItem(ItemSO item, int amount)
    {
        int amountLeft = amount;
        if (item == null || amount <= 0) return amount;
        // Empilha
        for (int i = 0; i < backpackSlots.Count && amountLeft > 0; i++)
        {
            var s = backpackSlots[i];
            if (s.item == item && s.count < item.maxStack)
            {
                int add = Mathf.Min(amountLeft, item.maxStack - s.count);
                s.count += add; amountLeft -= add;
            }
        }
        // Coloca em vazio
        if (amountLeft > 0)
        {
            for (int i = 0; i < backpackSlots.Count && amountLeft > 0; i++)
            {
                if (backpackSlots[i].item == null)
                {
                    int add = Mathf.Min(amountLeft, item.maxStack);
                    backpackSlots[i].Set(item, add); amountLeft -= add;
                }
            }
        }
        if (amountLeft < amount) OnInventoryChanged?.Invoke();
        return amountLeft;
    }

    // >> AS FUNÇÕES QUE O WEAPONHANDLER PRECISA <<
    public InventorySlot GetBackpackSlot(int i) => backpackSlots[i];
    public void RequestRedraw() => OnInventoryChanged?.Invoke();

    // Funções que a UI precisa
    public InventorySlot GetHeldItem() => heldItem;
    public int GetSize() => backpackSlots.Count;
}