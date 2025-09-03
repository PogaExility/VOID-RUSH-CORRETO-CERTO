using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // --- EVENTOS ---
    // A UI vai ouvir esses eventos para saber quando se atualizar.
    // O int � o �NDICE do slot que mudou. Simples e eficiente.
    public event Action<int> OnBackpackSlotChanged;
    public event Action<int> OnWeaponSlotChanged;
    public event Action OnInventoryRefreshed; // Para mudan�as em massa (ex: redimensionar)

    // --- CONFIGURA��O ---
    [Header("Configura��o da Mochila")]
    [Range(1, 20)] public int gridWidth = 10;
    [Range(1, 20)] public int gridHeight = 6;

    [Header("Armas Equipadas")]
    public const int WEAPON_SLOTS_COUNT = 3;

    // --- ESTRUTURA DE DADOS (O CORA��O DO SISTEMA) ---
    // Adeus arrays 2D. Ol� lista de objetos. � serializ�vel, ent�o voc� pode ver e
    // editar o invent�rio no Inspector em tempo de execu��o para debug.
    [SerializeField] private List<InventorySlot> backpackSlots = new();
    [SerializeField] private InventorySlot[] weaponSlots = new InventorySlot[WEAPON_SLOTS_COUNT];

    private void Awake()
    {
        InitializeInventory();
    }

    /// <summary>
    /// Garante que as listas de slots existam e tenham o tamanho correto.
    /// </summary>
    private void InitializeInventory()
    {
        // Garante que a mochila tenha o tamanho certo
        int requiredSize = gridWidth * gridHeight;
        if (backpackSlots.Count != requiredSize)
        {
            ResizeAndPreserve(gridWidth, gridHeight);
        }

        // Garante que os slots de arma n�o sejam nulos
        for (int i = 0; i < WEAPON_SLOTS_COUNT; i++)
        {
            if (weaponSlots[i] == null)
            {
                weaponSlots[i] = new InventorySlot();
            }
        }
    }

    // --- API P�BLICA: FUN��ES QUE OUTROS SCRIPTS V�O USAR ---

    #region Leitura de Dados

    public int GetBackpackSize() => backpackSlots.Count;
    public InventorySlot GetBackpackSlot(int index) => (index >= 0 && index < backpackSlots.Count) ? backpackSlots[index] : null;
    public InventorySlot GetWeaponSlot(int index) => (index >= 0 && index < WEAPON_SLOTS_COUNT) ? weaponSlots[index] : null;

    /// <summary> Conta o total de um item espec�fico em toda a mochila. </summary>
    public int CountTotalAmount(ItemSO itemToCount)
    {
        if (itemToCount == null) return 0;
        int total = 0;
        foreach (var slot in backpackSlots)
        {
            if (slot.item == itemToCount)
            {
                total += slot.count;
            }
        }
        return total;
    }

    #endregion

    #region Modifica��o de Dados

    /// <summary> Tenta adicionar uma quantidade de um item � mochila. </summary>
    /// <returns>A quantidade de itens que N�O coube no invent�rio.</returns>
    public int TryAddItem(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return amount;

        // Se for uma arma, tenta equipar primeiro.
        if (item.itemType == ItemType.Weapon)
        {
            for (int i = 0; i < WEAPON_SLOTS_COUNT; i++)
            {
                if (weaponSlots[i].item == null)
                {
                    weaponSlots[i].Set(item, 1);
                    OnWeaponSlotChanged?.Invoke(i);
                    return 0; // Conseguiu equipar tudo.
                }
            }
        }

        int amountLeft = amount;

        // 1� Passada: Tenta empilhar em slots que j� cont�m o mesmo item.
        for (int i = 0; i < backpackSlots.Count && amountLeft > 0; i++)
        {
            InventorySlot slot = backpackSlots[i];
            if (slot.item == item && slot.count < item.maxStack)
            {
                int spaceAvailable = item.maxStack - slot.count;
                int amountToAdd = Mathf.Min(amountLeft, spaceAvailable);
                slot.count += amountToAdd;
                amountLeft -= amountToAdd;
                OnBackpackSlotChanged?.Invoke(i);
            }
        }

        // 2� Passada: Se ainda sobraram itens, procura por slots vazios.
        for (int i = 0; i < backpackSlots.Count && amountLeft > 0; i++)
        {
            InventorySlot slot = backpackSlots[i];
            if (slot.item == null)
            {
                int amountToAdd = Mathf.Min(amountLeft, item.maxStack);
                slot.Set(item, amountToAdd);
                amountLeft -= amountToAdd;
                OnBackpackSlotChanged?.Invoke(i);
            }
        }

        return amountLeft;
    }

    /// <summary> Remove uma quantidade de item de um slot espec�fico da mochila. </summary>
    public void RemoveFromSlot(int index, int amount)
    {
        InventorySlot slot = GetBackpackSlot(index);
        if (slot == null || slot.item == null) return;

        slot.count -= amount;
        if (slot.count <= 0)
        {
            slot.Clear();
        }
        OnBackpackSlotChanged?.Invoke(index);
    }

    /// <summary> A fun��o MAIS IMPORTANTE para Drag & Drop. Move ou funde itens entre dois slots. </summary>
    public void SwapOrMergeSlots(int fromIndex, int toIndex)
    {
        InventorySlot fromSlot = GetBackpackSlot(fromIndex);
        InventorySlot toSlot = GetBackpackSlot(toIndex);
        if (fromSlot == null || toSlot == null || fromSlot == toSlot) return;

        // Caso 1: O slot de destino est� vazio. Simplesmente move o item.
        if (toSlot.item == null)
        {
            toSlot.Set(fromSlot.item, fromSlot.count);
            fromSlot.Clear();
        }
        // Caso 2: Os itens s�o iguais e empilh�veis. Tenta fundir (merge).
        else if (fromSlot.item == toSlot.item && toSlot.item.stackable)
        {
            int spaceInToSlot = toSlot.item.maxStack - toSlot.count;
            if (spaceInToSlot > 0)
            {
                int amountToMove = Mathf.Min(fromSlot.count, spaceInToSlot);
                toSlot.count += amountToMove;
                fromSlot.count -= amountToMove;
                if (fromSlot.count <= 0) fromSlot.Clear();
            }
            // Se n�o couber nada, faz uma troca normal (swap)
            else
            {
                (fromSlot.item, toSlot.item) = (toSlot.item, fromSlot.item);
                (fromSlot.count, toSlot.count) = (toSlot.count, fromSlot.count);
            }
        }
        // Caso 3: Itens diferentes. Faz a troca (swap).
        else
        {
            (fromSlot.item, toSlot.item) = (toSlot.item, fromSlot.item);
            (fromSlot.count, toSlot.count) = (toSlot.count, fromSlot.count);
        }

        // Notifica a UI que ambos os slots mudaram.
        OnBackpackSlotChanged?.Invoke(fromIndex);
        OnBackpackSlotChanged?.Invoke(toIndex);
    }

    /// <summary> Desequipa uma arma e tenta coloc�-la na mochila. Se n�o couber, ela � perdida (ou dropada). </summary>
    public void UnequipWeapon(int weaponIndex)
    {
        InventorySlot weaponSlot = GetWeaponSlot(weaponIndex);
        if (weaponSlot == null || weaponSlot.item == null) return;

        ItemSO weaponToUnequip = weaponSlot.item;
        weaponSlot.Clear();
        OnWeaponSlotChanged?.Invoke(weaponIndex);

        // Tenta adicionar a arma de volta � mochila.
        int amountLeft = TryAddItem(weaponToUnequip, 1);

        // Se n�o coube, aqui voc� chamaria a l�gica para dropar o item no ch�o.
        if (amountLeft > 0)
        {
            Debug.LogWarning($"N�o havia espa�o para {weaponToUnequip.name} na mochila. Item foi dropado/perdido.");
            // Exemplo: ItemSpawner.Instance.SpawnItemInWorld(weaponToUnequip, transform.position, 1);
        }
    }

    #endregion

    // --- FUN��ES DE EDITOR E DEBUG ---

    /// <summary>
    /// Chamado automaticamente no Editor quando voc� altera um valor no Inspector.
    /// �til para redimensionar a grade dinamicamente sem perder os itens.
    /// </summary>
    private void OnValidate()
    {
        int requiredSize = gridWidth * gridHeight;
        if (backpackSlots.Count != requiredSize)
        {
            // Adicionado um delay para evitar problemas de timing no editor
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) // Garante que o objeto ainda existe
                {
                    ResizeAndPreserve(gridWidth, gridHeight);
                    OnInventoryRefreshed?.Invoke(); // Notifica a UI para redesenhar tudo
                }
            };
        }
    }

    private void ResizeAndPreserve(int newWidth, int newHeight)
    {
        List<InventorySlot> oldSlots = new List<InventorySlot>(backpackSlots);
        backpackSlots.Clear();

        int newSize = newWidth * newHeight;
        for (int i = 0; i < newSize; i++)
        {
            if (i < oldSlots.Count)
            {
                backpackSlots.Add(oldSlots[i]);
            }
            else
            {
                backpackSlots.Add(new InventorySlot());
            }
        }
    }
}