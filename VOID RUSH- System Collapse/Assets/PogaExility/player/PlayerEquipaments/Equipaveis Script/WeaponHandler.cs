// WeaponHandler.cs - VERSÃO COMPLETA E CORRIGIDA
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
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private GameObject armPivot; // A referência para o objeto do braço
    [SerializeField] private Animator handAnimator;
    private static readonly int HandIdleHash = Animator.StringToHash("Mao_Idle");
    private static readonly int HandReloadingHash = Animator.StringToHash("Recarregando");

    private WeaponBase activeWeaponInstance;
    public int currentWeaponIndex { get; private set; } = 0;
    private bool isInAimMode = false;

    public event Action<int> OnActiveWeaponChanged;
    public event Action OnAmmoSlotsChanged;
    public event Action OnWeaponSlotsChanged;
    private Vector3 initialWeaponSocketScale;
    public bool IsReloading;
    void Awake()
    {
        Instance = this;
        initialWeaponSocketScale = weaponSocket.localScale;
        if (playerController == null) playerController = GetComponent<PlayerController>();
        for (int i = 0; i < weaponSlots.Length; i++) { if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot(); }
        for (int i = 0; i < ammoSlots.Length; i++) { if (ammoSlots[i] == null) ammoSlots[i] = new InventorySlot(); }
    }

    void Start()
    {
        // Ao iniciar, equipa a arma no slot 0.
        EquipToHand(0);
    }

    void Update()
    {
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

        playerController.movementScript.FaceTowardsPoint(mouseWorldPos);

        Vector2 direction = (mouseWorldPos - weaponSocket.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (!playerController.movementScript.IsFacingRight())
        {
            weaponSocket.localScale = new Vector3(initialWeaponSocketScale.x, -initialWeaponSocketScale.y, initialWeaponSocketScale.z);
        }
        else
        {
            weaponSocket.localScale = initialWeaponSocketScale;
        }

        weaponSocket.rotation = Quaternion.Euler(0, 0, angle);
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
        // Se não tiver uma arma Ranger, ou se já estiver recarregando, não faz nada.
        if (!(activeWeaponInstance is RangedWeapon rangedWeapon) || rangedWeapon.IsReloading()) return;

        // Verifica se a arma pode recarregar (pente não está cheio).
        if (!rangedWeapon.CanReload()) return;

        int ammoFound = FindAndConsumeAmmo(rangedWeapon.GetAmmoNeeded());
        if (ammoFound > 0)
        {
            // --- A CORREÇÃO CRÍTICA ESTÁ AQUI ---
            // Força a ativação do braço ANTES de tocar a animação.
            if (armPivot != null)
            {
                armPivot.SetActive(true);
            }
            // --- FIM DA CORREÇÃO ---

            // Dispara o gatilho da animação no Animator da mão.
            handAnimator.Play(HandReloadingHash);

            // Dá a ordem para a arma iniciar a lógica de recarga.
            rangedWeapon.StartReload(ammoFound);
        }
    }
    public void OnReloadComplete()
    {
        if (activeWeaponInstance is RangedWeapon rangedWeapon)
        {
            rangedWeapon.OnReloadAnimationComplete();
            // Manda a mão voltar para a animação de parada.
            handAnimator.Play(HandIdleHash);
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
            if (ammoSlot.item != null && weaponData.acceptedAmmo.Contains(ammoSlot.item))
            {
                int amountToConsume = Mathf.Min(maxAmountNeeded - totalConsumed, ammoSlot.count);
                ammoSlot.count -= amountToConsume;
                totalConsumed += amountToConsume;
                if (ammoSlot.count <= 0) ammoSlot.Clear();
                if (totalConsumed >= maxAmountNeeded) break;
            }
        }

        if (totalConsumed > 0) OnAmmoSlotsChanged?.Invoke();
        return totalConsumed;
    }

    // A função EquipToHand agora usa o ÍNDICE do slot como parâmetro.
    public void EquipToHand(int slotIndex)
    {
        IsReloading = false;
        if (activeWeaponInstance is RangedWeapon oldRangedWeapon)
        {
            weaponSlots[currentWeaponIndex].currentAmmo = oldRangedWeapon.CurrentAmmo;
        }

        // 2. LIMPEZA
        if (activeWeaponInstance != null)
        {
            activeWeaponInstance.OnWeaponStateChanged -= HandleWeaponStateChange;
            Destroy(activeWeaponInstance.gameObject);
        }

        // 3. ATUALIZA O ÍNDICE E PEGA OS DADOS
        currentWeaponIndex = slotIndex;
        var newWeaponSlot = weaponSlots[currentWeaponIndex];
        var weaponData = newWeaponSlot.item;
        activeWeaponInstance = null;

        if (weaponData == null || weaponData.equipPrefab == null)
        {
            SetAimMode(false);
            OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
            return;
        }

        // 4. CRIA A NOVA ARMA
        GameObject weaponGO = Instantiate(weaponData.equipPrefab, weaponSocket);
        weaponGO.transform.localPosition = Vector3.zero;
        weaponGO.transform.localRotation = Quaternion.identity;

        var playerRenderer = playerController.GetComponent<SpriteRenderer>();
        var weaponRenderer = weaponGO.GetComponentInChildren<SpriteRenderer>();
        if (weaponRenderer != null && playerRenderer != null)
        {
            weaponRenderer.sortingLayerName = playerRenderer.sortingLayerName;
            weaponRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
        }

        // 5. INICIALIZA A NOVA ARMA COM A MUNIÇÃO SALVA
        activeWeaponInstance = weaponGO.GetComponent<WeaponBase>();
        if (activeWeaponInstance != null)
        {
            activeWeaponInstance.Initialize(weaponData, newWeaponSlot.currentAmmo);
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
        int nextIndex = (currentWeaponIndex + 1) % weaponSlots.Length;
        EquipToHand(nextIndex);
    }

    public void EquipItemFromMouse(int weaponSlotIndex)
    {
        var itemNoMouse = inventoryManager.GetHeldItem();
        var equipmentSlot = weaponSlots[weaponSlotIndex];
        if (itemNoMouse.item != null && itemNoMouse.item.itemType != ItemType.Weapon) return;

        // Troca os itens
        (itemNoMouse.item, equipmentSlot.item) = (equipmentSlot.item, itemNoMouse.item);
        (itemNoMouse.count, equipmentSlot.count) = (equipmentSlot.count, itemNoMouse.count);
        (itemNoMouse.currentAmmo, equipmentSlot.currentAmmo) = (equipmentSlot.currentAmmo, itemNoMouse.currentAmmo); // Troca a munição também

        inventoryManager.RequestRedraw();
        OnWeaponSlotsChanged?.Invoke();

        // Reequipa a arma na mão se o slot ativo foi modificado.
        if (currentWeaponIndex == weaponSlotIndex)
        {
            EquipToHand(weaponSlotIndex);
        }
    }

    // EquipAmmoFromMouse permanece o mesmo
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

        // CORREÇÃO: O braço (armPivot) é ativado/desativado diretamente.
        // Isso resolve as mãos que não aparecem e o erro de corrotina.
        if (armPivot != null)
        {
            armPivot.SetActive(shouldBeAiming);
        }

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

    // Funções de Acesso para a UI (sem mudanças)
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