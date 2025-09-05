using System;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("Referências Essenciais")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform weaponSocket;

    [Header("Runtime - Armas Equipadas")]
    public const int NUM_WEAPON_SLOTS = 3;
    [SerializeField] private InventorySlot[] weaponSlots = new InventorySlot[NUM_WEAPON_SLOTS];
    [SerializeField] public int currentWeaponIndex = 0; // TORNE-A PÚBLICA

    // VARIÁVEL CORRIGIDA: Usa o que já existe
    private ItemSO activeEquippedWeapon;
    private GameObject activeWeaponGO;

    // EVENTOS PARA A UI OUVIR
    public event Action<int, ItemSO> OnEquipmentChanged;
    public event Action<int> OnActiveWeaponChanged;

    // DEFINIÇÃO FALTANTE: A interface para dano
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
    // Em WeaponHandler.cs, adicione esta função pública
    public InventorySlot GetActiveWeaponSlot()
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= weaponSlots.Length)
            return null;

        return weaponSlots[currentWeaponIndex];
    }
    void Awake()
    {
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;
        for (int i = 0; i < NUM_WEAPON_SLOTS; i++)
        {
            if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot();
        }
        // Equipa a primeira arma no início do jogo, se houver
        EquipWeaponFromSlot(currentWeaponIndex);
    }

    // --- LÓGICA DE EQUIPAR/TROCAR ---

    public void CycleWeapon(bool forward)
    {
        if (forward) currentWeaponIndex = (currentWeaponIndex + 1) % NUM_WEAPON_SLOTS;
        else currentWeaponIndex = (currentWeaponIndex - 1 + NUM_WEAPON_SLOTS) % NUM_WEAPON_SLOTS;

        EquipWeaponFromSlot(currentWeaponIndex);
        OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
    }

    public void EquipItemFromMouse(int weaponSlotIndex)
    {
        var itemNoMouse = inventoryManager.GetHeldItem(); // Pergunta ao InvManager o que está no mouse
        var equipmentSlot = weaponSlots[weaponSlotIndex];

        // Só permite a troca se o item do mouse for uma arma
        if (itemNoMouse.item != null && itemNoMouse.item.itemType != ItemType.Weapon)
            return;

        // Troca os DADOS: o que estava no mouse vai para o slot de arma,
        // E o que estava no slot de arma vai para o mouse.
        (itemNoMouse.item, equipmentSlot.item) = (equipmentSlot.item, itemNoMouse.item);
        (itemNoMouse.count, equipmentSlot.count) = (equipmentSlot.count, itemNoMouse.count);

        // Se a arma que trocamos era a que estava ativa, atualiza a mão do jogador
        if (currentWeaponIndex == weaponSlotIndex)
        {
            EquipWeaponFromSlot(currentWeaponIndex);
        }

        // Avisa os sistemas de UI que os dados mudaram
        inventoryManager.RequestRedraw();
        OnEquipmentChanged?.Invoke(weaponSlotIndex, equipmentSlot.item);
    }

    // --- LÓGICA INTERNA ---

    private void EquipWeaponFromSlot(int index)
    {
        UnequipActiveWeapon();
        var weaponToEquip = weaponSlots[index];
        if (weaponToEquip == null || weaponToEquip.item == null) return;

        // NOME DA VARIÁVEL CORRIGIDO
        activeEquippedWeapon = weaponToEquip.item;
        if (activeEquippedWeapon.equipPrefab != null && weaponSocket != null)
        {
            activeWeaponGO = Instantiate(activeEquippedWeapon.equipPrefab, weaponSocket);
        }
    }

    private void UnequipActiveWeapon()
    {
        if (activeWeaponGO != null) Destroy(activeWeaponGO);
        activeEquippedWeapon = null;
    }

    // --- LÓGICA DE COMBATE ---
    public void Fire(Vector3 origin, Vector3 direction)
    {
        if (activeEquippedWeapon == null) return;

        if (TryConsumeOneAmmo())
        {
            float dmg = activeEquippedWeapon.bulletDamage;
            if (Physics.Raycast(origin, direction, out var hit, 200f))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var hp)) hp.TakeDamage(dmg);
            }
        }
        else
        {
            float range = activeEquippedWeapon.powderRange;
            float pdmg = activeEquippedWeapon.powderDamage;
            if (Physics.Raycast(origin, direction, out var phit, range))
            {
                if (phit.collider.TryGetComponent<IDamageable>(out var hp)) hp.TakeDamage(pdmg);
            }
        }
    }

    private bool TryConsumeOneAmmo()
    {
        if (activeEquippedWeapon == null || activeEquippedWeapon.acceptedAmmo == null) return false;
        foreach (var ammo in activeEquippedWeapon.acceptedAmmo)
        {
            // A lógica de consumir precisa ser refeita para o TryAddItem atual
            // if (inventoryManager.TryConsumeItem(ammo, 1)) return true;
        }
        return false; // Simulação por agora
    }
}