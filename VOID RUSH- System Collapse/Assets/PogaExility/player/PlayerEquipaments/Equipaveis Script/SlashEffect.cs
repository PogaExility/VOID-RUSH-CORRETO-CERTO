// NOME DO ARQUIVO: SlashEffect.cs - VERSÃO CORRIGIDA

using UnityEngine;
using System.Collections.Generic;

// Garante que o GameObject sempre terá os componentes necessários para funcionar.
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ProjectileAnimatorController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class SlashEffect : MonoBehaviour
{

    [Header("Efeitos Sonoros")]
    [Tooltip("Som que toca quando este efeito de corte atinge um inimigo ou objeto.")]
    [SerializeField] private AudioClip hitSound;

    // --- Dados do Ataque (recebidos da MeeleeWeapon) ---
    private float damage;
    private float knockbackPower;
    private Vector2 precalculatedKnockbackDirection;

    // --- Componentes & Referências ---
    private ProjectileAnimatorController projectileAnimator;
    private Collider2D attackCollider;
    private Animator animator;
    private AudioSource audioSource;

    // --- Controle de Lógica ---
    // Lista para garantir que cada inimigo só seja atingido uma vez por um único golpe.
    private List<Collider2D> targetsHit;

    void Awake()
    {
        // Pega as referências de todos os componentes necessários no mesmo GameObject.
        projectileAnimator = GetComponent<ProjectileAnimatorController>();
        attackCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

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
        this.precalculatedKnockbackDirection = knockbackDir;

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
        // A verificação para não atingir o mesmo alvo múltiplas vezes permanece a mesma.
        if (targetsHit.Contains(other))
        {
            return;
        }

        bool targetWasHit = false;

        // Tenta encontrar o nosso novo componente unificado.
        var objetoInterativo = other.GetComponent<ObjetoInterativo>();
        if (objetoInterativo != null)
        {
            objetoInterativo.ReceberHit(TipoDeAtaqueAceito.ApenasMelee);

            // Adiciona à lista de alvos atingidos para não interagir novamente com o mesmo golpe.
            targetsHit.Add(other);
            targetWasHit = true;

            // Retorna para não tentar, no mesmo objeto, procurar por um inimigo.
            return;
        }

        // --- CORREÇÃO AQUI ---
        // Substituído AIController_Basic por EnemyHealth para corrigir o erro CS0246.
        var enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            targetsHit.Add(other);
            // Mantém sua lógica de direção pré-calculada
            enemyHealth.TakeDamage(damage, precalculatedKnockbackDirection, knockbackPower);
            targetWasHit = true;
        }

        if (targetWasHit && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
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