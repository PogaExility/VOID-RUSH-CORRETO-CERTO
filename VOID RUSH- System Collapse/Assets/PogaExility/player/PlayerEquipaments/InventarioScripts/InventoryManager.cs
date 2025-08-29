using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // --- Eventos para comunicar com outros sistemas ---
    public event Action<ItemSO, int, int, bool> OnItemAdded;
    public event Action<ItemSO> OnItemRemoved;
    public event Action<ItemSO, bool> OnItemHeld;
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

    // --- A NOVA LÓGICA DE QUEST ---
    private List<ItemSO> temporaryItems = new List<ItemSO>();
    private ItemSO potentiallyTemporaryItem; // Guarda o item que acabamos de pegar do chão

    void Awake()
    {
        EnsureGridExists();
    }

    private void EnsureGridExists()
    {
        if (inventoryGrid == null)
        {
            inventoryGrid = new ItemSO[gridWidth, gridHeight];
        }
    }

    // --- AS NOVAS FUNÇÕES PÚBLICAS DE QUEST ---

    /// <summary>
    /// Chamado pelo PlayerController ANTES de pegar um item.
    /// Ele "avisa" ao manager que o próximo item a ser pego PODE ser temporário.
    /// </summary>
    public void FlagItemAsPotentiallyTemporary(ItemSO item)
    {
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestActive && item.isLostOnDeathDuringQuest)
        {
            potentiallyTemporaryItem = item;
        }
    }

    /// <summary>
    /// Transforma todos os itens temporários em permanentes.
    /// </summary>
    public void CommitTemporaryItems()
    {
        if (temporaryItems.Count > 0)
        {
            Debug.Log($"Itens de quest ({temporaryItems.Count}) foram salvos permanentemente.");
            temporaryItems.Clear();
        }
    }

    /// <summary>
    /// Remove apenas os itens de quest do inventário.
    /// </summary>
    public void ClearTemporaryItems()
    {
        if (temporaryItems.Count > 0)
        {
            Debug.Log($"Removendo {temporaryItems.Count} itens de quest por morte.");
            foreach (ItemSO item in new List<ItemSO>(temporaryItems))
            {
                RemoveItem(item); // Usa sua função de remoção já existente
            }
            temporaryItems.Clear();
        }
    }

    // --- SUAS FUNÇÕES ORIGINAIS (COM PEQUENAS MODIFICAÇÕES) ---

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
            OnItemDropped?.Invoke(heldItem);
            heldItem = null;

            // --- MUDANÇA IMPORTANTE ---
            // Se o item dropado era o que pegamos do chão, limpamos a flag.
            potentiallyTemporaryItem = null;
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

    public bool AddItem(ItemSO itemToAdd)
    {
        EnsureGridExists();
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (CanPlaceItem(x, y, itemToAdd.width, itemToAdd.height))
                {
                    PlaceItem(itemToAdd, x, y, false);
                    return true;
                }
            }
        }
        Debug.Log("Não há espaço no inventário para: " + itemToAdd.itemName);
        return false;
    }

    private void PlaceItem(ItemSO item, int startX, int startY, bool isRotated)
    {
        EnsureGridExists();
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

        // --- MUDANÇA IMPORTANTE ---
        // Se o item que acabamos de colocar no grid era o que pegamos do chão,
        // agora o confirmamos como um item temporário.
        if (item == potentiallyTemporaryItem)
        {
            if (!temporaryItems.Contains(item))
            {
                temporaryItems.Add(item);
                Debug.Log($"'{item.itemName}' foi confirmado como item de quest temporário.");
            }
            potentiallyTemporaryItem = null; // Limpa a flag
        }
    }

    public void RemoveItem(ItemSO itemToRemove)
    {
        EnsureGridExists();
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
                    if (itemX + i < gridWidth && itemY + j < gridHeight)
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
        EnsureGridExists();
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
        EnsureGridExists();
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
        EnsureGridExists();
        foreach (var item in new List<ItemSO>(inventoryItems))
        {
            if (FindItemPosition(item, out int x, out int y))
            {
                bool isRotated = itemRotations.ContainsKey(item) && itemRotations[item];
                OnItemAdded?.Invoke(item, x, y, isRotated);
            }
        }
    }

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