using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // --- Eventos para comunicar com outros sistemas ---
    public event Action<ItemSO, int, int, bool> OnItemAdded; // bool para 'isRotated'
    public event Action<ItemSO> OnItemRemoved;
    public event Action<ItemSO, bool> OnItemHeld; // bool para 'isRotated'
    public event Action<ItemSO> OnItemDropped;

    [Header("Configuração do Grid")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    [Header("Referências de Equipamento")]
    public ItemSO equippedMeleeWeapon;
    public ItemSO equippedFirearm;
    public ItemSO equippedBuster;

    // --- Estado do Inventário ---
    public ItemSO heldItem { get; private set; }
    public bool isHeldItemRotated { get; private set; }

    private ItemSO[,] inventoryGrid;
    public List<ItemSO> inventoryItems = new List<ItemSO>();
    private Dictionary<ItemSO, bool> itemRotations = new Dictionary<ItemSO, bool>();

    void Awake()
    {
        // Awake agora só garante que a inicialização aconteça.
        EnsureGridExists();
    }

    // --- CORREÇÃO DEFINITIVA DO CRASH: Esta função garante que o 'inventoryGrid' nunca seja nulo. ---
    private void EnsureGridExists()
    {
        if (inventoryGrid == null)
        {
            inventoryGrid = new ItemSO[gridWidth, gridHeight];
        }
    }

    // --- Funções Principais de Interação ---

    public void StartHoldingItem(ItemSO item)
    {
        if (heldItem != null) return;
        heldItem = item;
        isHeldItemRotated = false;
        OnItemHeld?.Invoke(heldItem, isHeldItemRotated);
    }

    public void PickUpItemFromGrid(ItemSO itemToPickUp)
    {
        if (heldItem != null) return;
        bool wasRotated = itemRotations.ContainsKey(itemToPickUp) && itemRotations[itemToPickUp];
        RemoveItem(itemToPickUp);
        heldItem = itemToPickUp;
        isHeldItemRotated = wasRotated;
        OnItemHeld?.Invoke(heldItem, isHeldItemRotated);
    }

    public void RotateHeldItem()
    {
        if (heldItem != null)
        {
            isHeldItemRotated = !isHeldItemRotated;
            OnItemHeld?.Invoke(heldItem, isHeldItemRotated);
        }
    }

    public void DropHeldItem()
    {
        if (heldItem != null)
        {
            OnItemDropped?.Invoke(heldItem); // Avisa o spawner
            heldItem = null; // LIMPA A MÃO
        }
    }

    public bool PlaceHeldItem(int x, int y)
    {
        if (heldItem == null) return false;

        int width = isHeldItemRotated ? heldItem.height : heldItem.width;
        int height = isHeldItemRotated ? heldItem.width : heldItem.height;

        if (!CanPlaceItem(x, y, width, height)) return false;

        PlaceItem(heldItem, x, y, isHeldItemRotated);
        heldItem = null;
        return true;
    }

    // --- Funções de Gerenciamento do Grid ---

    public bool AddItem(ItemSO itemToAdd)
    {
        EnsureGridExists(); // GARANTE QUE NÃO VAI QUEBRAR
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (CanPlaceItem(x, y, itemToAdd.width, itemToAdd.height))
                {
                    PlaceItem(itemToAdd, x, y, false); // Itens sempre são adicionados sem rotação
                    return true;
                }
            }
        }
        Debug.Log("Não há espaço no inventário para: " + itemToAdd.itemName);
        return false;
    }

    private void PlaceItem(ItemSO item, int startX, int startY, bool isRotated)
    {
        EnsureGridExists(); // GARANTE QUE NÃO VAI QUEBRAR
        int width = isRotated ? item.height : item.width;
        int height = isRotated ? item.width : item.height;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                inventoryGrid[startX + i, startY + j] = item;
            }
        }

        if (!inventoryItems.Contains(item))
        {
            inventoryItems.Add(item);
        }
        itemRotations[item] = isRotated;
        OnItemAdded?.Invoke(item, startX, startY, isRotated);
    }

    public void RemoveItem(ItemSO itemToRemove)
    {
        EnsureGridExists(); // GARANTE QUE NÃO VAI QUEBRAR
        if (!inventoryItems.Contains(itemToRemove)) return;

        if (FindItemPosition(itemToRemove, out int itemX, out int itemY))
        {
            bool wasRotated = itemRotations.ContainsKey(itemToRemove) && itemRotations[itemToRemove];
            int width = wasRotated ? itemToRemove.height : itemToRemove.width;
            int height = wasRotated ? itemToRemove.width : itemToRemove.height;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    inventoryGrid[itemX + i, itemY + j] = null;
                }
            }
        }

        inventoryItems.Remove(itemToRemove);
        itemRotations.Remove(itemToRemove);
        OnItemRemoved?.Invoke(itemToRemove);
    }

    public void DropItemFromUI(ItemSO itemToDrop)
    {
        if (inventoryItems.Contains(itemToDrop))
        {
            RemoveItem(itemToDrop);
            OnItemDropped?.Invoke(itemToDrop);
        }
    }

    public bool CanPlaceItem(int startX, int startY, int width, int height)
    {
        EnsureGridExists(); // GARANTE QUE NÃO VAI QUEBRAR
        if (startX < 0 || startY < 0 || startX + width > gridWidth || startY + height > gridHeight) return false;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                if (inventoryGrid[startX + i, startY + j] != null) return false;
            }
        }
        return true;
    }

    public bool FindItemPosition(ItemSO item, out int xPos, out int yPos)
    {
        EnsureGridExists(); // GARANTE QUE NÃO VAI QUEBRAR
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

    public void RedrawAllItems()
    {
        EnsureGridExists(); // GARANTE QUE NÃO VAI QUEBRAR
        foreach (var item in new List<ItemSO>(inventoryItems)) // Itera sobre uma cópia para segurança
        {
            if (FindItemPosition(item, out int x, out int y))
            {
                bool isRotated = itemRotations.ContainsKey(item) && itemRotations[item];
                OnItemAdded?.Invoke(item, x, y, isRotated);
            }
        }
    }

    // --- Funções de Equipamento (Sem Mudanças) ---
    public void EquipWeapon(ItemSO weaponToEquip)
    {
        if (weaponToEquip.itemType != ItemType.Weapon || !inventoryItems.Contains(weaponToEquip)) return;
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