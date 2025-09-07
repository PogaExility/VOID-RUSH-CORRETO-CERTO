// WeaponHandler.cs - VERSÃO CORRIGIDA
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

    public int currentWeaponIndex { get; private set; } = 0;
    private WeaponBase activeWeaponInstance;

    [Header("REFERÊNCIAS")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private GameObject headPivot;
    [SerializeField] private GameObject armPivot;


    private bool isInAimMode = false;
    private Vector3 initialWeaponSocketScale;
    public bool allowMovementFlip = true;

    public event Action<int> OnActiveWeaponChanged;
    private SpriteRenderer activeWeaponRenderer;
    public event Action OnAmmoSlotsChanged;
    public event Action OnWeaponSlotsChanged;

    // A PARTIR DAQUI, O CÓDIGO É O CORRETO, SEM AS FUNÇÕES ABSTRATAS
    // QUE CAUSARAM O ERRO.

    void Awake()
    {
        Instance = this;
        initialWeaponSocketScale = weaponSocket.localScale;
        if (playerController == null) playerController = GetComponent<PlayerController>();

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
        var activeWeaponData = GetActiveWeaponSlot()?.item;
        if (activeWeaponData != null && activeWeaponData.useAimMode)
        {
            AimLogic();
        }
    }

    // EM WeaponHandler.cs

    private void AimLogic()
    {
        if (activeWeaponInstance == null || playerController == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // --- PASSO 1: DAR A ORDEM PARA VIRAR ---
        // O WeaponHandler agora só pede para o script de movimento fazer o flip.
        playerController.movementScript.FaceTowardsPoint(mouseWorldPos);

        // --- PASSO 2: PERGUNTAR A DIREÇÃO ATUAL ---
        // Depois que o personagem JÁ virou, perguntamos qual é a direção dele agora.
        bool isFacingRight = playerController.movementScript.IsFacingRight();
        float currentDirectionX = isFacingRight ? 1f : -1f;

        // --- PASSO 3: CÁLCULO DO ÂNGULO DA ARMA ---
        Vector2 playerForwardDirection = new Vector2(currentDirectionX, 0);
        Vector2 directionToMouse = (mouseWorldPos - weaponSocket.position);
        float armAngle = Vector2.SignedAngle(playerForwardDirection, directionToMouse);
        float clampedArmAngle = Mathf.Clamp(armAngle, -90f, 90f);

        // A rotação é aplicada LOCALMENTE, o SignedAngle já faz o trabalho de inversão.
        weaponSocket.localRotation = Quaternion.Euler(0, 0, clampedArmAngle);

        // --- PASSO 4: CÁLCULO E TRAVA DO ÂNGULO DA CABEÇA ---
        if (headPivot != null)
        {
            float headAngle = Mathf.Clamp(armAngle, -45, 45);
            headPivot.transform.localRotation = Quaternion.Euler(0, 0, headAngle);
        }
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
            if (ammoNeeded <= 0) return;

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

                if (ammoSlot.count <= 0)
                {
                    ammoSlot.Clear();
                }

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
        // 1. Limpeza
        if (activeWeaponInstance != null)
        {
            activeWeaponInstance.OnWeaponStateChanged -= HandleWeaponStateChange;
            Destroy(activeWeaponInstance.gameObject);
            activeWeaponInstance = null;
            activeWeaponRenderer = null;
        }

        // 2. Verificação
        if (weaponData == null || weaponData.equipPrefab == null)
        {
            SetAimMode(false);
            OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
            return;
        }

        // 3. Criação e Posição
        GameObject weaponGO = Instantiate(weaponData.equipPrefab, weaponSocket);
        weaponGO.transform.localPosition = Vector3.zero;
        weaponGO.transform.localRotation = Quaternion.identity;

        // 4. Renderização (CORRIGIDO)
        activeWeaponRenderer = weaponGO.GetComponentInChildren<SpriteRenderer>();
        // Pega o renderer do mesmo objeto que tem o PlayerController, que é o correto.
        var playerRenderer = playerController.GetComponent<SpriteRenderer>();

        if (activeWeaponRenderer != null && playerRenderer != null)
        {
            activeWeaponRenderer.sortingLayerName = playerRenderer.sortingLayerName;
            activeWeaponRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
        }

        // 5. Inicialização
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

        // 6. Finalização
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

        (itemNoMouse.item, equipmentSlot.item) = (equipmentSlot.item, itemNoMouse.item);
        (itemNoMouse.count, equipmentSlot.count) = (equipmentSlot.count, itemNoMouse.count);

        inventoryManager.RequestRedraw();

        if (currentWeaponIndex == weaponSlotIndex)
        {
            EquipToHand(weaponSlots[currentWeaponIndex].item);
        }

        OnWeaponSlotsChanged?.Invoke(); // DISPARA O NOVO EVENTO
    }

    public void EquipAmmoFromMouse(int ammoSlotIndex)
    {
        var itemNoMouse = inventoryManager.GetHeldItem();
        var equipmentSlot = ammoSlots[ammoSlotIndex];

        if (itemNoMouse.item != null && itemNoMouse.item.itemType != ItemType.Ammo) return;

        (itemNoMouse.item, equipmentSlot.item) = (equipmentSlot.item, itemNoMouse.item);
        (itemNoMouse.count, equipmentSlot.count) = (equipmentSlot.count, itemNoMouse.count);

        inventoryManager.RequestRedraw();
        OnAmmoSlotsChanged?.Invoke();
    }

    private void SetAimMode(bool shouldBeAiming)
    {
        isInAimMode = shouldBeAiming;

        if (playerController != null)
            playerController.SetAimingState(isInAimMode);

        // APAGUE OU COMENTE ESTAS DUAS LINHAS:
        // if (headPivot != null) headPivot.SetActive(isInAimMode);
        // if (armPivot != null) armPivot.SetActive(isInAimMode);

        var cursorManager = playerController?.cursorManager;
        if (cursorManager != null)
        {
            if (isInAimMode)
                cursorManager.SetAimCursor();
            else
                cursorManager.SetDefaultCursor();
        }

        if (shouldBeAiming)
        {
            AimLogic();
        }
    }
    public InventorySlot GetActiveWeaponSlot() => weaponSlots[currentWeaponIndex];
    public InventorySlot GetWeaponSlot(int index) => weaponSlots[index]; // NOVA FUNÇÃO
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