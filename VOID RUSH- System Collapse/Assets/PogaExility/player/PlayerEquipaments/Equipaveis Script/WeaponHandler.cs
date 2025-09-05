using System;
using UnityEngine;
using System.Collections.Generic;

public class WeaponHandler : MonoBehaviour
{
    public static WeaponHandler Instance;

    [Header("CONFIGURAÇÃO DE DADOS")]
    public const int NUM_WEAPON_SLOTS = 3;
    [SerializeField] private InventorySlot[] weaponSlots = new InventorySlot[NUM_WEAPON_SLOTS];
    public int currentWeaponIndex { get; private set; } = 0;

    [Header("REFERÊNCIAS DO MUNDO")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform weaponSocket;

    [Header("REFERÊNCIAS DA UI DE COMBATE (ARRASTE AQUI)")]
    public Transform[] equipmentSlotContainers = new Transform[NUM_WEAPON_SLOTS];
    public Transform activeWeaponHUDContainer;
    public GameObject weaponItemViewPrefab; // <<-- O PREFAB VISUAL DA ARMA

    private List<GameObject> activeVisuals = new List<GameObject>();
    private ItemSO activeWeapon;
    private GameObject activeWeaponGO;
    public event Action<int> OnActiveWeaponChanged;
    public InventorySlot GetActiveWeaponSlot() => weaponSlots[currentWeaponIndex];


    void Awake()
    {
        Instance = this;
        for (int i = 0; i < NUM_WEAPON_SLOTS; i++)
            if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot();
    }
    public void EquipItemFromMouse(int weaponSlotIndex)
    {
        var itemNoMouse = inventoryManager.GetHeldItem();
        var equipmentSlot = weaponSlots[weaponSlotIndex];

        if (itemNoMouse.item != null && itemNoMouse.item.itemType != ItemType.Weapon)
            return;

        (itemNoMouse.item, equipmentSlot.item) = (equipmentSlot.item, itemNoMouse.item);
        (itemNoMouse.count, equipmentSlot.count) = (equipmentSlot.count, itemNoMouse.count);

        inventoryManager.RequestRedraw();
        // RedrawWeaponUI(); // Você ainda precisa de uma função para desenhar a UI de armas

        if (currentWeaponIndex == weaponSlotIndex)
        {
            // >> A CORREÇÃO ESTÁ AQUI <<
            // A gente já tem uma função pra isso, não precisa de outra.
            EquipToHand(weaponSlots[currentWeaponIndex].item);
            OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
        }
    }
    void Start()
    {
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;
        RedrawWeaponUI();
    }

    // A FUNÇÃO QUE CRIA A PORRA DOS ITENS VISUAIS ONDE DEVEM ESTAR
    private void RedrawWeaponUI()
    {
        // 1. Destrói todos os visuais antigos para evitar duplicação.
        foreach (var visual in activeVisuals) Destroy(visual);
        activeVisuals.Clear();

        // 2. Cria os visuais para os 3 slots no INVENTÁRIO.
        for (int i = 0; i < NUM_WEAPON_SLOTS; i++)
        {
            var data = weaponSlots[i];
            if (data.item != null)
            {
                var go = Instantiate(weaponItemViewPrefab, equipmentSlotContainers[i]);
                go.GetComponent<WeaponItemView>().Render(data.item);
                activeVisuals.Add(go);
            }
        }

        // 3. Cria o visual para a arma ATIVA na HOTBAR.
        var activeWeaponData = weaponSlots[currentWeaponIndex];
        if (activeWeaponData.item != null)
        {
            var go = Instantiate(weaponItemViewPrefab, activeWeaponHUDContainer);
            go.GetComponent<WeaponItemView>().Render(activeWeaponData.item);
            activeVisuals.Add(go);
        }
    }

    // --- LÓGICA DE INTERAÇÃO ---

    public void CycleWeapon(bool forward)
    {
        if (forward) currentWeaponIndex = (currentWeaponIndex + 1) % NUM_WEAPON_SLOTS;
        else currentWeaponIndex = (currentWeaponIndex - 1 + NUM_WEAPON_SLOTS) % NUM_WEAPON_SLOTS;

        EquipToHand(weaponSlots[currentWeaponIndex].item);
        RedrawWeaponUI();
    }

    public void EquipItemFromBackpack(int backpackSlotIndex, int weaponSlotIndex)
    {
        var backpackSlot = inventoryManager.GetBackpackSlot(backpackSlotIndex);
        if (backpackSlot.item != null && backpackSlot.item.itemType != ItemType.Weapon) return;

        (backpackSlot.item, weaponSlots[weaponSlotIndex].item) = (weaponSlots[weaponSlotIndex].item, backpackSlot.item);
        (backpackSlot.count, weaponSlots[weaponSlotIndex].count) = (weaponSlots[weaponSlotIndex].count, backpackSlot.count);

        inventoryManager.RequestRedraw();
        RedrawWeaponUI();

        if (currentWeaponIndex == weaponSlotIndex)
        {
            EquipToHand(weaponSlots[currentWeaponIndex].item);
        }
    }

    private void EquipToHand(ItemSO weapon)
    {
        if (activeWeaponGO != null) Destroy(activeWeaponGO);
        activeWeapon = weapon;
        if (activeWeapon != null && activeWeapon.equipPrefab != null && weaponSocket != null)
        {
            activeWeaponGO = Instantiate(activeWeapon.equipPrefab, weaponSocket);
        }
        // UpdateAimState(); // Se tiver, mantenha aqui
    }

}