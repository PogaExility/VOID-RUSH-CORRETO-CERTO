using UnityEngine;

[RequireComponent(typeof(InventoryManager), typeof(PlayerAttack), typeof(DefenseHandler))]
public class CombatController : MonoBehaviour
{
    [Header("Referências")]
    private InventoryManager _inventoryManager;
    public PlayerAttack playerAttack;
    public DefenseHandler defenseHandler;

    [Header("Configuração de Postura")]
    public WeaponType activeStance = WeaponType.Melee;

    private WeaponSO _currentWeapon;

    void Awake()
    {
        // ====================================================================
        // A SOLUÇÃO: Pega todas as referências automaticamente no Awake().
        // ====================================================================
        _inventoryManager = GetComponent<InventoryManager>();
        playerAttack = GetComponent<PlayerAttack>();
        defenseHandler = GetComponent<DefenseHandler>();
    }

    void Update()
    {
        HandleStanceSwitch();
    }

    public void ProcessCombatInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            switch (activeStance)
            {
                case WeaponType.Melee: _currentWeapon = _inventoryManager.equippedMeleeWeapon; break;
                case WeaponType.Firearm: _currentWeapon = _inventoryManager.equippedFirearm; break;
                case WeaponType.Buster: _currentWeapon = _inventoryManager.equippedBuster; break;
            }

            if (_currentWeapon != null)
            {
                playerAttack.PerformAttack(_currentWeapon);
            }
            else { Debug.Log("Nenhuma arma equipada para a postura " + activeStance); }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            defenseHandler.StartBlock();
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            defenseHandler.EndBlock();
        }

        if (activeStance == WeaponType.Firearm && Input.GetKeyDown(KeyCode.R))
        {
            playerAttack.Reload(_inventoryManager.equippedFirearm);
        }
    }

    private void HandleStanceSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { activeStance = WeaponType.Melee; Debug.Log("Postura: Melee"); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { activeStance = WeaponType.Firearm; Debug.Log("Postura: Firearm"); }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { activeStance = WeaponType.Buster; Debug.Log("Postura: Buster"); }
    }
}