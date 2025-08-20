using System.Collections.Generic;
using UnityEngine;
using System; // Necessário para Action (eventos)

public class InventoryManager : MonoBehaviour
{
    // Eventos para comunicar com a UI
    public event Action<ItemSO, int, int> OnItemAdded;
    public event Action<ItemSO> OnItemRemoved;

    [Header("Referências de Equipamento")]
    public WeaponSO equippedMeleeWeapon;
    public WeaponSO equippedFirearm;
    public WeaponSO equippedBuster;

    [Header("Configuração do Grid (Maleta)")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    private ItemSO[,] inventoryGrid;
    public List<ItemSO> inventoryItems = new List<ItemSO>();

    void Awake()
    {
        inventoryGrid = new ItemSO[gridWidth, gridHeight];
    }

    // A SEÇÃO DE TESTE QUE ESTAVA AQUI FOI REMOVIDA
    // [Header("Itens de Teste")]
    // public ItemSO itemDeTeste;
    // void Start()
    // {
    //     if (itemDeTeste != null) AddItem(itemDeTeste);
    // }

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

        // AVISA A UI que um item foi adicionado e onde
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
            // AVISA A UI que um item foi removido
            OnItemRemoved?.Invoke(itemToRemove);
        }
    }

    // Função para a UI pedir que todos os itens sejam redesenhados
    public void RedrawAllItems()
    {
        foreach (var item in inventoryItems)
        {
            FindItemPosition(item, out int x, out int y);
            OnItemAdded?.Invoke(item, x, y);
        }
    }

    private bool FindItemPosition(ItemSO item, out int xPos, out int yPos)
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

    public void EquipWeapon(WeaponSO weaponToEquip)
    {
        if (!inventoryItems.Contains(weaponToEquip))
        {
            return;
        }
        switch (weaponToEquip.weaponType)
        {
            case WeaponType.Melee:
                equippedMeleeWeapon = weaponToEquip;
                break;
            case WeaponType.Firearm:
                equippedFirearm = weaponToEquip;
                break;
            case WeaponType.Buster:
                equippedBuster = weaponToEquip;
                break;
        }
    }

    public void UnequipWeapon(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Melee:
                equippedMeleeWeapon = null;
                break;
            case WeaponType.Firearm:
                equippedFirearm = null;
                break;
            case WeaponType.Buster:
                equippedBuster = null;
                break;
        }
    }
}