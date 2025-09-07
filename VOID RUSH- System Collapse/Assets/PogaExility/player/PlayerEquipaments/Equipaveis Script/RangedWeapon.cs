// RangedWeapon.cs - VERSÃO SIMPLIFICADA
using System.Collections;
using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("CONFIGURAÇÃO DO PREFAB")]
    [SerializeField] private Transform muzzlePoint;

    public int CurrentAmmo { get; private set; }
    private float lastAttackTime = -999f;
    private bool isReloading = false;

    protected override void InternalInitialize()
    {
        CurrentAmmo = weaponData.magazineSize;
    }

    public override void Attack()
    {
        if (Time.time < lastAttackTime + weaponData.attackRate) return;
        if (isReloading) return;

        if (CurrentAmmo > 0) FireBullet();
        else FirePowder();

        lastAttackTime = Time.time;
    }

    private void FireBullet()
    {
        CurrentAmmo--;
        if (weaponData.bulletPrefab != null && muzzlePoint != null)
        {
            Instantiate(weaponData.bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        }
        RaiseOnWeaponStateChanged();
    }

    private void FirePowder()
    {
        Debug.Log("Fagulha. Sem munição.");
    }

    // NOVO: Pergunta que o Handler faz
    public int GetAmmoNeeded()
    {
        return weaponData.magazineSize - CurrentAmmo;
    }

    // NOVO: Ordem que o Handler dá
    public void StartReload(int ammoToLoad)
    {
        if (isReloading) return;
        StartCoroutine(ReloadTimerCoroutine(ammoToLoad));
    }

    private IEnumerator ReloadTimerCoroutine(int ammoToLoad)
    {
        isReloading = true;
        Debug.Log("Iniciando timer de recarga...");

        yield return new WaitForSeconds(weaponData.reloadTime);

        CurrentAmmo += ammoToLoad;
        isReloading = false;

        Debug.Log($"Recarga finalizada! Pente com {CurrentAmmo} balas.");
        RaiseOnWeaponStateChanged();
    }
}