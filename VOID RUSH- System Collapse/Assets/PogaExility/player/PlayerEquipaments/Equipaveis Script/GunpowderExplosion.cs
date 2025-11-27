using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    // --- Dados da Explosão (recebidos da RangedWeapon) ---
    private float damage;
    private float radius;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection;
    private Vector2 shotDirection;

    [SerializeField] private LayerMask enemyLayer;

    // --- ADIÇÃO: Variável de Áudio ---
    [Header("Áudio")]
    [SerializeField] private AudioClip explosionSound;

    void Start()
    {
        // Toca a animação visual
        GetComponent<ProjectileAnimatorController>().PlayAnimation(ProjectileAnimState.polvora);

        // --- ADIÇÃO: Toca o som da explosão ---
        if (AudioManager.Instance != null && explosionSound != null)
        {
            AudioManager.Instance.PlaySoundEffect(explosionSound, transform.position);
        }
    }

    /// <summary>
    /// Função de inicialização final, chamada pela RangedWeapon.
    /// Configura todos os parâmetros da explosão, incluindo a direção do cano da arma.
    /// </summary>
    public void Initialize(float damageAmount, float explosionRadius, float knockbackForce, RangedKnockbackDirection knockbackDir, Vector2 fireDirection)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
        this.knockbackPower = knockbackForce;
        this.knockbackDirection = knockbackDir;
        this.shotDirection = fireDirection;
    }

    /// <summary>
    /// Esta função é chamada por um Animation Event no frame da animação onde o dano deve ocorrer.
    /// </summary>
    public void TriggerDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            // Lógica corrigida usando EnemyHealth
            if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
            {
                Vector2 finalKnockbackDirection;

                switch (knockbackDirection)
                {
                    case RangedKnockbackDirection.Frente:
                        // LÓGICA MANTIDA: Usa a direção do cano da arma que foi guardada.
                        finalKnockbackDirection = this.shotDirection;
                        break;

                    default:
                        // Comportamento padrão: empurra do centro para fora.
                        finalKnockbackDirection = (hit.transform.position - transform.position).normalized;
                        break;
                }

                // Aplica o dano no novo sistema de vida
                enemy.TakeDamage(this.damage, finalKnockbackDirection, this.knockbackPower);
            }
        }
    }

    /// <summary>
    /// Esta função é chamada por um Animation Event no final da animação para destruir o objeto.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}