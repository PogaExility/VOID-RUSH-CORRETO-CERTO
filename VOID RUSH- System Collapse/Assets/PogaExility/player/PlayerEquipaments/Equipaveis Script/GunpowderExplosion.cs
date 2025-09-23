using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    // --- Dados da Explosão (recebidos da RangedWeapon) ---
    private float damage;
    private float radius;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection;
    private Vector2 shotDirection; // <-- VARIÁVEL ADICIONADA PARA GUARDAR A DIREÇÃO DO TIRO

    [SerializeField] private LayerMask enemyLayer;

    void Start()
    {
        GetComponent<ProjectileAnimatorController>().PlayAnimation(ProjectileAnimState.polvora);
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
        this.shotDirection = fireDirection; // <-- ARMAZENA A DIREÇÃO RECEBIDA
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

                switch (knockbackDirection)
                {
                    case RangedKnockbackDirection.Frente:
                        // LÓGICA CORRIGIDA: Usa a direção do cano da arma que foi guardada.
                        finalKnockbackDirection = this.shotDirection;
                        break;

                    default:
                        // Comportamento padrão: empurra do centro para fora.
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