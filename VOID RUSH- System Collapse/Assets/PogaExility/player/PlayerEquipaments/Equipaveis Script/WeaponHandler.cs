using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerStats))]
public class WeaponHandler : MonoBehaviour
{
    public static WeaponHandler Instance;

    [Header("CONFIGURAÇÃO DE EQUIPAMENTO")]
    public const int NUM_WEAPON_SLOTS = 3;
    [SerializeField] private InventorySlot[] weaponSlots = new InventorySlot[NUM_WEAPON_SLOTS];
    public int currentWeaponIndex { get; private set; } = 0;

    [Header("REFERÊNCIAS")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform weaponSocket; // Ponto na mão do jogador
    [SerializeField] private PlayerStats playerStats; // Referência para os stats

    [Header("UI DE COMBATE (OPCIONAL)")]
    public Transform[] equipmentSlotContainers = new Transform[NUM_WEAPON_SLOTS];
    public Transform activeWeaponHUDContainer;
    public GameObject weaponItemViewPrefab;

    // Eventos que a UI ouve
    public event Action<int, ItemSO> OnEquipmentChanged;
    public event Action<int> OnActiveWeaponChanged;

    // Estado interno do combate
    private ItemSO activeWeapon;
    private GameObject activeWeaponGO;
    private float lastAttackTime = -999f;
    private int meleeComboCount = 0;
    private const float COMBO_RESET_TIME = 1.2f;

    void Awake()
    {
        Instance = this;
        // Pega referências, se não foram arrastadas
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();

        // Inicializa os slots
        for (int i = 0; i < NUM_WEAPON_SLOTS; i++)
            if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot();
    }

    void Start()
    {
        EquipToHand(weaponSlots[currentWeaponIndex].item);
        // A UI de armas agora vai ter seu próprio script dedicado.
    }

    public void CycleWeapon(bool forward)
    {
        if (forward) currentWeaponIndex = (currentWeaponIndex + 1) % NUM_WEAPON_SLOTS;
        else currentWeaponIndex = (currentWeaponIndex - 1 + NUM_WEAPON_SLOTS) % NUM_WEAPON_SLOTS;

        EquipToHand(weaponSlots[currentWeaponIndex].item);
    
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
      

        if (currentWeaponIndex == weaponSlotIndex)
        {
            EquipToHand(weaponSlots[currentWeaponIndex].item);
        }
    }

    // >> NOVA LÓGICA DE ATAQUE (DO ANTIGO PLAYERATTACK) <<
    public void HandleAttackInput(Vector3 aimDirection)
    {
        if (activeWeapon == null) return; // Não ataca se não tiver arma
        if (Time.time < lastAttackTime + activeWeapon.attackRate) return; // Respeita a cadência

        switch (activeWeapon.weaponType)
        {
            case WeaponType.Melee:
                // Lógica de combo
                if (Time.time > lastAttackTime + COMBO_RESET_TIME) meleeComboCount = 0;
                int animationIndex = meleeComboCount % activeWeapon.comboAnimations.Length;
                Debug.Log($"Ataque Melee Combo {animationIndex + 1}");
                // TODO: Chamar animação: animator.Play(activeWeapon.comboAnimations[animationIndex].name);
                meleeComboCount++;
                break;

            case WeaponType.Ranger:
                Debug.Log("Atirou com Ranger!");
                // TODO: Chamar animação de tiro e instanciar `activeWeapon.bulletPrefab`
                break;

            case WeaponType.Buster:
                Debug.Log("Atirou com Buster!");
                // TODO: Lógica de carga e tiro do buster
                break;
        }

        lastAttackTime = Time.time;
    }

    private void EquipToHand(ItemSO weapon)
    {
        if (activeWeaponGO != null) Destroy(activeWeaponGO);
        activeWeapon = weapon;
        if (activeWeapon != null && activeWeapon.equipPrefab != null && weaponSocket != null)
        {
            activeWeaponGO = Instantiate(activeWeapon.equipPrefab, weaponSocket);
        }
    }

    // --- Funções Helper ---
    public InventorySlot GetActiveWeaponSlot() => weaponSlots[currentWeaponIndex];
}