using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    // --- Dados da Explos�o (recebidos da RangedWeapon) ---
    private float damage;
    private float radius;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection;
    private Vector2 shotDirection; // <-- VARI�VEL ADICIONADA PARA GUARDAR A DIRE��O DO TIRO

    [SerializeField] private LayerMask enemyLayer;

    void Start()
    {
        GetComponent<ProjectileAnimatorController>().PlayAnimation(ProjectileAnimState.polvora);
    }

    /// <summary>
    /// Fun��o de inicializa��o final, chamada pela RangedWeapon.
    /// Configura todos os par�metros da explos�o, incluindo a dire��o do cano da arma.
    /// </summary>
    public void Initialize(float damageAmount, float explosionRadius, float knockbackForce, RangedKnockbackDirection knockbackDir, Vector2 fireDirection)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
        this.knockbackPower = knockbackForce;
        this.knockbackDirection = knockbackDir;
        this.shotDirection = fireDirection; // <-- ARMAZENA A DIRE��O RECEBIDA
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

                switch (knockbackDirection)
                {
                    case RangedKnockbackDirection.Frente:
                        // L�GICA CORRIGIDA: Usa a dire��o do cano da arma que foi guardada.
                        finalKnockbackDirection = this.shotDirection;
                        break;

                    default:
                        // Comportamento padr�o: empurra do centro para fora.
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