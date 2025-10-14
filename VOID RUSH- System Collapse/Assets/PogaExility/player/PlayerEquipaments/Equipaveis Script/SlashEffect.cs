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
    private Vector2 precalculatedKnockbackDirection; // <-- NOVA VARIÁVEL para guardar a direção

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
    /// Configura o dano, o knockback, a direção pré-calculada e a animação a ser tocada.
    /// </summary>
    public void Initialize(float damageAmount, float knockbackForce, Vector2 knockbackDir, ProjectileAnimState animationToPlay)
    {
        this.damage = damageAmount;
        this.knockbackPower = knockbackForce;
        this.precalculatedKnockbackDirection = knockbackDir; // <-- ARMAZENA A DIREÇÃO RECEBIDA

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


    // DENTRO DO SCRIPT: SlashEffect.cs

    void OnTriggerEnter2D(Collider2D other)
    {
        // A verificação para não atingir o mesmo alvo múltiplas vezes permanece a mesma.
        if (targetsHit.Contains(other))
        {
            return;
        }

        // Tenta encontrar o nosso novo componente unificado.
        var objetoInterativo = other.GetComponent<ObjetoInterativo>();
        if (objetoInterativo != null)
        {
            // --- INÍCIO DA MUDANÇA ---
            // Agora chamamos a nova função 'ReceberHit', que é o ponto de entrada para todos os ataques.
            objetoInterativo.ReceberHit(TipoDeAtaqueAceito.ApenasMelee);
            // --- FIM DA MUDANÇA ---

            // Adiciona à lista de alvos atingidos para não interagir novamente com o mesmo golpe.
            targetsHit.Add(other);

            // Retorna para não tentar, no mesmo objeto, procurar por um inimigo.
            return;
        }

        // Se não era um objeto interativo, a lógica para inimigos continua exatamente como antes.
        var enemyAI = other.GetComponent<AIController_Basic>();
        if (enemyAI != null)
        {
            targetsHit.Add(other);
            enemyAI.TakeDamage(damage, precalculatedKnockbackDirection, knockbackPower);
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