// InventorySlot.cs - VERS�O CORRIGIDA
using System;

[Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int count;
    public int currentAmmo; // O "campo de mem�ria" para a muni��o.

    public InventorySlot()
    {
        Clear();
    }

    public void Clear()
    {
        item = null;
        count = 0;
        currentAmmo = -1; // -1 � o nosso valor padr�o para "n�o aplic�vel" ou "pente cheio".
    }

    public void Set(ItemSO newItem, int newCount)
    {
        item = newItem;
        count = newCount;
        currentAmmo = -1; // Sempre reseta a muni��o ao colocar um novo item.
    }
}