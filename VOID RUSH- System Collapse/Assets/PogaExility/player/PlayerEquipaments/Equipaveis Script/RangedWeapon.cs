using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("CONFIGURAÇÃO DO PREFAB")]
    [Tooltip("O ponto exato de onde o projétil sai.")]
    [SerializeField] private Transform muzzlePoint;

    private int currentAmmo;
    private float lastAttackTime = -999f;
    private bool isReloading = false;

    void Start()
    {
        // Ao ser criada, já carrega a arma com a munição máxima
        currentAmmo = weaponData.magazineSize;
    }

    // A "ASSINATURA" do contrato. É aqui que a mágica acontece.
    public override void Attack()
    {
        // 1. Checa a cadência de tiro
        if (Time.time < lastAttackTime + weaponData.attackRate) return;
        if (isReloading) return;

        // 2. Checa a munição
        if (currentAmmo > 0)
        {
            FireBullet();
        }
        else
        {
            FirePowder();
        }

        lastAttackTime = Time.time;
    }

    private void FireBullet()
    {
        currentAmmo--;
        Debug.Log($"TIRO! Munição: {currentAmmo}");

        if (weaponData.bulletPrefab != null && muzzlePoint != null)
        {
            // Cria o projétil no ponto certo, com a rotação certa.
            Instantiate(weaponData.bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        }
    }

    private void FirePowder()
    {
        Debug.Log("Fagulha. Sem munição.");
    }

    // TODO: Adicionar uma função Reload() que pode ser chamada pelo WeaponHandler
}