using System;
using System.Linq;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public static WeaponHandler Instance;

    [Header("CONFIGURAÇÃO DE EQUIPAMENTO")]
    public const int NUM_WEAPON_SLOTS = 3;
    [SerializeField] private InventorySlot[] weaponSlots = new InventorySlot[3];

    [Header("MUNIÇÃO")]
    [SerializeField] private InventorySlot[] ammoSlots = new InventorySlot[4];

    [Header("REFERÊNCIAS ESSENCIAIS")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform weaponSocket; // A "mão" onde a arma é criada.

    // A instância da arma ativa em cena.
    private WeaponBase activeWeaponInstance;
    // O renderer da arma ativa, para controle de camadas.
    private SpriteRenderer activeWeaponRenderer;

    public int currentWeaponIndex { get; private set; } = 0;
    private bool isInAimMode = false;

    // Eventos para a UI se atualizar.
    public event Action<int> OnActiveWeaponChanged;
    public event Action OnAmmoSlotsChanged;
    public event Action OnWeaponSlotsChanged;

    void Awake()
    {
        Instance = this;
        if (playerController == null) playerController = GetComponent<PlayerController>();

        // Inicializa os slots para evitar erros.
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot();
        }
        for (int i = 0; i < ammoSlots.Length; i++)
        {
            if (ammoSlots[i] == null) ammoSlots[i] = new InventorySlot();
        }
    }

    void Start()
    {
        EquipToHand(weaponSlots[currentWeaponIndex].item);
    }

    void Update()
    {
        // A lógica de mira só é executada se o jogador tiver uma arma que usa mira.
        var activeWeaponData = GetActiveWeaponSlot()?.item;
        if (isInAimMode && activeWeaponData != null && activeWeaponData.useAimMode)
        {
            AimLogic();
        }
    }

    private void AimLogic()
    {
        if (activeWeaponInstance == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 1. DÁ A ORDEM PARA VIRAR: O Handler pede, o Movement executa.
        playerController.movementScript.FaceTowardsPoint(mouseWorldPos);

        // 2. PERGUNTA A DIREÇÃO ATUAL: Pega a direção correta APÓS o flip.
        bool isFacingRight = playerController.movementScript.IsFacingRight();
        float currentDirectionX = isFacingRight ? 1f : -1f;

        // 3. CALCULA O ÂNGULO DA ARMA: Usa a direção correta como referência.
        Vector2 playerForwardDirection = new Vector2(currentDirectionX, 0);
        Vector2 directionToMouse = (mouseWorldPos - weaponSocket.position);
        float armAngle = Vector2.SignedAngle(playerForwardDirection, directionToMouse);
        float clampedArmAngle = Mathf.Clamp(armAngle, -90f, 90f);

        // 4. APLICA A ROTAÇÃO: A mira é aplicada localmente no braço.
        weaponSocket.localRotation = Quaternion.Euler(0, 0, clampedArmAngle);
    }

    public void HandleAttackInput()
    {
        if (activeWeaponInstance != null)
        {
            activeWeaponInstance.Attack();
        }
    }

    public void HandleReloadInput()
    {
        if (activeWeaponInstance is RangedWeapon rangedWeapon)
        {
            int ammoNeeded = rangedWeapon.GetAmmoNeeded();
            if (ammoNeeded <= 0) return; // Pente cheio

            int ammoFound = FindAndConsumeAmmo(ammoNeeded);

            if (ammoFound > 0)
            {
                rangedWeapon.StartReload(ammoFound);
            }
        }
    }

    private int FindAndConsumeAmmo(int maxAmountNeeded)
    {
        var weaponData = GetActiveWeaponSlot()?.item;
        if (weaponData == null || weaponData.acceptedAmmo == null || weaponData.acceptedAmmo.Length == 0) return 0;

        int totalConsumed = 0;
        for (int i = 0; i < ammoSlots.Length; i++)
        {
            var ammoSlot = ammoSlots[i];
            if (ammoSlot.item == null) continue;

            if (weaponData.acceptedAmmo.Contains(ammoSlot.item))
            {
                int amountToConsume = Mathf.Min(maxAmountNeeded - totalConsumed, ammoSlot.count);
                ammoSlot.count -= amountToConsume;
                totalConsumed += amountToConsume;

                if (ammoSlot.count <= 0) ammoSlot.Clear();
                if (totalConsumed >= maxAmountNeeded) break;
            }
        }

        if (totalConsumed > 0)
        {
            OnAmmoSlotsChanged?.Invoke();
        }
        return totalConsumed;
    }

    public void EquipToHand(ItemSO weaponData)
    {
        if (activeWeaponInstance != null)
        {
            activeWeaponInstance.OnWeaponStateChanged -= HandleWeaponStateChange;
            Destroy(activeWeaponInstance.gameObject);
            activeWeaponInstance = null;
            activeWeaponRenderer = null;
        }

        if (weaponData == null || weaponData.equipPrefab == null)
        {
            SetAimMode(false);
            OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
            return;
        }

        GameObject weaponGO = Instantiate(weaponData.equipPrefab, weaponSocket);
        weaponGO.transform.localPosition = Vector3.zero;
        weaponGO.transform.localRotation = Quaternion.identity;

        // CORREÇÃO: Pega o renderer do Player a partir da referência do PlayerController.
        activeWeaponRenderer = weaponGO.GetComponentInChildren<SpriteRenderer>();
        var playerRenderer = playerController.GetComponent<SpriteRenderer>();

        if (activeWeaponRenderer != null && playerRenderer != null)
        {
            activeWeaponRenderer.sortingLayerName = playerRenderer.sortingLayerName;
            activeWeaponRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
        }

        activeWeaponInstance = weaponGO.GetComponent<WeaponBase>();
        if (activeWeaponInstance != null)
        {
            activeWeaponInstance.Initialize(weaponData);
            activeWeaponInstance.OnWeaponStateChanged += HandleWeaponStateChange;
        }
        else
        {
            Debug.LogError($"Prefab '{weaponData.name}' não tem script derivado de WeaponBase!");
            Destroy(weaponGO);
            return;
        }

        SetAimMode(weaponData.useAimMode);
        OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
    }

    private void HandleWeaponStateChange()
    {
        OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
    }

    public void CycleWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % weaponSlots.Length;
        EquipToHand(weaponSlots[currentWeaponIndex].item);
    }

    public void EquipItemFromMouse(int weaponSlotIndex)
    {
        var itemNoMouse = inventoryManager.GetHeldItem();
        var equipmentSlot = weaponSlots[weaponSlotIndex];
        if (itemNoMouse.item != null && itemNoMouse.item.itemType != ItemType.Weapon) return;

        ItemSO tempItem = equipmentSlot.item;
        int tempCount = equipmentSlot.count;
        equipmentSlot.Set(itemNoMouse.item, itemNoMouse.count);
        itemNoMouse.Set(tempItem, tempCount);

        inventoryManager.RequestRedraw();
        OnWeaponSlotsChanged?.Invoke();

        if (currentWeaponIndex == weaponSlotIndex)
        {
            EquipToHand(weaponSlots[currentWeaponIndex].item);
        }
    }

    public void EquipAmmoFromMouse(int ammoSlotIndex)
    {
        var itemOnMouse = inventoryManager.GetHeldItem();
        var ammoSlot = ammoSlots[ammoSlotIndex];
        if (itemOnMouse.item != null && itemOnMouse.item.itemType != ItemType.Ammo) return;

        ItemSO tempItem = ammoSlot.item;
        int tempCount = ammoSlot.count;
        ammoSlot.Set(itemOnMouse.item, itemOnMouse.count);
        itemOnMouse.Set(tempItem, tempCount);

        inventoryManager.RequestRedraw();
        OnAmmoSlotsChanged?.Invoke();
    }

    private void SetAimMode(bool shouldBeAiming)
    {
        isInAimMode = shouldBeAiming;
        if (playerController != null)
        {
            playerController.SetAimingState(isInAimMode);
        }

        // A visibilidade do braço (armPivot) agora deve ser controlada pelo Animator,
        // com base no estado 'isInAimMode' que o PlayerController define.
        // Removido: armPivot.SetActive(isInAimMode);

        var cursorManager = playerController?.cursorManager;
        if (cursorManager != null)
        {
            if (isInAimMode) cursorManager.SetAimCursor();
            else cursorManager.SetDefaultCursor();
        }

        if (shouldBeAiming)
        {
            AimLogic();
        }
    }

    // Funções de Acesso para a UI
    public InventorySlot GetActiveWeaponSlot() => weaponSlots[currentWeaponIndex];
    public InventorySlot GetWeaponSlot(int index) => weaponSlots[index];
    public InventorySlot GetAmmoSlot(int index) => ammoSlots[index];

    public bool TryGetActiveWeaponAmmo(out int currentAmmo, out int maxAmmo)
    {
        currentAmmo = 0;
        maxAmmo = 0;
        if (activeWeaponInstance is RangedWeapon rangedWeapon)
        {
            var data = GetActiveWeaponSlot()?.item;
            if (data != null)
            {
                currentAmmo = rangedWeapon.CurrentAmmo;
                maxAmmo = data.magazineSize;
                return true;
            }
        }
        return false;
    }
}