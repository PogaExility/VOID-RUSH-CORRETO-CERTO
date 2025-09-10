// GunpowderExplosion.cs
using UnityEngine;

// Agora, este script REQUER que o ProjectileAnimator esteja no mesmo objeto.
[RequireComponent(typeof(ProjectileAnimator))]
public class GunpowderExplosion : MonoBehaviour
{
    private float damage;
    private float radius;
    [SerializeField] private LayerMask enemyLayer;

    // A referência para o nosso novo maestro.
    private ProjectileAnimator projectileAnimator;

    void Awake()
    {
        // Pega a referência do maestro no mesmo objeto.
        projectileAnimator = GetComponent<ProjectileAnimator>();
    }

    public void Initialize(float damageAmount, float explosionRadius)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
    }

    void Start()
    {
        // Assim que a explosão é criada, ela manda o maestro tocar a animação "polvora".
        projectileAnimator.PlayAnimation("polvora");
    }

    // ===================================================================
    // As funções para os Animation Events continuam sendo a melhor forma de sincronizar o dano.
    // ===================================================================

    public void TriggerDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<AIController_Basic>(out AIController_Basic enemy))
            {
                Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;
                enemy.TakeDamage(this.damage, knockbackDirection);
            }
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}