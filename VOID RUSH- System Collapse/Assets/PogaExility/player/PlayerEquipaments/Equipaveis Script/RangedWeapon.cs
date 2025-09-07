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

    // EM RangedWeapon.cs

    private void FireBullet()
    {
        CurrentAmmo--;
        RaiseOnWeaponStateChanged(); // Mova o Raise para o início para a UI atualizar instantaneamente.

        if (weaponData.bulletPrefab != null && muzzlePoint != null)
        {
            // 1. Cria o projétil.
            GameObject projectileGO = Instantiate(weaponData.bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);

            // 2. Pega o script do projétil recém-criado.
            Projectile projectileScript = projectileGO.GetComponent<Projectile>();

            // 3. Se o script existir, entrega a ele o dano que está no ItemSO.
            if (projectileScript != null)
            {
                // O dano vem da "ficha técnica" da arma (ItemSO).
                projectileScript.Initialize(weaponData.bulletDamage);
            }
        }
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

    public void StartReload(int ammoToLoad)
    {
        if (isReloading) return;
        StartCoroutine(ReloadTimerCoroutine(ammoToLoad));
    }

    private IEnumerator ReloadTimerCoroutine(int ammoToLoad)
    {
        isReloading = true;
        Debug.Log("Recarregando...");
        yield return new WaitForSeconds(weaponData.reloadTime);

        CurrentAmmo += ammoToLoad;
        isReloading = false;

        Debug.Log($"Recarga Completa! Pente com {CurrentAmmo} balas.");
        RaiseOnWeaponStateChanged(); // Avisa a HUD para atualizar
    }
}