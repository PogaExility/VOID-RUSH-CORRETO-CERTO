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

    [Header("Configura��o do Grid")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    [Header("Refer�ncias de Equipamento")]
    public ItemSO equippedMeleeWeapon;
    public ItemSO equippedFirearm;
    public ItemSO equippedBuster;
  
    // --- Estado do Invent�rio ---
    public ItemSO heldItem { get; private set; }
    public bool isHeldItemRotated { get; private set; }

    private ItemSO[,] inventoryGrid;
    public List<ItemSO> inventoryItems = new List<ItemSO>();
    private Dictionary<ItemSO, bool> itemRotations = new Dictionary<ItemSO, bool>();
    private List<ItemSO> temporaryItems = new List<ItemSO>();
   

    void Awake()
    {
        // Awake agora s� garante que a inicializa��o aconte�a.
        EnsureGridExists();
    }

    // --- CORRE��O DEFINITIVA DO CRASH: Esta fun��o garante que o 'inventoryGrid' nunca seja nulo. ---
    private void EnsureGridExists()
    {
        if (inventoryGrid == null)
        {
            inventoryGrid = new ItemSO[gridWidth, gridHeight];
        }
    }

    // --- Fun��es Principais de Intera��o ---

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
            heldItem = null; // LIMPA A M�O
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

    // --- Fun��es de Gerenciamento do Grid ---

    public bool AddItem(ItemSO itemToAdd)
    {
        EnsureGridExists(); // GARANTE QUE N�O VAI QUEBRAR
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (CanPlaceItem(x, y, itemToAdd.width, itemToAdd.height))
                {
                    PlaceItem(itemToAdd, x, y, false); // Itens sempre s�o adicionados sem rota��o
                    return true;
                }
            }
        }
        Debug.Log("N�o h� espa�o no invent�rio para: " + itemToAdd.itemName);
        return false;
    }

    private void PlaceItem(ItemSO item, int startX, int startY, bool isRotated)
    {
        EnsureGridExists(); // GARANTE QUE N�O VAI QUEBRAR
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
        EnsureGridExists(); // GARANTE QUE N�O VAI QUEBRAR
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
        EnsureGridExists(); // GARANTE QUE N�O VAI QUEBRAR
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
        EnsureGridExists(); // GARANTE QUE N�O VAI QUEBRAR
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
        EnsureGridExists(); // GARANTE QUE N�O VAI QUEBRAR
        foreach (var item in new List<ItemSO>(inventoryItems)) // Itera sobre uma c�pia para seguran�a
        {
            if (FindItemPosition(item, out int x, out int y))
            {
                bool isRotated = itemRotations.ContainsKey(item) && itemRotations[item];
                OnItemAdded?.Invoke(item, x, y, isRotated);
            }
        }
    }

    // --- Fun��es de Equipamento (Sem Mudan�as) ---
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
    // --- NOVA FUN��O DE COLETA ---
    public bool PickupItem(ItemSO itemToAdd)
    {
        if (AddItem(itemToAdd)) // Usa sua fun��o AddItem existente para encontrar um espa�o e colocar no grid
        {
            // Se foi adicionado com sucesso, verificamos o estado da quest
            if (QuestManager.Instance != null && QuestManager.Instance.IsQuestActive)
            {
                // Se a quest estiver ativa E o item estiver marcado no SO como "perd�vel",
                // n�s o adicionamos � lista de itens tempor�rios.
                if (itemToAdd.isLostOnDeathDuringQuest && !temporaryItems.Contains(itemToAdd))
                {
                    temporaryItems.Add(itemToAdd);
                    Debug.Log($"{itemToAdd.itemName} foi adicionado como item tempor�rio de quest.");
                }
            }
            return true;
        }
        return false;
    }
    // --- NOVAS FUN��ES DE GERENCIAMENTO DE QUEST ---

    /// <summary>
    /// Chamado quando uma quest � completada ou um checkpoint � salvo.
    /// Torna todos os itens tempor�rios em permanentes, simplesmente limpando a lista de rastreamento.
    /// </summary>
    public void CommitTemporaryItems()
    {
        if (temporaryItems.Count > 0)
        {
            Debug.Log($"{temporaryItems.Count} item(ns) tempor�rio(s) foram salvos permanentemente no invent�rio.");
            temporaryItems.Clear();
        }
    }

    /// <summary>
    /// Chamado pelo RespawnManager quando o jogador morre com uma quest ativa.
    /// Remove do invent�rio principal apenas os itens que estavam na lista de tempor�rios.
    /// </summary>
    public void ClearTemporaryItems()
    {
        if (temporaryItems.Count > 0)
        {
            Debug.Log($"Removendo {temporaryItems.Count} item(ns) tempor�rio(s) por morte em quest.");

            // Usamos 'new List<ItemSO>(temporaryItems)' para criar uma c�pia, 
            // pois n�o se pode modificar uma lista enquanto se itera sobre ela.
            foreach (ItemSO itemToRemove in new List<ItemSO>(temporaryItems))
            {
                // Usa a sua fun��o RemoveItem j� existente para limpar tudo (grid, lista, eventos).
                RemoveItem(itemToRemove);
            }

            temporaryItems.Clear();
        }
    }
}