using System;

[Serializable] // Permite que isso seja visualizado no Inspector e salvo
public class InventorySlot
{
    public ItemSO item;
    public int count;

    public InventorySlot()
    {
        item = null;
        count = 0;
    }

    public void Clear()
    {
        item = null;
        count = 0;
    }

    public void Set(ItemSO newItem, int newCount)
    {
        item = newItem;
        count = newCount;
    }
}