using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    #region 1. Variáveis e Configurações
    [Header("Configuração de Dano")]
    private float damage;
    private float radius;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection;
    private Vector2 shotDirection;

    [Header("Configuração de Alvos")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Configuração de Áudio")]
    [Tooltip("O som da explosão.")]
    [SerializeField] private AudioClip explosionSound;

    [Tooltip("Multiplicador de volume específico para ESTA explosão (1 = normal).")]
    [Range(0f, 2f)]
    [SerializeField] private float explosionVolume = 1f;
    #endregion

    #region 2. Ciclo de Vida
    void Start()
    {
        // 1. Toca a animação visual
        GetComponent<ProjectileAnimatorController>().PlayAnimation(ProjectileAnimState.polvora);

        // 2. Toca o som usando o Gerente Global
        // Usamos o AudioManager porque ele cria um objeto temporário para o som.
        // Assim, mesmo que a explosão visual suma, o som termina de tocar.
        if (AudioManager.Instance != null && explosionSound != null)
        {
            // Passamos o explosionVolume como 3º parâmetro para ter controle individual
            AudioManager.Instance.PlaySoundEffect(explosionSound, transform.position, explosionVolume);
        }
    }
    #endregion

    #region 3. Inicialização e Lógica
    /// <summary>
    /// Função chamada pela arma para configurar a explosão antes dela aparecer.
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
    /// Chamado via Animation Event (no momento exato da explosão visual).
    /// </summary>
    public void TriggerDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
            {
                Vector2 finalKnockbackDirection;

                switch (knockbackDirection)
                {
                    case RangedKnockbackDirection.Frente:
                        // Empurra na direção do tiro
                        finalKnockbackDirection = this.shotDirection;
                        break;

                    default:
                        // Empurra do centro da explosão para fora
                        finalKnockbackDirection = (hit.transform.position - transform.position).normalized;
                        break;
                }

                enemy.TakeDamage(this.damage, finalKnockbackDirection, this.knockbackPower);
            }
        }
    }

    /// <summary>
    /// Chamado via Animation Event (no fim da animação).
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    #endregion
}