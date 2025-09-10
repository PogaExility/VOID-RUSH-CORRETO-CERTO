// RangedWeapon.cs - VERSÃO COMPLETA E CORRIGIDA
using System.Collections;
using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("CONFIGURAÇÃO DO PREFAB")]
    [SerializeField] private Transform muzzlePoint;

    public int CurrentAmmo { get; private set; }
    private float lastAttackTime = -999f;
    private bool isReloading = false;
    private int ammoToLoad;
    public override void Initialize(ItemSO data, int savedAmmo = -1)
    {
        base.Initialize(data, savedAmmo);
        CurrentAmmo = (savedAmmo == -1) ? weaponData.magazineSize : savedAmmo;
    }

    public override void Attack()
    {
        if (Time.time < lastAttackTime + weaponData.attackRate || isReloading) return;

        if (CurrentAmmo > 0) FireBullet();
        else FirePowder();

        lastAttackTime = Time.time;
    }

    private void FireBullet()
    {
        CurrentAmmo--;
        RaiseOnWeaponStateChanged();

        if (weaponData.bulletPrefab == null)
        {
            Debug.LogWarning($"A arma {weaponData.itemName} não tem um 'bulletPrefab' configurado!");
            return;
        }
        if (muzzlePoint == null)
        {
            Debug.LogError($"A arma {weaponData.itemName} não tem um 'muzzlePoint' configurado no prefab!");
            return;
        }

        GameObject projGO = Instantiate(weaponData.bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        Projectile proj = projGO.GetComponent<Projectile>();
        if (proj != null)
        {
            // A CHAMADA CORRIGIDA: Agora passa os 5 argumentos que o projétil espera.
            proj.Initialize(
                weaponData.bulletDamage,
                weaponData.bulletSpeed,
                weaponData.bulletLifetime,
                weaponData.pierceCount,
                weaponData.damageFalloff
            );
        }
    }

    private void FirePowder()
    {
        Debug.Log("Click. Sem munição.");
    }

    public int GetAmmoNeeded()
    {
        return weaponData.magazineSize - CurrentAmmo;
    }
    public bool CanReload()
    {
        // Não pode recarregar se já estiver recarregando ou se o pente estiver cheio.
        return !isReloading && CurrentAmmo < weaponData.magazineSize;
    }

    public void StartReload(int foundAmmo)
    {
        if (isReloading) return;

        isReloading = true;
        this.ammoToLoad = foundAmmo;
        // Apenas se prepara para a recarga. A animação vai chamar a próxima função.
    }

    public void CancelReload()
    {
        isReloading = false;
    }

    // ESTA É A FUNÇÃO QUE O ANIMATION EVENT VAI CHAMAR
    public void OnReloadAnimationComplete()
    {
        CurrentAmmo += this.ammoToLoad;
        isReloading = false;
        RaiseOnWeaponStateChanged();
        Debug.Log("Recarga Completa via Animation Event!");
    }
}