using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    // --- Eventos para comunicar com outros sistemas ---
    public event Action<ItemSO, int, int> OnItemAdded; // Avisa a UI para desenhar um item
    public event Action<ItemSO> OnItemRemoved;           // Avisa a UI para apagar um item
    public event Action<ItemSO> OnItemHeld;              // Avisa a UI para "colar" um item no mouse
    public event Action<ItemSO> OnItemDropped;           // Avisa o ItemSpawner para criar um item no mundo

    [Header("Referências de Equipamento")]
    public ItemSO equippedMeleeWeapon;
    public ItemSO equippedFirearm;
    public ItemSO equippedBuster;

    [Header("Configuração do Grid (Maleta)")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    public ItemSO heldItem { get; private set; } // O item que está "na mão" do jogador

    private ItemSO[,] inventoryGrid;
    public List<ItemSO> inventoryItems = new List<ItemSO>();

    void Awake()
    {
        inventoryGrid = new ItemSO[gridWidth, gridHeight];
    }

    // --- Funções Principais de Interação ---

    // Chamado pelo PlayerController quando o jogador pega um item
    public void StartHoldingItem(ItemSO item)
    {
        if (heldItem != null) return;
        heldItem = item;
        OnItemHeld?.Invoke(heldItem);
    }

    // Chamado pela UI quando o jogador clica para colocar o item
    public bool PlaceHeldItem(int x, int y)
    {
        if (heldItem == null) return false;

        if (CanPlaceItem(heldItem, x, y))
        {
            PlaceItem(heldItem, x, y);
            heldItem = null;
            return true;
        }
        return false;
    }

    // Chamado pela UI da "lixeira"
    public void DropItemFromUI(ItemSO itemToDrop)
    {
        if (inventoryItems.Contains(itemToDrop))
        {
            // 1. Remove o item da lógica do inventário
            RemoveItem(itemToDrop);

            // 2. Dispara o evento para o ItemSpawner criar o objeto no mundo
            OnItemDropped?.Invoke(itemToDrop);
        }
    }

    // Chamado quando o inventário é fechado com um item na mão
    public void DropHeldItem()
    {
        if (heldItem != null)
        {
            // Dispara o evento para o ItemSpawner criar o objeto no mundo
            OnItemDropped?.Invoke(heldItem);
            heldItem = null;
        }
    }

    // --- Funções de Gerenciamento do Grid ---

    public bool AddItem(ItemSO itemToAdd)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (CanPlaceItem(itemToAdd, x, y))
                {
                    PlaceItem(itemToAdd, x, y);
                    return true;
                }
            }
        }
        Debug.Log("Não há espaço no inventário para: " + itemToAdd.itemName);
        return false;
    }

    private void PlaceItem(ItemSO item, int startX, int startY)
    {
        for (int y = 0; y < item.height; y++)
        {
            for (int x = 0; x < item.width; x++)
            {
                inventoryGrid[startX + x, startY + y] = item;
            }
        }

        if (!inventoryItems.Contains(item))
        {
            inventoryItems.Add(item);
        }
        OnItemAdded?.Invoke(item, startX, startY);
    }

    public void RemoveItem(ItemSO itemToRemove)
    {
        if (inventoryItems.Contains(itemToRemove))
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (inventoryGrid[x, y] == itemToRemove)
                    {
                        inventoryGrid[x, y] = null;
                    }
                }
            }
            inventoryItems.Remove(itemToRemove);
            OnItemRemoved?.Invoke(itemToRemove);
        }
    }

    public void RedrawAllItems()
    {
        foreach (var item in inventoryItems)
        {
            if (FindItemPosition(item, out int x, out int y))
            {
                OnItemAdded?.Invoke(item, x, y);
            }
        }
    }

    public bool FindItemPosition(ItemSO item, out int xPos, out int yPos)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (inventoryGrid[x, y] == item)
                {
                    xPos = x;
                    yPos = y;
                    return true;
                }
            }
        }
        xPos = -1;
        yPos = -1;
        return false;
    }

    private bool CanPlaceItem(ItemSO item, int startX, int startY)
    {
        if (startX < 0 || startY < 0 || startX + item.width > gridWidth || startY + item.height > gridHeight)
        {
            return false;
        }
        for (int y = 0; y < item.height; y++)
        {
            for (int x = 0; x < item.width; x++)
            {
                if (inventoryGrid[startX + x, startY + y] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // ===== CORREÇÃO DE TIPO: USA 'ItemSO' e depois checa o 'weaponType' =====
    public void EquipWeapon(ItemSO weaponToEquip)
    {
        if (weaponToEquip.itemType != ItemType.Weapon) return;
        if (!inventoryItems.Contains(weaponToEquip)) return;

        switch (weaponToEquip.weaponType)
        {
            case WeaponType.Melee: equippedMeleeWeapon = weaponToEquip; break;
            case WeaponType.Firearm: equippedFirearm = weaponToEquip; break;
            case WeaponType.Buster: equippedBuster = weaponToEquip; break;
        }
    }

    public void UnequipWeapon(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Melee: equippedMeleeWeapon = null; break;
            case WeaponType.Firearm: equippedFirearm = null; break;
            case WeaponType.Buster: equippedBuster = null; break;
        }
    }
}