using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("CONFIGURA��O DO PREFAB")]
    [Tooltip("O ponto exato de onde o proj�til sai.")]
    [SerializeField] private Transform muzzlePoint;

    private int currentAmmo;
    private float lastAttackTime = -999f;
    private bool isReloading = false;

    void Start()
    {
        // Ao ser criada, j� carrega a arma com a muni��o m�xima
        currentAmmo = weaponData.magazineSize;
    }

    // A "ASSINATURA" do contrato. � aqui que a m�gica acontece.
    public override void Attack()
    {
        // 1. Checa a cad�ncia de tiro
        if (Time.time < lastAttackTime + weaponData.attackRate) return;
        if (isReloading) return;

        // 2. Checa a muni��o
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
        Debug.Log($"TIRO! Muni��o: {currentAmmo}");

        if (weaponData.bulletPrefab != null && muzzlePoint != null)
        {
            // Cria o proj�til no ponto certo, com a rota��o certa.
            Instantiate(weaponData.bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        }
    }

    private void FirePowder()
    {
        Debug.Log("Fagulha. Sem muni��o.");
    }

    // TODO: Adicionar uma fun��o Reload() que pode ser chamada pelo WeaponHandler
}