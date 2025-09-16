// NOME DO ARQUIVO: SlashEffect.cs - VERSÃO COMPLETA E FINAL

using UnityEngine;
using System.Collections.Generic;

// Garante que o GameObject sempre terá os componentes necessários para funcionar.
[RequireComponent(typeof(Collider2D), typeof(ProjectileAnimatorController), typeof(Animator))]
public class SlashEffect : MonoBehaviour
{
    // --- Dados do Ataque (recebidos da MeeleeWeapon) ---
    private float damage;
    private float knockbackPower;

    // --- Componentes & Referências ---
    private ProjectileAnimatorController projectileAnimator;
    private Collider2D attackCollider;
    private Animator animator;

    // --- Controle de Lógica ---
    // Lista para garantir que cada inimigo só seja atingido uma vez por um único golpe.
    private List<Collider2D> targetsHit;

    void Awake()
    {
        // Pega as referências de todos os componentes necessários no mesmo GameObject.
        projectileAnimator = GetComponent<ProjectileAnimatorController>();
        attackCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        // Garante que o collider seja um trigger para não causar colisões físicas indesejadas.
        attackCollider.isTrigger = true;

        // Inicializa a lista de alvos atingidos.
        targetsHit = new List<Collider2D>();
    }

    /// <summary>
    /// Função de inicialização chamada pela MeeleeWeapon logo após a instanciação.
    /// Configura o dano, o knockback e a animação a ser tocada.
    /// </summary>a
    public void Initialize(float damageAmount, float knockbackForce, ProjectileAnimState animationToPlay)
    {
        this.damage = damageAmount;
        this.knockbackPower = knockbackForce;

        if (projectileAnimator != null)
        {
            projectileAnimator.PlayAnimation(animationToPlay);
        }
    }

    /// <summary>
    /// Ajusta a velocidade da animação deste efeito de corte, sincronizando com o comboSpeed.
    /// </summary>
    public void SetSpeed(float speedMultiplier)
    {
        if (animator != null)
        {
            // Garante que a velocidade não seja negativa.
            animator.speed = Mathf.Max(0.1f, speedMultiplier);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se já atingimos este alvo, ignora.
        if (targetsHit.Contains(other))
        {
            return;
        }

        // Tenta encontrar o componente da IA do inimigo.
        var enemyAI = other.GetComponent<AIController_Basic>();
        if (enemyAI != null)
        {
            // Adiciona o alvo à lista para não atingi-lo novamente.
            targetsHit.Add(other);

            // Calcula a direção do ataque para o knockback.
            Vector2 attackDirection = (other.transform.position - transform.position).normalized;

            // Chama a função TakeDamage da IA, passando todos os dados do ataque.
            enemyAI.TakeDamage(damage, attackDirection, knockbackPower);
        }
    }

    /// <summary>
    /// Esta função pública é projetada para ser chamada por um Animation Event no final da animação do efeito de corte.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}