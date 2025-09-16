using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    private float damage;
    private float radius;
    private float knockbackPower; // <<< VARIÁVEL ADICIONADA
    [SerializeField] private LayerMask enemyLayer;

    void Start()
    {
        GetComponent<ProjectileAnimatorController>().PlayAnimation(ProjectileAnimState.polvora);
    }

    // A assinatura desta função MUDOU para aceitar o knockback
    public void Initialize(float damageAmount, float explosionRadius, float knockback)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
        this.knockbackPower = knockback; // <<< VALOR GUARDADO
    }

    // Sobrecarga para manter compatibilidade, caso seja chamada sem knockback
    public void Initialize(float damageAmount, float explosionRadius)
    {
        Initialize(damageAmount, explosionRadius, 0f);
    }

    public void TriggerDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<AIController_Basic>(out AIController_Basic enemy))
            {
                Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;

                // --- CHAMADA CORRIGIDA ---
                // Agora chama a função TakeDamage completa, passando o knockbackPower.
                enemy.TakeDamage(this.damage, knockbackDirection, this.knockbackPower);
            }
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}