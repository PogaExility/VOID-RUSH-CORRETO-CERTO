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
    [SerializeField] private PlayerAnimatorController animatorController;

    private WeaponBase activeWeaponInstance;
    public int currentWeaponIndex { get; private set; } = 0;
    private bool isInAimMode = false;

    public event Action<int> OnActiveWeaponChanged;
    public event Action OnAmmoSlotsChanged;
    public event Action OnWeaponSlotsChanged;
    private Vector3 initialWeaponSocketScale;
    public bool IsReloading;
    private Transform attackPoint;
    void Awake()
    {
        Instance = this;
        initialWeaponSocketScale = weaponSocket.localScale;

        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (animatorController == null) animatorController = GetComponentInParent<PlayerAnimatorController>();

        // Inicializa os slots de arma e munição para evitar erros
        for (int i = 0; i < weaponSlots.Length; i++) { if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot(); }
        for (int i = 0; i < ammoSlots.Length; i++) { if (ammoSlots[i] == null) ammoSlots[i] = new InventorySlot(); }

        // Encontra e armazena a referência para o AttackPoint
        attackPoint = playerController.transform.Find("AttackPoint");
        if (attackPoint == null)
        {
            Debug.LogError("CRÍTICO: O objeto filho 'AttackPoint' não foi encontrado no Player. Crie um objeto vazio com este nome para o combate Meelee funcionar.", playerController.gameObject);
        }
    }

    public Transform GetAttackPoint()
    {
        return attackPoint;
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
            // A versão correta que não passa parâmetros.
            activeWeaponInstance.Attack();
        }
    }

    public void ForceExitAimMode()
    {
        if (isInAimMode)
        {
            SetAimMode(false);
        }
    }

    public bool IsAimWeaponEquipped()
    {
        var activeWeaponData = GetActiveWeaponSlot()?.item;
        if (activeWeaponData == null) return false;
        return activeWeaponData.useAimMode;
    }

    public void HandleReloadInput()
    {
        if (activeWeaponInstance is RangedWeapon rangedWeapon)
        {
            if (rangedWeapon.isReloading || rangedWeapon.GetAmmoNeeded() <= 0) return;

            int ammoFound = FindAndConsumeAmmo(rangedWeapon.GetAmmoNeeded());
            if (ammoFound > 0)
            {
                IsReloading = true;

                float desiredDuration = GetActiveWeaponSlot().item.reloadTime;
                float baseDuration = animatorController.reloadAnimationBaseDuration; // Adicionado para clareza
                float speedMultiplier = (desiredDuration > 0) ? baseDuration / desiredDuration : 1f;

                // ADICIONE ESTE DEBUG.LOG
                Debug.Log($"CÁLCULO DE VELOCIDADE: Duração Base ({baseDuration}) / Duração Desejada ({desiredDuration}) = Multiplicador Final ({speedMultiplier})");

                animatorController.SetAnimatorFloat(AnimatorTarget.PlayerHand, "ReloadSpeedMultiplier", speedMultiplier);
                animatorController.PlayState(AnimatorTarget.PlayerHand, PlayerAnimState.recarregando);
                rangedWeapon.StartReload(ammoFound, OnReloadLogicComplete);
            }
        }
    }

    // ADICIONE esta nova função. É ela que a RangedWeapon vai chamar quando o TIMER dela acabar.
    public void OnReloadLogicComplete()
    {
        // A lógica terminou, então podemos liberar o estado.
        IsReloading = false;
        Debug.Log("Lógica de recarga finalizada. Jogador pode mirar/atirar novamente.");

        // ADIÇÃO CRÍTICA: Avisamos o Maestro para mandar a mão voltar para a animação de "parado".
        // Isso vai "limpar" o cérebro dele e prepará-lo para a próxima recarga.
        animatorController.PlayState(AnimatorTarget.PlayerHand, PlayerAnimState.parado);
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

    public void UpdateAimVisuals(bool isAiming)
    {
        isInAimMode = isAiming;
        if (armPivot != null) armPivot.SetActive(isAiming);
        var cursorManager = playerController?.cursorManager;
        if (cursorManager != null)
        {
            if (isAiming) cursorManager.SetAimCursor();
            else cursorManager.SetDefaultCursor();
        }
        if (isAiming) AimLogic();
    }
    public void EquipToHand(int slotIndex)
    {
        // --- LIMPEZA DA ARMA ANTIGA (UNIFICADO) ---
        IsReloading = false;

        if (activeWeaponInstance != null)
        {
            // Se a arma antiga for um componente neste objeto (lógica Meelee), destrói o componente.
            if (activeWeaponInstance is MeeleeWeapon oldMeeleeWeapon)
            {
                oldMeeleeWeapon.CancelAttack();
                Destroy(oldMeeleeWeapon);
            }
            // Se for um objeto físico (lógica Ranger), destrói o GameObject.
            else
            {
                if (activeWeaponInstance is RangedWeapon oldRangedWeapon)
                {
                    oldRangedWeapon.CancelReload();
                    weaponSlots[currentWeaponIndex].currentAmmo = oldRangedWeapon.CurrentAmmo;
                }
                activeWeaponInstance.OnWeaponStateChanged -= HandleWeaponStateChange;
                Destroy(activeWeaponInstance.gameObject);
            }
        }

        activeWeaponInstance = null;
        currentWeaponIndex = slotIndex;
        var newWeaponSlot = weaponSlots[currentWeaponIndex];
        var weaponData = newWeaponSlot.item;

        if (weaponData == null)
        {
            playerController.SetAimingState(false);
            OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
            return;
        }

        // --- LÓGICA HÍBRIDA DE EQUIPAMENTO ---
        if (weaponData.weaponType == WeaponType.Meelee)
        {
            // TIPO MEELEE: Adiciona o script como um componente lógico.
            MeeleeWeapon meeleeInstance = gameObject.AddComponent<MeeleeWeapon>();
            meeleeInstance.Initialize(weaponData);
            meeleeInstance.InitializeMeelee(playerController, animatorController);
            activeWeaponInstance = meeleeInstance;
        }
        else // Para Ranger, Buster, etc.
        {
            // TIPO RANGER (E OUTROS): Instancia o prefab físico, como antes.
            if (weaponData.equipPrefab == null)
            {
                playerController.SetAimingState(false);
                OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
                return;
            }

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
        }

        playerController.SetAimingState(weaponData.useAimMode);
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

    public void SetAimMode(bool shouldBeAiming)
    {
        isInAimMode = shouldBeAiming;

        // Esta função já chama o PlayerController, mas agora ela é a "parte visual".
        // Isso é bom, pois garante que o estado de movimento seja sempre atualizado.
        if (playerController != null)
        {
            playerController.SetAimingStateVisuals(shouldBeAiming);

        }

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