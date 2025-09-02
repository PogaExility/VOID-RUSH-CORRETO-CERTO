using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // --- Eventos para UI e sistemas ---
    public event Action<ItemSO, int, int> OnBackpackItemChanged; // item, x, y
    public event Action<ItemSO, int> OnItemHeld;            // item, amount (ghost)
    public event Action<ItemSO, int> OnWeaponSlotChanged;   // se você quiser ligar com a HUD depois
    public event Action<ItemSO> OnItemRemoved;   // ADICIONE este evento
    private int[,] tempCounts;

    [Header("Configuração do Grid")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    // --- Estrutura de dados 1×1 (Terraria) ---
    private ItemSO[,] inventoryGrid;
    private int[,] stackCounts;

    [Header("Armas equipadas")]
    public const int WEAPON_SLOTS_COUNT = 3;
    public ItemSO[] equippedWeapons = new ItemSO[WEAPON_SLOTS_COUNT];

    // --- Estado de arrasto/ghost ---
    public ItemSO heldItem { get; private set; }
    public int heldCount { get; private set; }

    // --- Refs auxiliares ---
    private PlayerStats playerStats;
    private Transform playerTransform;

    void Awake()
    {
        EnsureGridExists();
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats) playerTransform = playerStats.transform;
    }

    public void EnsureGridExists()
    {
        if (inventoryGrid == null || inventoryGrid.GetLength(0) != gridWidth || inventoryGrid.GetLength(1) != gridHeight)
            inventoryGrid = new ItemSO[gridWidth, gridHeight];
        if (stackCounts == null || stackCounts.GetLength(0) != gridWidth || stackCounts.GetLength(1) != gridHeight)
            stackCounts = new int[gridWidth, gridHeight];

        // ADICIONE isto:
        if (tempCounts == null || tempCounts.GetLength(0) != gridWidth || tempCounts.GetLength(1) != gridHeight)
            tempCounts = new int[gridWidth, gridHeight];
    }

    // ---------- GETTERS usados pela UI ----------
    public ItemSO GetItemAt(int x, int y) => inventoryGrid[x, y];
    public int GetCountAt(int x, int y) => stackCounts[x, y];

    // ---------- Adição (pickup/baú/loja) ----------
    public bool TryAddItem(ItemSO item, int amount = 1)
    {
        EnsureGridExists();
        if (item == null || amount <= 0) return false;

        // Arma: tenta equipar direto (backpack não guarda arma)
        if (item.itemType == ItemType.Weapon)
        {
            for (int i = 0; i < WEAPON_SLOTS_COUNT; i++)
            {
                if (equippedWeapons[i] == null)
                {
                    equippedWeapons[i] = item;
                    OnWeaponSlotChanged?.Invoke(item, i);
                    return true;
                }
            }
            return false; // slots de arma cheios
        }

        int left = amount;
        // 1) completa pilhas existentes
        for (int y = 0; y < gridHeight && left > 0; y++)
            for (int x = 0; x < gridWidth && left > 0; x++)
            {
                if (inventoryGrid[x, y] == item && stackCounts[x, y] < item.maxStack)
                {
                    int space = item.maxStack - stackCounts[x, y];
                    int add = Mathf.Min(space, left);
                    stackCounts[x, y] += add;
                    left -= add;
                    OnBackpackItemChanged?.Invoke(item, x, y);
                }
            }
        // 2) preenche slots vazios
        for (int y = 0; y < gridHeight && left > 0; y++)
            for (int x = 0; x < gridWidth && left > 0; x++)
            {
                if (inventoryGrid[x, y] == null)
                {
                    inventoryGrid[x, y] = item;
                    int put = Mathf.Min(item.maxStack, left);
                    stackCounts[x, y] = put;
                    left -= put;
                    OnBackpackItemChanged?.Invoke(item, x, y);
                }
            }

        return left == 0;
    }

    // ---------- Remoção/uso ----------
    public void RemoveItemAt(int x, int y, int amount = 1)
    {
        if (inventoryGrid[x, y] == null || amount <= 0) return;
        stackCounts[x, y] -= amount;
        if (stackCounts[x, y] <= 0)
        {
            inventoryGrid[x, y] = null;
            stackCounts[x, y] = 0;
        }
        OnBackpackItemChanged?.Invoke(inventoryGrid[x, y], x, y);
    }

    public void UseItem(int x, int y)
    {
        var it = GetItemAt(x, y);
        if (it == null) return;

        if (it.itemType == ItemType.Consumable && playerStats != null)
        {
            playerStats.Heal(it.healthToRestore);
            RemoveItemAt(x, y, 1);
        }
        // outros usos (buffs etc.) entram aqui depois
    }

    // ---------- Pegar/soltar (drag & drop) ----------
    public void StartHoldingItem(int x, int y, bool halfStack)
    {
        var it = GetItemAt(x, y);
        if (it == null || heldItem != null) return;

        int take = halfStack ? Mathf.Max(1, GetCountAt(x, y) / 2) : GetCountAt(x, y);
        heldItem = it;
        heldCount = take;

        stackCounts[x, y] -= take;
        if (stackCounts[x, y] <= 0) { inventoryGrid[x, y] = null; stackCounts[x, y] = 0; }
        OnBackpackItemChanged?.Invoke(inventoryGrid[x, y], x, y);
        OnItemHeld?.Invoke(heldItem, heldCount);
    }

    public void PlaceHeldItem(int x, int y)
    {
        if (heldItem == null) return;

        var cellItem = GetItemAt(x, y);
        if (cellItem == null)
        {
            inventoryGrid[x, y] = heldItem;
            int put = Mathf.Min(heldItem.maxStack, heldCount);
            stackCounts[x, y] = put;
            heldCount -= put;
        }
        else if (cellItem == heldItem && heldItem.stackable && stackCounts[x, y] < heldItem.maxStack)
        {
            int space = heldItem.maxStack - stackCounts[x, y];
            int put = Mathf.Min(space, heldCount);
            stackCounts[x, y] += put;
            heldCount -= put;
        }

        OnBackpackItemChanged?.Invoke(GetItemAt(x, y), x, y);

        if (heldCount <= 0) { heldItem = null; heldCount = 0; }
        OnItemHeld?.Invoke(heldItem, heldCount);
    }

    public void ReturnHeldItem()
    {
        if (heldItem == null) return;
        // tenta devolver por autostack + slots vazios
        TryAddItem(heldItem, heldCount);
        heldItem = null;
        heldCount = 0;
        OnItemHeld?.Invoke(null, 0);
    }

    public void DropHeldItemToWorld()
    {
        if (heldItem == null || heldCount <= 0) return;

        // usa teu spawner padrão (ajuste se seu projeto usar outro ponto de origem)
        Vector3 pos = playerTransform ? playerTransform.position : Vector3.zero;
        ItemSpawner.Instance.SpawnItemInWorld(heldItem, pos, heldCount);

        heldItem = null;
        heldCount = 0;
        OnItemHeld?.Invoke(null, 0);
    }

    // ---------- Helpers para armas (ammo) ----------
    public int CountItem(ItemSO item)
    {
        if (item == null) return 0;
        int total = 0;
        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
                if (inventoryGrid[x, y] == item) total += stackCounts[x, y];
        return total;
    }

    public bool TryConsumeItem(ItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        if (CountItem(item) < amount) return false;

        int left = amount;
        for (int y = 0; y < gridHeight && left > 0; y++)
            for (int x = 0; x < gridWidth && left > 0; x++)
            {
                if (inventoryGrid[x, y] != item) continue;
                int take = Mathf.Min(stackCounts[x, y], left);
                stackCounts[x, y] -= take;
                left -= take;
                if (stackCounts[x, y] <= 0) { inventoryGrid[x, y] = null; stackCounts[x, y] = 0; }
                OnBackpackItemChanged?.Invoke(inventoryGrid[x, y], x, y);
            }
        return true;
    }

    public void CommitTemporaryItems()
    {
        // Tudo que era “temporário” até aqui vira permanente (zera contagens temp).
        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
                tempCounts[x, y] = 0;
    }

    public void ClearTemporaryItems()
    {
        // Remove APENAS a parte temporária das pilhas
        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
            {
                int temp = tempCounts[x, y];
                if (temp <= 0) continue;

                stackCounts[x, y] = Mathf.Max(0, stackCounts[x, y] - temp);
                tempCounts[x, y] = 0;

                if (stackCounts[x, y] == 0 && inventoryGrid[x, y] != null)
                {
                    var it = inventoryGrid[x, y];
                    inventoryGrid[x, y] = null;
                    OnItemRemoved?.Invoke(it);
                }
                OnBackpackItemChanged?.Invoke(inventoryGrid[x, y], x, y);
            }
    }
}
