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
    private Vector2 precalculatedKnockbackDirection; // <-- NOVA VARI�VEL para guardar a dire��o

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
    /// Configura o dano, o knockback, a dire��o pr�-calculada e a anima��o a ser tocada.
    /// </summary>
    public void Initialize(float damageAmount, float knockbackForce, Vector2 knockbackDir, ProjectileAnimState animationToPlay)
    {
        this.damage = damageAmount;
        this.knockbackPower = knockbackForce;
        this.precalculatedKnockbackDirection = knockbackDir; // <-- ARMAZENA A DIRE��O RECEBIDA

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


    // DENTRO DO SCRIPT: SlashEffect.cs

    void OnTriggerEnter2D(Collider2D other)
    {
        // A verifica��o para n�o atingir o mesmo alvo m�ltiplas vezes permanece a mesma.
        if (targetsHit.Contains(other))
        {
            return;
        }

        // Tenta encontrar o nosso novo componente unificado.
        var objetoInterativo = other.GetComponent<ObjetoInterativo>();
        if (objetoInterativo != null)
        {
            // --- IN�CIO DA MUDAN�A ---
            // Agora chamamos a nova fun��o 'ReceberHit', que � o ponto de entrada para todos os ataques.
            objetoInterativo.ReceberHit(TipoDeAtaqueAceito.ApenasMelee);
            // --- FIM DA MUDAN�A ---

            // Adiciona � lista de alvos atingidos para n�o interagir novamente com o mesmo golpe.
            targetsHit.Add(other);

            // Retorna para n�o tentar, no mesmo objeto, procurar por um inimigo.
            return;
        }

        // Se n�o era um objeto interativo, a l�gica para inimigos continua exatamente como antes.
        var enemyAI = other.GetComponent<AIController_Basic>();
        if (enemyAI != null)
        {
            targetsHit.Add(other);
            enemyAI.TakeDamage(damage, precalculatedKnockbackDirection, knockbackPower);
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