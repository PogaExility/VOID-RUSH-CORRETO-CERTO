using UnityEngine;

[RequireComponent(typeof(InventoryManager), typeof(PlayerAttack), typeof(DefenseHandler))]
public class CombatController : MonoBehaviour
{
    [Header("Refer�ncias")]
    private InventoryManager _inventoryManager;
    public PlayerAttack playerAttack;
    public DefenseHandler defenseHandler;

    [Header("Configura��o de Postura")]
    public WeaponType activeStance = WeaponType.Melee;

    private WeaponSO _currentWeapon;

    void Awake()
    {
        _inventoryManager = GetComponent<InventoryManager>();
        playerAttack = GetComponent<PlayerAttack>();
        defenseHandler = GetComponent<DefenseHandler>();
    }

    // A fun��o Update agora s� se preocupa em trocar a postura.
    // O processamento do input foi movido para a fun��o p�blica abaixo.
    void Update()
    {
        HandleStanceSwitch();
    }

    // ====================================================================
    // FUN��O CORRIGIDA para ser p�blica e com o nome certo
    // ====================================================================
    public void ProcessCombatInput()
    {
        // S� permite a��es de combate se o jogador n�o estiver fazendo outra coisa (como dar um dash).
        // Isso ser� aprimorado no PlayerController.

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