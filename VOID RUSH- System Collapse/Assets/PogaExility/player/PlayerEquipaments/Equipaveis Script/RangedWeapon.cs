// RangedWeapon.cs - VERS�O COMPLETA E CORRIGIDA
using System.Collections;
using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("CONFIGURA��O DO PREFAB")]
    [SerializeField] private Transform muzzlePoint;

    public int CurrentAmmo { get; private set; }
    private float lastAttackTime = -999f;
    private bool isReloading = false;
    private int ammoToLoad;
    private Coroutine reloadCoroutine;

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
            Debug.LogWarning($"A arma {weaponData.itemName} n�o tem um 'bulletPrefab' configurado!");
            return;
        }
        if (muzzlePoint == null)
        {
            Debug.LogError($"A arma {weaponData.itemName} n�o tem um 'muzzlePoint' configurado no prefab!");
            return;
        }

        GameObject projGO = Instantiate(weaponData.bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        Projectile proj = projGO.GetComponent<Projectile>();
        if (proj != null)
        {
            // A CHAMADA CORRIGIDA: Agora passa os 5 argumentos que o proj�til espera.
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
        Debug.Log("Click. Sem muni��o.");
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public void StartReload(int foundAmmo)
    {
        isReloading = true;
        this.ammoToLoad = foundAmmo;
    }

    public void CancelReload()
    {
        isReloading = false;
    }

    public void OnReloadAnimationComplete()
    {
        CurrentAmmo += this.ammoToLoad;
        isReloading = false;
        RaiseOnWeaponStateChanged();
    }
}