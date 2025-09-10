// InventorySlot.cs - VERSÃO CORRIGIDA
using System;

[Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int count;
    public int currentAmmo; // O "campo de memória" para a munição.

    public InventorySlot()
    {
        Clear();
    }

    public void Clear()
    {
        item = null;
        count = 0;
        currentAmmo = -1; // -1 é o nosso valor padrão para "não aplicável" ou "pente cheio".
    }

    public void Set(ItemSO newItem, int newCount)
    {
        item = newItem;
        count = newCount;
        currentAmmo = -1; // Sempre reseta a munição ao colocar um novo item.
    }
}