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

    [Tooltip("Volume desta explosão (1 = normal).")]
    [Range(0f, 2f)]
    [SerializeField] private float explosionVolume = 1f;

    [Tooltip("Velocidade/Tom do som (1 = normal).")]
    [Range(0.5f, 3f)]
    [SerializeField] private float explosionPitch = 1f;

    [Tooltip("Tempo em segundos para CORTAR o áudio. Use isso para deixar a explosão seca (Ex: 0.3). Se deixar 0, toca o som inteiro.")]
    [Range(0f, 5f)]
    [SerializeField] private float audioDuration = 0.5f; // <-- NOVA VARIÁVEL DE CORTE
    #endregion

    #region 2. Ciclo de Vida
    void Start()
    {
        GetComponent<ProjectileAnimatorController>().PlayAnimation(ProjectileAnimState.polvora);

        if (AudioManager.Instance != null && explosionSound != null)
        {
            // Passamos todos os parâmetros, incluindo a duração do corte no final
            AudioManager.Instance.PlaySoundEffect(
                explosionSound,
                transform.position,
                explosionVolume,
                explosionPitch,
                audioDuration // <--- AQUI ESTÁ O CORTE
            );
        }
    }
    #endregion

    #region 3. Inicialização e Lógica
    public void Initialize(float damageAmount, float explosionRadius, float knockbackForce, RangedKnockbackDirection knockbackDir, Vector2 fireDirection)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
        this.knockbackPower = knockbackForce;
        this.knockbackDirection = knockbackDir;
        this.shotDirection = fireDirection;
    }

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
                        finalKnockbackDirection = this.shotDirection;
                        break;
                    default:
                        finalKnockbackDirection = (hit.transform.position - transform.position).normalized;
                        break;
                }
                enemy.TakeDamage(this.damage, finalKnockbackDirection, this.knockbackPower);
            }
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    #endregion
}