
// RangedWeapon.cs - VERS�O COMPLETA E CORRIGIDA
using System.Collections;
using UnityEngine;
public class RangedWeapon : WeaponBase
{
    [Header("CONFIGURA��O DO PREFAB")]
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
public override void Attack()
{
    if (Time.time < lastAttackTime + weaponData.attackRate || isReloading) return;

    // --- A��ES QUE ACONTECEM EM TODOS OS TIROS ---

    // 1. Recoil: Continua igual.
    if (recoilCoroutine != null) StopCoroutine(recoilCoroutine);
    recoilCoroutine = StartCoroutine(RecoilCoroutine());

    // 2. Efeito Visual da P�lvora (L�GICA CORRIGIDA)
    if (weaponData.gunpowderPrefab != null)
    {
        // A. Calculamos a posi��o para frente.
        Vector3 spawnPosition = muzzlePoint.position + (muzzlePoint.right * weaponData.gunpowderSpawnOffset);

        // B. Instanciamos o objeto na posi��o e rota��o certas.
        GameObject gunpowderGO = Instantiate(weaponData.gunpowderPrefab, spawnPosition, muzzlePoint.rotation);

        // C. A M�GICA: Imediatamente removemos qualquer parentesco.
        // Isso garante que ele n�o herde nenhuma escala e fique fixo no espa�o.
        gunpowderGO.transform.SetParent(null);
    }

    // --- A��ES QUE DEPENDEM DA MUNI��O (continua igual) ---

    if (CurrentAmmo > 0)
    {
        FireBullet();
    }
    else
    {
        DesperationAttack();
    }

    lastAttackTime = Time.time;
}

    // Dentro de RangedWeapon.cs

    private void DesperationAttack()
    {
        Debug.Log("Ataque de curto alcance sem muni��o!");

        // --- PARTE VISUAL CORRIGIDA ---
        if (weaponData.gunpowderPrefab != null)
        {
            // Ao instanciar, passamos 'muzzlePoint' como o segundo argumento.
            // Isso automaticamente torna o efeito um filho do muzzlePoint, fazendo-o segui-lo.
            GameObject explosionGO = Instantiate(weaponData.gunpowderPrefab, muzzlePoint);

            // Pega o script para passar os dados de dano/knockback (esta parte j� estava impl�cita, mas � bom garantir)
            GunpowderExplosion explosionScript = explosionGO.GetComponent<GunpowderExplosion>();
            if (explosionScript != null)
            {
                // Usa os dados do ItemSO para inicializar a explos�o.
                explosionScript.Initialize(weaponData.powderDamage, weaponData.powderRange);
            }
        }

        // --- PARTE DA F�SICA (PERMANECE IGUAL) ---
        // A �rea de dano invis�vel ainda � necess�ria para registrar os acertos.
        Collider2D[] hits = Physics2D.OverlapCircleAll(muzzlePoint.position, weaponData.powderRange);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<AIController_Basic>(out AIController_Basic enemy))
            {
                Vector2 knockbackDirection = (hit.transform.position - muzzlePoint.position).normalized;
                enemy.TakeDamage(weaponData.powderDamage, knockbackDirection, weaponData.powderKnockback);
            }
        }
    }



    // ADICIONE a corrotina do Recoil
    private IEnumerator RecoilCoroutine()
{
    Vector3 originalPosition = Vector3.zero; // A posi��o inicial da arma � sempre (0,0,0) em rela��o ao socket
    Vector3 recoilPosition = new Vector3(-weaponData.recoilDistance, 0, 0);

    float journey = 0f;

    // Movimento para tr�s (o "soco")
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
            weaponData.damageFalloff,
               weaponData.bulletKnockback

        );
    }
}
public int GetAmmoNeeded()
{
    // Retorna a quantidade de muni��o necess�ria para encher o pente.
    // Garante que n�o retorne um n�mero negativo se por algum bug a CurrentAmmo for maior que o magazineSize.
    return Mathf.Max(0, weaponData.magazineSize - CurrentAmmo);
}
private void FirePowder()
{
    if (weaponData.gunpowderPrefab == null)
    {
        Debug.Log("Click. Sem muni��o e sem prefab de p�lvora.");
        return;
    }

    // Instancia o prefab da explos�o no cano da arma.
    GameObject explosionGO = Instantiate(weaponData.gunpowderPrefab, muzzlePoint.position, muzzlePoint.rotation);

    // Pega o script da explos�o para passar os dados.
    GunpowderExplosion explosionScript = explosionGO.GetComponent<GunpowderExplosion>();
    if (explosionScript != null)
    {
        // Usa os dados do ItemSO para inicializar a explos�o.
        explosionScript.Initialize(weaponData.powderDamage, weaponData.powderRange);
    }
}

public bool IsReloading()
{
    return isReloading;
}

public void StartReload(int ammoToLoad, System.Action onReloadLogicFinished)
{
    if (isReloading || !gameObject.activeInHierarchy) return;
    // Guardamos a corrotina na vari�vel para podermos cancel�-la depois.
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

    // 1. A L�GICA ESPERA o tempo definido no seu ItemSO da arma.
    yield return new WaitForSeconds(weaponData.reloadTime);

    // 2. A L�GICA ADICIONA a muni��o.
    CurrentAmmo += ammoToLoad;
    isReloading = false;
    RaiseOnWeaponStateChanged(); // Avisa a UI que a muni��o mudou.

    // 3. A L�GICA AVISA o WeaponHandler que o trabalho dela terminou.
    onReloadLogicFinished?.Invoke();
}
}