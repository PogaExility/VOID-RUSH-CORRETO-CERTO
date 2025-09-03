using UnityEngine;
using System.Collections; // Necess�rio para corotinas se for preciso
using System.Collections.Generic;

[RequireComponent(typeof(InventoryManager), typeof(PlayerAttack), typeof(DefenseHandler))]
public class CombatController : MonoBehaviour
{
    // Vari�veis que j� existem no seu c�digo
    private InventoryManager _inventoryManager;
    public PlayerAttack playerAttack;
    public DefenseHandler defenseHandler;

    [Header("Configura��o de Postura")]
    public WeaponType activeStance = WeaponType.Melee;

    // Vari�veis de mira para o pivotamento (passadas para o PlayerAttack)
    public Vector3 aimDirection { get; private set; }

    void Awake()
    {
        _inventoryManager = GetComponent<InventoryManager>();
        playerAttack = GetComponent<PlayerAttack>();
        defenseHandler = GetComponent<DefenseHandler>();
    }

    void Update()
    {
        HandleStanceSwitch();
        // A mira deve ser atualizada em todo frame para precis�o
        UpdateAimDirection();
    }

    private void UpdateAimDirection()
    {
        // Pega a posi��o do mouse na tela e converte para coordenadas do mundo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // Garante que a profundidade seja 0

        // A dire��o de mira � do jogador para o mouse
        aimDirection = (mousePos - transform.position).normalized;
    }

    //public void ProcessCombatInput()
  //  {
     //   ItemSO currentWeapon = GetEquippedWeapon();

       // if (Input.GetButtonDown("Fire1"))
       // {
           // if (activeStance == WeaponType.Buster && currentWeapon != null)
          //  {
          //      playerAttack.StartBusterCharge(currentWeapon);
          //  }
           // else if (currentWeapon != null)
          //  {
          //      playerAttack.PerformAttack(currentWeapon, aimDirection);
         //   }
     //   }

     //   if (Input.GetButtonUp("Fire1"))
     //   {
      //      if (activeStance == WeaponType.Buster && currentWeapon != null)
      //      {
      //          playerAttack.ReleaseBusterCharge(currentWeapon, aimDirection);
      //      }
    //    }
  //  }
    // Fun��o auxiliar para pegar a arma no slot correto
//private ItemSO GetEquippedWeapon()
   // {
      //  int slotIndex = (int)activeStance;
       // if (slotIndex >= _inventoryManager.equippedWeapons.Length) return null;
       // return _inventoryManager.equippedWeapons[slotIndex];
   // }

    private void HandleStanceSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) activeStance = WeaponType.Melee;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) activeStance = WeaponType.Ranger;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) activeStance = WeaponType.Buster;
    }
}