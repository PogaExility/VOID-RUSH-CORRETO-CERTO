// NOME DO ARQUIVO: BusterWeapon.cs (NOVO SCRIPT)
using UnityEngine;
using System.Collections;

public class BusterWeapon : WeaponBase
{
    [Header("Referências")]
    [SerializeField] private Transform muzzlePoint;

    private bool isCharging = false;
    private float currentChargeTime = 0f;
    private Coroutine chargeCoroutine;

    // A função Attack será chamada a cada frame enquanto o botão estiver pressionado
    public override void Attack()
    {
        // Se o botão acabou de ser pressionado, inicia a carga
        if (!isCharging)
        {
            isCharging = true;
            chargeCoroutine = StartCoroutine(ChargeShotCoroutine());
        }
    }

    // Esta função será chamada quando o botão de ataque for solto
    public void ReleaseAttack()
    {
        if (!isCharging) return;

        isCharging = false;
        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
        }

        // Verifica se a carga atingiu o tempo mínimo para um tiro carregado
        if (currentChargeTime >= weaponData.chargeTime)
        {
            FireChargedShot();
        }
        else
        {
            FireNormalShot();
        }

        // Reseta o tempo de carga
        currentChargeTime = 0f;
    }

    private IEnumerator ChargeShotCoroutine()
    {
        currentChargeTime = 0f;
        while (isCharging)
        {
            currentChargeTime += Time.deltaTime;
            // Aqui você pode adicionar lógica para efeitos visuais de carga
            yield return null;
        }
    }

    private void FireNormalShot()
    {
        Debug.Log("Disparando tiro normal do Buster!");
        if (weaponData.busterShotPrefab == null) return;

        GameObject projGO = Instantiate(weaponData.busterShotPrefab, muzzlePoint.position, muzzlePoint.rotation);
        Projectile proj = projGO.GetComponent<Projectile>();
        if (proj != null)
        {
            // Use os valores do ItemSO para o tiro normal
            proj.Initialize(
                weaponData.bulletDamage,
                weaponData.bulletSpeed,
                weaponData.bulletLifetime,
                weaponData.pierceCount,
                weaponData.damageFalloff,
                weaponData.bulletKnockback // << APLICA KNOCKBACK DO TIRO NORMAL
            );
        }
    }

    private void FireChargedShot()
    {
        Debug.Log("Disparando tiro CARREGADO do Buster!");
        if (weaponData.chargedShotPrefab == null) return;

        GameObject projGO = Instantiate(weaponData.chargedShotPrefab, muzzlePoint.position, muzzlePoint.rotation);
        Projectile proj = projGO.GetComponent<Projectile>();
        if (proj != null)
        {
            // Para o tiro carregado, você pode querer usar valores diferentes
            // Crie campos no ItemSO para "chargedShotKnockback", etc., se necessário.
            // Por enquanto, vamos usar os mesmos do tiro normal como exemplo.
            proj.Initialize(
                weaponData.chargedShotDamage, // Dano do tiro carregado
                weaponData.bulletSpeed,       // Pode ter velocidade diferente
                weaponData.bulletLifetime,
                weaponData.pierceCount,       // Pode ter pierce diferente
                weaponData.damageFalloff,
                weaponData.bulletKnockback * 2f // Ex: Knockback dobrado para o tiro carregado
            );
        }
    }
}