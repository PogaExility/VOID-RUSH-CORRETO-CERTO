// RangedWeapon.cs - VERSÃO COMPLETA E CORRIGIDA
using System.Collections;
using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("CONFIGURAÇÃO DO PREFAB")]
    [SerializeField] private Transform muzzlePoint;

    public int CurrentAmmo { get; private set; }
    private float lastAttackTime = -999f;
    public bool isReloading { get; private set; } = false;
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
    public int GetAmmoNeeded()
    {
        // Retorna a quantidade de munição necessária para encher o pente.
        // Garante que não retorne um número negativo se por algum bug a CurrentAmmo for maior que o magazineSize.
        return Mathf.Max(0, weaponData.magazineSize - CurrentAmmo);
    }
    private void FirePowder()
    {
        Debug.Log("Click. Sem munição.");
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public void StartReload(int ammoToLoad, System.Action onReloadLogicFinished)
    {
        if (isReloading || !gameObject.activeInHierarchy) return;
        // Guardamos a corrotina na variável para podermos cancelá-la depois.
        reloadCoroutine = StartCoroutine(ReloadTimerCoroutine(ammoToLoad, onReloadLogicFinished));
    }

    public void CancelReload()
    {
        // Se a corrotina estiver rodando, pare-a.
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        isReloading = false;
    }

    public void OnReloadAnimationComplete()
    {
        CurrentAmmo += this.ammoToLoad;
        isReloading = false;
        RaiseOnWeaponStateChanged();
    }
    private IEnumerator ReloadTimerCoroutine(int ammoToLoad, System.Action onReloadLogicFinished)
    {
        isReloading = true;

        // 1. A LÓGICA ESPERA o tempo definido no seu ItemSO da arma.
        yield return new WaitForSeconds(weaponData.reloadTime);

        // 2. A LÓGICA ADICIONA a munição.
        CurrentAmmo += ammoToLoad;
        isReloading = false;
        RaiseOnWeaponStateChanged(); // Avisa a UI que a munição mudou.

        // 3. A LÓGICA AVISA o WeaponHandler que o trabalho dela terminou.
        onReloadLogicFinished?.Invoke();
    }
}