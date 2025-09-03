// ----- WeaponHandler.cs (SUBSTITUIR OU COMPLETAR) -----
using System.Linq;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("Refs")]
    public InventoryManager inventory;         // arraste o InventoryManager do Player
    public Transform weaponSocket;             // arraste o Transform da mão (ex.: RightHandSocket)

    [Header("Runtime")]
    public ItemSO equippedWeapon;              // arma equipada atualmente
    private GameObject equippedGO;             // instância do prefab equipPrefab
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
    // === Equip / Unequip ===
    public void Equip(ItemSO weaponSO)
    {
        if (weaponSO == null || weaponSO.itemType != ItemType.Weapon) return;
        Unequip();

        equippedWeapon = weaponSO;

        if (weaponSO.equipPrefab != null && weaponSocket != null)
        {
            equippedGO = Instantiate(weaponSO.equipPrefab, weaponSocket);
            equippedGO.transform.localPosition = Vector3.zero;
            equippedGO.transform.localRotation = Quaternion.identity;
            equippedGO.transform.localScale = Vector3.one;
        }

        // Aqui você pode sincronizar HUD (ícone, ammo/cooldown) via evento seu.
        // Ex.: HUD.WeaponWidget.SetWeapon(equippedWeapon);
    }

    public void Unequip()
    {
        if (equippedGO != null) Destroy(equippedGO);
        equippedGO = null;
        equippedWeapon = null;
    }

    // === Ammo helpers ===
  //  public int GetTotalAmmo()
  //  {
  //      if (equippedWeapon == null || equippedWeapon.acceptedAmmo == null || equippedWeapon.acceptedAmmo.Length == 0)
    //        return 0;
    //       int total = 0;
   //     foreach (var ammo in equippedWeapon.acceptedAmmo)
    //    return total;
  //  }

    public bool TryConsumeOneAmmo()
    {
        if (equippedWeapon == null || equippedWeapon.acceptedAmmo == null) return false;

        // prioridade: consome na ordem do array acceptedAmmo
        foreach (var ammo in equippedWeapon.acceptedAmmo)
        {
            //if (inventory.TryConsumeItem(ammo, 1))
                return true;
        }
        return false;
    }

    // === Disparo genérico ===
    // Chame este método a partir do seu CombatController quando o player apertar o botão de tiro.
    public void Fire(Vector3 origin, Vector3 direction)
    {
        if (equippedWeapon == null) return;

        // 1) Tenta consumir munição
        if (TryConsumeOneAmmo())
        {
            // Disparo com munição (dano de bala)
            float dmg = equippedWeapon.bulletDamage;
            // TODO: aqui você instancia projétil / faz raycast e aplica 'dmg'
            // Exemplo simples (raycast hitscan):
            if (Physics.Raycast(origin, direction, out var hit, 200f))
            {
                var hp = hit.collider.GetComponent<IDamageable>();
                if (hp != null) hp.TakeDamage(dmg);
            }
            // Atualize HUD de munição: HUD.WeaponWidget.SetAmmo(GetTotalAmmo(), ???max???)
            return;
        }

        // 2) Sem munição → dano de pólvora (curto alcance)
        float range = equippedWeapon.powderRange;
        float pdmg = equippedWeapon.powderDamage;

        if (Physics.Raycast(origin, direction, out var phit, range))
        {
            var hp = phit.collider.GetComponent<IDamageable>();
            if (hp != null) hp.TakeDamage(pdmg);
        }
        // Pode tocar um SFX "clique seco" + VFX de fagulha curta.
    }
}
