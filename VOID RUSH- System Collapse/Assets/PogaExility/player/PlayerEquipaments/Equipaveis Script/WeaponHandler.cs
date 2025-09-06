using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("REFERÊNCIAS DE COMBATE")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform playerVisualsTransform; // O Sprite do Player
    [SerializeField] private GameObject headPivot;
    [SerializeField] private GameObject armPivot;

    [Header("ESTADO DE COMBATE")]
    [SerializeField] private bool isInAimMode = false;
    private int currentAmmo;
    private bool isReloading = false;
    private float lastAttackTime = -999f;
    private int meleeComboCount = 0;
    private const float COMBO_RESET_TIME = 1.2f;

    [Header("REFERÊNCIAS DA UI DE COMBATE (ARRASTE AQUI)")]
    public Transform[] equipmentSlotContainers = new Transform[NUM_WEAPON_SLOTS];
    public Transform activeWeaponHUDContainer;
    public GameObject weaponItemViewPrefab; // <<-- O PREFAB VISUAL DA ARMA

    private List<GameObject> activeVisuals = new List<GameObject>();
    private ItemSO activeWeapon;
    private GameObject activeWeaponGO;
    public event Action<int> OnActiveWeaponChanged;
    public InventorySlot GetActiveWeaponSlot() => weaponSlots[currentWeaponIndex];

   

    // Adicione esta nova função
    private void AimLogic()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDirection = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        if (headPivot != null) headPivot.transform.rotation = Quaternion.Euler(0, 0, angle);
        if (armPivot != null) armPivot.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (playerVisualsTransform != null)
        {
            if (mouseWorldPos.x < transform.position.x)
                playerVisualsTransform.localScale = new Vector3(-1, 1, 1);
            else
                playerVisualsTransform.localScale = new Vector3(1, 1, 1);
        }
    }
    void Awake()
    {
        Instance = this;
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;
        for (int i = 0; i < NUM_WEAPON_SLOTS; i++)
            if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot();
    }

    void Start() => EquipToHand(weaponSlots[currentWeaponIndex].item);

    void Update()
    {
        UpdateAimState();
        if (isInAimMode) AimLogic();
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
        RedrawWeaponUI();

        if (currentWeaponIndex == weaponSlotIndex)
        {
            // >> A CORREÇÃO ESTÁ AQUI <<
            // A gente já tem uma função pra isso, não precisa de outra.
            EquipToHand(weaponSlots[currentWeaponIndex].item);
            OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
        }
    }
   

    // A FUNÇÃO QUE CRIA A PORRA DOS ITENS VISUAIS ONDE DEVEM ESTAR
    private void RedrawWeaponUI()
    {
        foreach (var visual in activeVisuals) Destroy(visual);
        activeVisuals.Clear();

        // 1. Redesenha os 3 slots no inventário
        for (int i = 0; i < NUM_WEAPON_SLOTS; i++)
        {
            // >> LINHA DE SEGURANÇA <<
            if (equipmentSlotContainers[i] == null)
            {
                Debug.LogError($"FATAL: O 'Equipment Slot Container' de índice {i} está faltando no Inspector do WeaponHandler!", this);
                continue; // Pula para o próximo para não quebrar tudo
            }

            var data = weaponSlots[i];
            if (data.item != null)
            {
                var go = Instantiate(weaponItemViewPrefab, equipmentSlotContainers[i]);
                go.GetComponent<WeaponItemView>().Render(data.item);
                activeVisuals.Add(go);
            }
        }

        // 2. Redesenha a hotbar
        // >> LINHA DE SEGURANÇA <<
        if (activeWeaponHUDContainer == null)
        {
            Debug.LogError("FATAL: O 'Active Weapon HUD Container' está faltando no Inspector do WeaponHandler!", this);    
        }
        else
        {
            var activeWeaponData = weaponSlots[currentWeaponIndex];
            if (activeWeaponData.item != null)
            {
                var go = Instantiate(weaponItemViewPrefab, activeWeaponHUDContainer);
                go.GetComponent<WeaponItemView>().Render(activeWeaponData.item);
                activeVisuals.Add(go);
            }
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
    private void UpdateAimState()
    {
        bool shouldAim = activeWeapon != null && activeWeapon.useAimMode;
        if (isInAimMode != shouldAim)
        {
            SetAimMode(shouldAim);
        }
    }

    private void SetAimMode(bool shouldBeAiming)
    {
        isInAimMode = shouldBeAiming;

        if (playerController != null)
            playerController.SetAimingState(isInAimMode);

        if (headPivot != null) headPivot.SetActive(isInAimMode);
        if (armPivot != null) armPivot.SetActive(isInAimMode);

        var cursorManager = playerController?.cursorManager;
        if (cursorManager != null)
        {
            if (isInAimMode)
                cursorManager.SetAimCursor();
            else
                cursorManager.SetDefaultCursor();
        }
    }

    private void EquipToHand(ItemSO weapon)
    {
        // Limpa a arma antiga
        if (activeWeaponGO != null) Destroy(activeWeaponGO);
        activeWeapon = weapon;

        // Se não tiver arma para equipar, para aqui.
        if (activeWeapon == null)
        {
            UpdateAimState(); // Garante que saia do modo mira se desequipou.
            return;
        }

        // Se a arma existe, mas não tem prefab ou não há onde encaixar, para aqui.
        if (activeWeapon.equipPrefab == null || weaponSocket == null)
        {
            UpdateAimState();
            return;
        }

        // 1. CRIA a arma como filha do socket.
        activeWeaponGO = Instantiate(activeWeapon.equipPrefab, weaponSocket);

        // 2. FORÇA a posição, rotação e escala para valores neutros.
        activeWeaponGO.transform.localPosition = Vector3.zero;
        activeWeaponGO.transform.localRotation = Quaternion.identity;
        activeWeaponGO.transform.localScale = Vector3.one;

        // 3. ATUALIZA o estado de mira.
        UpdateAimState();
    }

    // Adicione esta nova função
   
    public void HandleAttackInput()
    {
        if (activeWeapon == null || isReloading || Time.time < lastAttackTime + activeWeapon.attackRate) return;

        if (activeWeapon.weaponType == WeaponType.Ranger)
        {
            if (currentAmmo > 0) FireBullet();
            else FirePowder();
        }

        lastAttackTime = Time.time;
    }

    private void FireBullet()
    {
        currentAmmo--;
        Debug.Log("TIRO! Munição: " + currentAmmo);
        OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
    }
    private void FirePowder() => Debug.Log("TIRO DE PÓLVORA!");

    public void HandleReloadInput()
    {
        if (activeWeapon == null || activeWeapon.weaponType != WeaponType.Ranger || isReloading) return;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log("Recarregando...");
        yield return new WaitForSeconds(activeWeapon.reloadTime);
        currentAmmo = activeWeapon.magazineSize;
        isReloading = false;
        Debug.Log("Recarga completa!");
        OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
    }
}