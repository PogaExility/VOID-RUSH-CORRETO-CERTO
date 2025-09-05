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
    public GameObject weaponItemViewPrefab;

    private List<GameObject> activeVisuals = new List<GameObject>();

    // >> OS EVENTOS QUE O HUD PRECISA <<
    public event Action<int> OnActiveWeaponChanged;

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < NUM_WEAPON_SLOTS; i++)
            if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot();
    }

    void Start()
    {
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;
        RedrawWeaponUI();
    }

    private void RedrawWeaponUI()
    {
        foreach (var visual in activeVisuals) Destroy(visual);
        activeVisuals.Clear();

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

        var activeWeaponData = weaponSlots[currentWeaponIndex];
        if (activeWeaponData.item != null)
        {
            var go = Instantiate(weaponItemViewPrefab, activeWeaponHUDContainer);
            go.GetComponent<WeaponItemView>().Render(activeWeaponData.item);
            activeVisuals.Add(go);
        }
    }

    public void CycleWeapon(bool forward)
    {
        if (forward) currentWeaponIndex = (currentWeaponIndex + 1) % NUM_WEAPON_SLOTS;
        else currentWeaponIndex = (currentWeaponIndex - 1 + NUM_WEAPON_SLOTS) % NUM_WEAPON_SLOTS;

        EquipToHand(weaponSlots[currentWeaponIndex].item);
        RedrawWeaponUI();
        OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
    }

    public void EquipItemFromMouse(int weaponSlotIndex)
    {
        var itemNoMouse = inventoryManager.GetHeldItem();
        var equipmentSlot = weaponSlots[weaponSlotIndex];
        if (itemNoMouse.item != null && itemNoMouse.item.itemType != ItemType.Weapon) return;

        (itemNoMouse.item, equipmentSlot.item) = (equipmentSlot.item, itemNoMouse.item);
        (itemNoMouse.count, equipmentSlot.count) = (equipmentSlot.count, itemNoMouse.count);

        inventoryManager.RequestRedraw(); // <<-- Usa a função pública, sem erro
        RedrawWeaponUI();

        if (currentWeaponIndex == weaponSlotIndex)
        {
            EquipToHand(weaponSlots[currentWeaponIndex].item);
        }
    }

    private void EquipToHand(ItemSO weapon) { /* ...Lógica de Instantiate no socket... */ }

    // >> A FUNÇÃO QUE O HUD PRECISA, COM O NOME CERTO <<
    public InventorySlot GetActiveWeaponSlot() => weaponSlots[currentWeaponIndex];
}