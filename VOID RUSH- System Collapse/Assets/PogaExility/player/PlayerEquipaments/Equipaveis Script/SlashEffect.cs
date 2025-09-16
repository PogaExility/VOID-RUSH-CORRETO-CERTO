// NOME DO ARQUIVO: SlashEffect.cs - VERS�O COMPLETA E FINAL

using UnityEngine;
using System.Collections.Generic;

// Garante que o GameObject sempre ter� os componentes necess�rios para funcionar.
[RequireComponent(typeof(Collider2D), typeof(ProjectileAnimatorController), typeof(Animator))]
public class SlashEffect : MonoBehaviour
{
    // --- Dados do Ataque (recebidos da MeeleeWeapon) ---
    private float damage;
    private float knockbackPower;

    // --- Componentes & Refer�ncias ---
    private ProjectileAnimatorController projectileAnimator;
    private Collider2D attackCollider;
    private Animator animator;

    // --- Controle de L�gica ---
    // Lista para garantir que cada inimigo s� seja atingido uma vez por um �nico golpe.
    private List<Collider2D> targetsHit;

    void Awake()
    {
        // Pega as refer�ncias de todos os componentes necess�rios no mesmo GameObject.
        projectileAnimator = GetComponent<ProjectileAnimatorController>();
        attackCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        // Garante que o collider seja um trigger para n�o causar colis�es f�sicas indesejadas.
        attackCollider.isTrigger = true;

        // Inicializa a lista de alvos atingidos.
        targetsHit = new List<Collider2D>();
    }

    /// <summary>
    /// Fun��o de inicializa��o chamada pela MeeleeWeapon logo ap�s a instancia��o.
    /// Configura o dano, o knockback e a anima��o a ser tocada.
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
    /// Ajusta a velocidade da anima��o deste efeito de corte, sincronizando com o comboSpeed.
    /// </summary>
    public void SetSpeed(float speedMultiplier)
    {
        if (animator != null)
        {
            // Garante que a velocidade n�o seja negativa.
            animator.speed = Mathf.Max(0.1f, speedMultiplier);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se j� atingimos este alvo, ignora.
        if (targetsHit.Contains(other))
        {
            return;
        }

        // Tenta encontrar o componente da IA do inimigo.
        var enemyAI = other.GetComponent<AIController_Basic>();
        if (enemyAI != null)
        {
            // Adiciona o alvo � lista para n�o atingi-lo novamente.
            targetsHit.Add(other);

            // Calcula a dire��o do ataque para o knockback.
            Vector2 attackDirection = (other.transform.position - transform.position).normalized;

            // Chama a fun��o TakeDamage da IA, passando todos os dados do ataque.
            enemyAI.TakeDamage(damage, attackDirection, knockbackPower);
        }
    }

    /// <summary>
    /// Esta fun��o p�blica � projetada para ser chamada por um Animation Event no final da anima��o do efeito de corte.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}