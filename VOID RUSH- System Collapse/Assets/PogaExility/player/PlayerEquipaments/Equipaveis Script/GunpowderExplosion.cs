using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    // --- Dados da Explosão (recebidos da RangedWeapon) ---
    private float damage;
    private float radius;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection; // <-- VARIÁVEL ADICIONADA

    [SerializeField] private LayerMask enemyLayer;

    void Start()
    {
        // Toca a animação de explosão assim que o objeto é criado.
        GetComponent<ProjectileAnimatorController>().PlayAnimation(ProjectileAnimState.polvora);
    }

    /// <summary>
    /// Função de inicialização chamada pela RangedWeapon.
    /// Configura todos os parâmetros da explosão de uma só vez.
    /// </summary>
    public void Initialize(float damageAmount, float explosionRadius, float knockbackForce, RangedKnockbackDirection knockbackDir)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
        this.knockbackPower = knockbackForce;
        this.knockbackDirection = knockbackDir;
    }

    /// <summary>
    /// Esta função é chamada por um Animation Event no frame da animação onde o dano deve ocorrer.
    /// </summary>
    public void TriggerDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<AIController_Basic>(out AIController_Basic enemy))
            {
                Vector2 finalKnockbackDirection;

                // Decide qual vetor de direção usar com base na instrução recebida.
                switch (knockbackDirection)
                {
                    case RangedKnockbackDirection.Frente:
                        // Para uma explosão, "Frente" significa empurrar do centro para fora.
                        finalKnockbackDirection = (hit.transform.position - transform.position).normalized;
                        // Garante que a direção não seja zero se o inimigo estiver exatamente no centro.
                        if (finalKnockbackDirection == Vector2.zero)
                        {
                            finalKnockbackDirection = Vector2.up; // Empurra para cima como padrão.
                        }
                        break;

                    // Futuramente, outros 'cases' poderiam ser adicionados aqui.
                    default:
                        finalKnockbackDirection = (hit.transform.position - transform.position).normalized;
                        break;
                }

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