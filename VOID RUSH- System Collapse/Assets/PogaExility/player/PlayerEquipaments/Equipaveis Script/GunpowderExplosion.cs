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
    [SerializeField] private float audioDuration = 0.5f;

    [Header("Configuração de Seguimento")]
    private Transform followTarget; // O MuzzlePoint da arma
    private float spawnOffset;      // A distância do ItemSO
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
                audioDuration
            );
        }
    }

    // LateUpdate garante que a posição seja atualizada APÓS a arma se mover/animar no frame
    void LateUpdate()
    {
        UpdatePositionAndRotation();
    }
    #endregion

    #region 3. Inicialização e Lógica
    public void Initialize(float damageAmount, float explosionRadius, float knockbackForce, RangedKnockbackDirection knockbackDir, Vector2 fireDirection, Transform origin, float offset)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
        this.knockbackPower = knockbackForce;
        this.knockbackDirection = knockbackDir;
        this.shotDirection = fireDirection;
        this.followTarget = origin;
        this.spawnOffset = offset;

        // IMPORTANTE: Garante que não somos filhos de ninguém para evitar deformação de escala
        transform.SetParent(null);

        // Força a posição inicial imediata para não ter 1 frame de atraso visual
        UpdatePositionAndRotation();
    }

    private void UpdatePositionAndRotation()
    {
        if (followTarget != null)
        {
            // 1. Copia a rotação da arma
            transform.rotation = followTarget.rotation;

            // 2. A MÁGICA: TransformPoint pega o ponto local (X = offset) e calcula
            // onde ele está no mundo real baseado em como a arma está agora (posição e rotação).
            // Isso mantém a explosão "grudada" na ponta sem herdar a escala negativa do pai.
            transform.position = followTarget.TransformPoint(new Vector3(spawnOffset, 0f, 0f));
        }
        else
        {
            // Se a arma sumiu (trocou de arma ou destruiu o objeto), destroi a explosão
            Destroy(gameObject);
        }
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