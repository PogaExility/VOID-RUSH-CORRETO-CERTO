using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // --- Estrutura para rastrear itens temporários ---
    private struct TemporaryItemEntry
    {
        public ItemSO item;
        public int amount;
    }

    // Eventos para a UI se atualizar
    public event Action<ItemSO, int, int, bool> OnItemAdded;
    public event Action<ItemSO> OnItemRemoved;
    public event Action<ItemSO> OnItemHeld;

    [Header("Configuração da Mochila")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    [Header("Estado Atual (Debug)")]
    [SerializeField] private ItemSO[,] inventoryGrid;
    [SerializeField] private int[,] stackCounts;
    [SerializeField] public ItemSO equippedWeapon;

    public ItemSO heldItem { get; private set; }

    // --- LÓGICA DE ITENS DE QUEST (REINTEGRADA) ---
    private List<TemporaryItemEntry> temporaryItems = new List<TemporaryItemEntry>();

    private PlayerStats playerStats;
    private WeaponHandler weaponHandler;

    void Awake()
    {
        EnsureGridExists();
        playerStats = FindObjectOfType<PlayerStats>();
        weaponHandler = FindObjectOfType<WeaponHandler>();
    }

    public void EnsureGridExists()
    {
        if (inventoryGrid == null || inventoryGrid.GetLength(0) != gridWidth || inventoryGrid.GetLength(1) != gridHeight)
        {
            inventoryGrid = new ItemSO[gridWidth, gridHeight];
            stackCounts = new int[gridWidth, gridHeight];
        }
    }

    // --- FUNÇÕES DE QUEST PÚBLICAS (AGORA FUNCIONAM) ---

    /// <summary>
    /// Transforma todos os itens temporários em permanentes ao completar uma quest.
    /// </summary>
    public void CommitTemporaryItems()
    {
        if (temporaryItems.Count > 0)
        {
            Debug.Log($"Itens de quest ({temporaryItems.Count} tipos) foram salvos permanentemente.");
            temporaryItems.Clear();
        }
    }

    /// <summary>
    /// Remove os itens de quest do inventário ao morrer.
    /// </summary>
    public void ClearTemporaryItems()
    {
        if (temporaryItems.Count > 0)
        {
            Debug.Log($"Removendo {temporaryItems.Count} tipos de itens de quest por morte.");
            foreach (var entry in temporaryItems)
            {
                RemoveItemByType(entry.item, entry.amount);
            }
            temporaryItems.Clear();
        }
    }

    // --- MÉTODOS DE MANIPULAÇÃO DE ITENS (ATUALIZADOS) ---

    public bool TryAddItem(ItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // Antes de adicionar, verifica se é um item de quest
        bool isQuestItem = QuestManager.Instance != null && QuestManager.Instance.IsQuestActive && item.isLostOnDeathDuringQuest;

        // Se for arma, tenta equipar
        if (item.itemType == ItemType.Weapon)
        {
            if (TryEquipWeapon(item) && isQuestItem)
            {
                RecordTemporaryItem(item, 1);
            }
            return true;
        }

        int amountLeftToAdd = amount;

        // Tenta empilhar
        if (item.stackable)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (inventoryGrid[x, y] == item && stackCounts[x, y] < item.maxStack)
                    {
                        int spaceAvailable = item.maxStack - stackCounts[x, y];
                        int amountToStack = Mathf.Min(amountLeftToAdd, spaceAvailable);

                        stackCounts[x, y] += amountToStack;
                        amountLeftToAdd -= amountToStack;

                        if (isQuestItem) RecordTemporaryItem(item, amountToStack);

                        OnItemAdded?.Invoke(item, x, y, false);
                        if (amountLeftToAdd <= 0) return true;
                    }
                }
            }
        }

        // Tenta colocar em slots vazios
        while (amountLeftToAdd > 0)
        {
            bool placed = false;
            for (int y = 0; y < gridHeight && amountLeftToAdd > 0; y++)
            {
                for (int x = 0; x < gridWidth && amountLeftToAdd > 0; x++)
                {
                    if (inventoryGrid[x, y] == null)
                    {
                        int amountToPlace = item.stackable ? Mathf.Min(amountLeftToAdd, item.maxStack) : 1;

                        inventoryGrid[x, y] = item;
                        stackCounts[x, y] = amountToPlace;
                        amountLeftToAdd -= amountToPlace;

                        if (isQuestItem) RecordTemporaryItem(item, amountToPlace);

                        OnItemAdded?.Invoke(item, x, y, false);
                        placed = true;
                        break; // Sai do loop interno para procurar o próximo slot vazio
                    }
                }
                if (placed) break; // Sai do loop externo
            }

            if (!placed)
            {
                Debug.LogWarning("Inventário cheio!");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Registra um item e sua quantidade como temporário.
    /// </summary>
    private void RecordTemporaryItem(ItemSO item, int amount)
    {
        // Se já temos esse tipo de item na lista, apenas somamos a quantidade.
        for (int i = 0; i < temporaryItems.Count; i++)
        {
            if (temporaryItems[i].item == item)
            {
                var entry = temporaryItems[i];
                entry.amount += amount;
                temporaryItems[i] = entry;
                Debug.Log($"Adicionado +{amount} de '{item.itemName}' à lista de itens temporários.");
                return;
            }
        }

        // Se não, adiciona uma nova entrada.
        temporaryItems.Add(new TemporaryItemEntry { item = item, amount = amount });
        Debug.Log($"'{item.itemName}' (x{amount}) foi registrado como item de quest temporário.");
    }

    /// <summary>
    /// Remove uma quantidade específica de um tipo de item, procurando em todo o inventário.
    /// </summary>
    private void RemoveItemByType(ItemSO itemToRemove, int amount)
    {
        int amountLeftToRemove = amount;

        // Itera de trás para frente para remover de pilhas parciais primeiro
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            for (int x = gridWidth - 1; x >= 0; x--)
            {
                if (inventoryGrid[x, y] == itemToRemove)
                {
                    int amountInSlot = stackCounts[x, y];
                    int amountToRemoveFromSlot = Mathf.Min(amountLeftToRemove, amountInSlot);

                    RemoveItemAt(x, y, amountToRemoveFromSlot);
                    amountLeftToRemove -= amountToRemoveFromSlot;

                    if (amountLeftToRemove <= 0) return;
                }
            }
        }
    }

    // O resto do seu script (TryEquipWeapon, UseItem, RemoveItemAt, PlaceHeldItemStack, etc.) continua aqui...
    // Eles já foram corrigidos na resposta anterior e não precisam de mais alterações para a lógica de quest.
    #region Resto do Código
    public int GetCountAt(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return 0;
        return stackCounts[x, y];
    }

    public ItemSO GetItemAt(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return null;
        return inventoryGrid[x, y];
    }
    public bool TryEquipWeapon(ItemSO weapon)
    {
        if (weapon.itemType != ItemType.Weapon) return false;

        if (equippedWeapon == null)
        {
            equippedWeapon = weapon;
            // weaponHandler.Equip(equippedWeapon); // Integração
            OnItemAdded?.Invoke(weapon, -1, -1, false); // -1,-1 indica slot de equipamento
            return true;
        }
        else
        {
            return false;
        }
    }
    public void UseItem(int x, int y)
    {
        ItemSO item = GetItemAt(x, y);
        if (item == null) return;

        if (item.itemType == ItemType.Consumable)
        {
            playerStats.Heal(item.healthToRestore);
            RemoveItemAt(x, y, 1);
        }
    }
    public void RemoveItemAt(int x, int y, int amount = int.MaxValue)
    {
        ItemSO item = GetItemAt(x, y);
        if (item == null) return;

        int currentStack = GetCountAt(x, y);
        int amountToRemove = Mathf.Min(amount, currentStack);

        stackCounts[x, y] -= amountToRemove;

        OnItemRemoved?.Invoke(item);

        if (stackCounts[x, y] <= 0)
        {
            inventoryGrid[x, y] = null;
        }

        OnItemAdded?.Invoke(inventoryGrid[x, y], x, y, false);
    }

    public bool PlaceHeldItemStack(int x, int y)
    {
        if (heldItem == null) return false;

        ItemSO targetSlotItem = GetItemAt(x, y);

        if (targetSlotItem == null)
        {
            inventoryGrid[x, y] = heldItem;
            stackCounts[x, y] = 1;
            OnItemAdded?.Invoke(heldItem, x, y, false);
            return true;
        }
        else if (targetSlotItem == heldItem && targetSlotItem.stackable && GetCountAt(x, y) < targetSlotItem.maxStack)
        {
            stackCounts[x, y]++;
            OnItemAdded?.Invoke(heldItem, x, y, false);
            return true;
        }

        return false;
    }

    public void StartHoldingItem(int x, int y)
    {
        if (heldItem != null) return;

        heldItem = GetItemAt(x, y);
        if (heldItem != null)
        {
            RemoveItemAt(x, y, 1);
            OnItemHeld?.Invoke(heldItem);
        }
    }

    public void DropHeldItem()
    {
        if (heldItem == null) return;

        if (!TryAddItem(heldItem))
        {
            Debug.Log("Não coube no inventário, jogou fora: " + heldItem.itemName);
        }
        heldItem = null;
        OnItemHeld?.Invoke(null);
    }
    #endregion
}