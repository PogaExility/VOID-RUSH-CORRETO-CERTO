using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public static WeaponHandler Instance;

    [Header("CONFIGURAÇÃO DE EQUIPAMENTO")]
    public const int NUM_WEAPON_SLOTS = 3;
    [SerializeField] private InventorySlot[] weaponSlots = new InventorySlot[3];

    public int currentWeaponIndex { get; private set; } = 0;
    private WeaponBase activeWeaponInstance;

    [Header("REFERÊNCIAS")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Transform playerVisualsTransform;
    [SerializeField] private GameObject headPivot;
    [SerializeField] private GameObject armPivot;

    [Header("ESTADO DE COMBATE")]
    private bool isInAimMode = false;

    // >> O EVENTO ESTÁ AQUI <<
    public event Action<int> OnActiveWeaponChanged;

    private ItemSO activeWeapon;
    private GameObject activeWeaponGO;
    private int currentAmmo;
    private bool isReloading = false;
    private float lastAttackTime = -999f;


    void Awake()
    {
        // Se a referência não foi arrastada, tenta pegar
        if (playerController == null) playerController = GetComponent<PlayerController>();

        // Inicializa os slots para evitar erros de nulo
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null) weaponSlots[i] = new InventorySlot();
        }
    }
    void Update()
    {
        // Pega a arma ativa DOS DADOS.
        var activeWeaponData = GetActiveWeaponSlot()?.item;

        // Se tem uma arma e ela usa mira, o AimLogic é chamado.
        if (activeWeaponData != null && activeWeaponData.useAimMode)
        {
            AimLogic();
        }
    }
    void Start()
    {
        EquipToHand(weaponSlots[currentWeaponIndex].item);
    }
    private void AimLogic()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (playerVisualsTransform != null)
        {
            playerVisualsTransform.localScale = new Vector3(Mathf.Sign(mouseWorldPos.x - transform.position.x), 1, 1);
        }
    }

    public void EquipWeapon(ItemSO weapon)
    {
        // 1. Destrói a instância da arma antiga
        if (activeWeaponInstance != null) Destroy(activeWeaponInstance.gameObject);

        // 2. Se não tem arma nova ou prefab, para aqui
        if (weapon == null || weapon.equipPrefab == null) { SetAimMode(false); return; }

        // 3. Cria o NOVO prefab da arma no socket
        GameObject weaponGO = Instantiate(weapon.equipPrefab, weaponSocket);
        weaponGO.transform.localPosition = Vector3.zero;
        weaponGO.transform.localRotation = Quaternion.identity;
        weaponGO.transform.localScale = Vector3.one;

        // 4. Pega o SCRIPT da arma e dá a identidade a ela
        activeWeaponInstance = weaponGO.GetComponent<WeaponBase>();
        if (activeWeaponInstance != null)
        {
            activeWeaponInstance.Initialize(weapon);
        }

        // 5. Atualiza o modo mira
        SetAimMode(weapon.useAimMode);
    }

    public void CycleWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % weaponSlots.Length;
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

        inventoryManager.RequestRedraw();

        if (currentWeaponIndex == weaponSlotIndex)
        {
            EquipToHand(weaponSlots[currentWeaponIndex].item);
            OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
        }
    }
    // Em WeaponHandler.cs
    private void FireBullet()
    {
        currentAmmo--;
        Debug.Log("TIRO! Munição: " + currentAmmo);
        // TODO: Instanciar o prefab da bala: Instantiate(activeWeapon.bulletPrefab, ...);

        // Avisa a UI que a munição mudou
        OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
    }

    private void FirePowder()
    {
        Debug.Log("TIRO DE PÓLVORA! (Curto alcance)");
        // TODO: Lógica de dano de perto com OverlapCircle
    }
    // Em WeaponHandler.cs
    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log("Recarregando...");

        // TODO: Chamar animação de recarga aqui

        yield return new WaitForSeconds(activeWeapon.reloadTime);

        // Lógica para pegar munição do inventário viria aqui.
        // Por agora, apenas enche o pente.
        currentAmmo = activeWeapon.magazineSize;

        isReloading = false;
        Debug.Log("Recarga Completa! Munição: " + currentAmmo);

        // Avisa a UI que a munição mudou
        OnActiveWeaponChanged?.Invoke(currentWeaponIndex);
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
        if (activeWeaponInstance != null && activeWeaponInstance is RangedWeapon rangedWeapon)
        {
            // rangedWeapon.Reload();
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

    // A função auxiliar que ele precisa
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
    public void EquipToHand(ItemSO weaponData)
    {
        // Destrói a arma antiga
        if (activeWeaponInstance != null)
        {
            Destroy(activeWeaponInstance.gameObject);
        }

        // Se não tiver arma para equipar, para aqui.
        if (weaponData == null || weaponData.equipPrefab == null)
        {
            activeWeaponInstance = null;
            return;
        }

        // Cria a nova arma, posiciona e inicializa
        GameObject weaponGO = Instantiate(weaponData.equipPrefab, weaponSocket);
        weaponGO.transform.localPosition = Vector3.zero;
        weaponGO.transform.localRotation = Quaternion.identity;
        weaponGO.transform.localScale = Vector3.one;

        activeWeaponInstance = weaponGO.GetComponent<WeaponBase>();
        if (activeWeaponInstance != null)
        {
            activeWeaponInstance.Initialize(weaponData);
        }
    }

    // >> A FUNÇÃO ESTÁ AQUI <<
    public InventorySlot GetActiveWeaponSlot() => weaponSlots[currentWeaponIndex];
}
