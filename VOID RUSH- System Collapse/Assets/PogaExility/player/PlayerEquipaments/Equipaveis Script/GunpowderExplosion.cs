using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    // --- Dados da Explos�o (recebidos da RangedWeapon) ---
    private float damage;
    private float radius;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection; // <-- VARI�VEL ADICIONADA

    [SerializeField] private LayerMask enemyLayer;

    void Start()
    {
        // Toca a anima��o de explos�o assim que o objeto � criado.
        GetComponent<ProjectileAnimatorController>().PlayAnimation(ProjectileAnimState.polvora);
    }

    /// <summary>
    /// Fun��o de inicializa��o chamada pela RangedWeapon.
    /// Configura todos os par�metros da explos�o de uma s� vez.
    /// </summary>
    public void Initialize(float damageAmount, float explosionRadius, float knockbackForce, RangedKnockbackDirection knockbackDir)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
        this.knockbackPower = knockbackForce;
        this.knockbackDirection = knockbackDir;
    }

    /// <summary>
    /// Esta fun��o � chamada por um Animation Event no frame da anima��o onde o dano deve ocorrer.
    /// </summary>
    public void TriggerDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<AIController_Basic>(out AIController_Basic enemy))
            {
                Vector2 finalKnockbackDirection;

                // Decide qual vetor de dire��o usar com base na instru��o recebida.
                switch (knockbackDirection)
                {
                    case RangedKnockbackDirection.Frente:
                        // Para uma explos�o, "Frente" significa empurrar do centro para fora.
                        finalKnockbackDirection = (hit.transform.position - transform.position).normalized;
                        // Garante que a dire��o n�o seja zero se o inimigo estiver exatamente no centro.
                        if (finalKnockbackDirection == Vector2.zero)
                        {
                            finalKnockbackDirection = Vector2.up; // Empurra para cima como padr�o.
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
    /// Esta fun��o � chamada por um Animation Event no final da anima��o para destruir o objeto.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}