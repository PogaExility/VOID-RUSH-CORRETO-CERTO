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
    private Coroutine recoilCoroutine;

    public override void Initialize(ItemSO data, int savedAmmo = -1)
    {
        base.Initialize(data, savedAmmo);
        CurrentAmmo = (savedAmmo == -1) ? weaponData.magazineSize : savedAmmo;
    }
    // Função `Attack` alterada para evitar a duplicação de prefabs de pólvora.
    // Função Attack corrigida para instanciar solto no mundo
    public override void Attack()
    {
        if (Time.time < lastAttackTime + weaponData.attackRate || isReloading) return;

        lastAttackTime = Time.time;

        var audioSource = WeaponHandler.Instance.PlayerAudioSource;
        if (weaponData.shotSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(weaponData.shotSound);
        }

        if (recoilCoroutine != null) StopCoroutine(recoilCoroutine);
        recoilCoroutine = StartCoroutine(RecoilCoroutine());

        if (CurrentAmmo > 0)
        {
            if (weaponData.gunpowderPrefab != null)
            {
                // MUDANÇA: Instanciamos usando a rotação do muzzle, mas SEM PARENT (null no final não é necessário pois essa sobrecarga já é world space)
                // A posição inicial não importa tanto pois o Initialize vai corrigir no mesmo frame via UpdatePositionAndRotation
                GameObject gunpowderGO = Instantiate(weaponData.gunpowderPrefab, muzzlePoint.position, muzzlePoint.rotation);

                GunpowderExplosion script = gunpowderGO.GetComponent<GunpowderExplosion>();
                if (script != null)
                {
                    // Enviamos os dados e o Initialize já vai posicionar corretamente usando o TransformPoint
                    script.Initialize(0, 0, 0, RangedKnockbackDirection.Frente, Vector2.zero, muzzlePoint, weaponData.gunpowderSpawnOffset);
                }
            }
            FireBullet();
        }
        else
        {
            DesperationAttack();
        }
    }

    // Função DesperationAttack corrigida
    private void DesperationAttack()
    {
        Debug.Log("Ataque de curto alcance sem munição!");

        if (weaponData.gunpowderPrefab != null)
        {
            // Instancia no lugar do muzzle
            GameObject explosionGO = Instantiate(weaponData.gunpowderPrefab, muzzlePoint.position, muzzlePoint.rotation);

            GunpowderExplosion explosionScript = explosionGO.GetComponent<GunpowderExplosion>();
            if (explosionScript != null)
            {
                explosionScript.Initialize(
                    weaponData.powderDamage,
                    weaponData.powderRange,
                    weaponData.powderKnockback,
                    weaponData.powderKnockbackDirection,
                    muzzlePoint.right,
                    muzzlePoint,
                    weaponData.gunpowderSpawnOffset
                );
            }
        }
    }
    private IEnumerator RecoilCoroutine()
    {
        Vector3 originalPosition = Vector3.zero; // A posição inicial da arma é sempre (0,0,0) em relação ao socket
        Vector3 recoilPosition = new Vector3(-weaponData.recoilDistance, 0, 0);

        float journey = 0f;

        // Movimento para trás (o "soco")
        while (journey < weaponData.recoilDistance)
        {
            journey += Time.deltaTime * weaponData.recoilSpeed;
            transform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, journey / weaponData.recoilDistance);
            yield return null;
        }

        journey = 0f;

        // Movimento de volta para o lugar
        while (journey < weaponData.recoilDistance)
        {
            journey += Time.deltaTime * weaponData.returnSpeed;
            transform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, journey / weaponData.recoilDistance);
            yield return null;
        }

        transform.localPosition = originalPosition; // Garante que a arma volte exatamente para o lugar
        recoilCoroutine = null;
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
            // A CHAMADA CORRIGIDA: Agora passa os 7 argumentos que o projétil espera.
            proj.Initialize(
                weaponData.bulletDamage,
                weaponData.bulletSpeed,
                weaponData.bulletLifetime,
                weaponData.pierceCount,
                weaponData.damageFalloff,
                weaponData.bulletKnockback,
                weaponData.bulletKnockbackDirection // <-- PARÂMETRO ADICIONADO
            );
        }
    }
    public int GetAmmoNeeded()
    {
        // Retorna a quantidade de munição necessária para encher o pente.
        // Garante que não retorne um número negativo se por algum bug a CurrentAmmo for maior que o magazineSize.
        return Mathf.Max(0, weaponData.magazineSize - CurrentAmmo);
    }

    // A função FirePowder original que estava aqui foi substituída pelo DesperationAttack modificado.
    // Se a FirePowder original tinha alguma outra finalidade, ela precisaria ser reavaliada.
    // Pelo contexto, DesperationAttack é o "ataque de pólvora".

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