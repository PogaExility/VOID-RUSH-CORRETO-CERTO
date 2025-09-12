// NOME DO ARQUIVO: SlashEffect.cs

using UnityEngine;
using System.Collections.Generic;

// Garante que o GameObject sempre terá os componentes necessários para funcionar.
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ProjectileAnimatorController))]
public class SlashEffect : MonoBehaviour
{
    // --- Dados do Ataque ---
    private float damage;
    private float knockbackPower;

    // --- Componentes & Referências ---
    private ProjectileAnimatorController projectileAnimator;
    private Collider2D attackCollider;

    // --- Controle de Lógica ---
    // Lista para garantir que cada inimigo só seja atingido uma vez por um único golpe.
    private List<Collider2D> targetsHit;

    void Awake()
    {
        // Pega as referências dos componentes no mesmo GameObject.
        projectileAnimator = GetComponent<ProjectileAnimatorController>();
        attackCollider = GetComponent<Collider2D>();

        // Garante que o collider seja um trigger para não causar colisões físicas indesejadas.
        attackCollider.isTrigger = true;

        // Inicializa a lista de alvos atingidos.
        targetsHit = new List<Collider2D>();
    }

    /// <summary>
    /// Função de inicialização chamada pela MeeleeWeapon logo após a instanciação.
    /// Configura o dano, o knockback e a animação a ser tocada.
    /// </summary>
      // DENTRO DE SlashEffect.cs

    public void Initialize(float damageAmount, float knockbackForce, ProjectileAnimState animationToPlay)
    {
        this.damage = damageAmount;
        this.knockbackPower = knockbackForce;

        if (projectileAnimator != null)
        {
            // CORREÇÃO: Chama a função PlayAnimation que agora aceita o ENUM.
            projectileAnimator.PlayAnimation(animationToPlay);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (targetsHit.Contains(other))
        {
            return;
        }

        // --- CORREÇÃO AQUI ---
        // Tenta pegar o componente AIController_Basic em vez do genérico EnemyStats.
        var enemyAI = other.GetComponent<AIController_Basic>();
        if (enemyAI != null)
        {
            targetsHit.Add(other);
            Vector2 attackDirection = (other.transform.position - transform.position).normalized;

            // Chama a função TakeDamage correta no script da IA.
            enemyAI.TakeDamage(damage, attackDirection, knockbackPower);
        }
    }

    /// <summary>
    /// Esta função pública é chamada por um Animation Event no final da animação do efeito de corte.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}