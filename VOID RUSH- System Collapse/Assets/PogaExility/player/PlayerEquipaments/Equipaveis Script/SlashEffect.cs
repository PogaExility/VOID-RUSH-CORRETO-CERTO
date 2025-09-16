// NOME DO ARQUIVO: SlashEffect.cs

using UnityEngine;
using System.Collections.Generic;

// Garante que o GameObject sempre ter� os componentes necess�rios para funcionar.
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ProjectileAnimatorController))]
public class SlashEffect : MonoBehaviour
{
    // --- Dados do Ataque ---
    private float damage;
    private float knockbackPower;

    // --- Componentes & Refer�ncias ---
    private ProjectileAnimatorController projectileAnimator;
    private Collider2D attackCollider;

    // --- Controle de L�gica ---
    // Lista para garantir que cada inimigo s� seja atingido uma vez por um �nico golpe.
    private List<Collider2D> targetsHit;

    void Awake()
    {
        // Pega as refer�ncias dos componentes no mesmo GameObject.
        projectileAnimator = GetComponent<ProjectileAnimatorController>();
        attackCollider = GetComponent<Collider2D>();

        // Garante que o collider seja um trigger para n�o causar colis�es f�sicas indesejadas.
        attackCollider.isTrigger = true;

        // Inicializa a lista de alvos atingidos.
        targetsHit = new List<Collider2D>();
    }

    /// <summary>
    /// Fun��o de inicializa��o chamada pela MeeleeWeapon logo ap�s a instancia��o.
    /// Configura o dano, o knockback e a anima��o a ser tocada.
    /// </summary>
    public void Initialize(float damageAmount, float knockbackForce, string animationToPlay)
    {
        this.damage = damageAmount;
        this.knockbackPower = knockbackForce;

        // Comanda o nosso "maestro" de anima��o para tocar o clipe correto.
        if (projectileAnimator != null && !string.IsNullOrEmpty(animationToPlay))
        {
            projectileAnimator.PlayAnimation(animationToPlay);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (targetsHit.Contains(other))
        {
            return;
        }

        // --- CORRE��O AQUI ---
        // Tenta pegar o componente AIController_Basic em vez do gen�rico EnemyStats.
        var enemyAI = other.GetComponent<AIController_Basic>();
        if (enemyAI != null)
        {
            targetsHit.Add(other);
            Vector2 attackDirection = (other.transform.position - transform.position).normalized;

            // Chama a fun��o TakeDamage correta no script da IA.
            enemyAI.TakeDamage(damage, attackDirection, knockbackPower);
        }
    }

    /// <summary>
    /// Esta fun��o p�blica � chamada por um Animation Event no final da anima��o do efeito de corte.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}