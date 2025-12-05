// NOME DO ARQUIVO: SlashEffect.cs - VERSÃO COM ÁUDIO

using UnityEngine;
using System.Collections.Generic;

// Garante que o GameObject sempre terá os componentes necessários para funcionar.
[RequireComponent(typeof(Collider2D), typeof(ProjectileAnimatorController), typeof(Animator))]
public class SlashEffect : MonoBehaviour
{
    #region 1. Variáveis e Configurações
    // --- Dados do Ataque (recebidos da MeeleeWeapon) ---
    private float damage;
    private float knockbackPower;
    private Vector2 precalculatedKnockbackDirection;

    // --- Configuração de Áudio (NOVO) ---
    [Header("Configuração de Áudio")]
    [Tooltip("O som do corte (swish/slash).")]
    [SerializeField] private AudioClip slashSound;

    [Tooltip("Volume deste corte (1 = normal).")]
    [Range(0f, 2f)]
    [SerializeField] private float slashVolume = 1f;

    [Tooltip("Velocidade/Tom do som. Aumente para deixar mais agudo/rápido.")]
    [Range(0.5f, 3f)]
    [SerializeField] private float slashPitch = 1f;

    [Tooltip("Tempo em segundos para cortar o áudio (0 = toca inteiro). Útil para ataques muito rápidos.")]
    [SerializeField] private float audioDuration = 0f;

    // --- Componentes & Referências ---
    private ProjectileAnimatorController projectileAnimator;
    private Collider2D attackCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Configuração Visual")]
    [Tooltip("Nome da Sorting Layer (Ex: 'Player', 'VFX', 'Foreground'). Isso ganha do Order in Layer.")]
    [SerializeField] private string sortingLayerName = "Default";
    [Tooltip("Ordem na camada. 32767 é o máximo.")]
    [SerializeField] private int orderInLayer = 32767;

    // --- Controle de Lógica ---
    // Lista para garantir que cada inimigo só seja atingido uma vez por um único golpe.
    private List<Collider2D> targetsHit;
    #endregion

    #region 2. Ciclo de Vida
    void Awake()
    {
        // Pega as referências de todos os componentes necessários no mesmo GameObject.
        projectileAnimator = GetComponent<ProjectileAnimatorController>();
        attackCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // <--- PEGA O RENDERER

        // Garante que o collider seja um trigger para não causar colisões físicas indesejadas.
        attackCollider.isTrigger = true;

        // Inicializa a lista de alvos atingidos.
        targetsHit = new List<Collider2D>();
    }

    void Start()
    {
        // --- FORÇA A ORDEM DE RENDERIZAÇÃO ---
        // Isso garante que o efeito fique na frente, corrigindo o bug visual.
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = orderInLayer;
        }

        // --- TOCA O SOM DO CORTE ---
        if (AudioManager.Instance != null && slashSound != null)
        {
            AudioManager.Instance.PlaySoundEffect(
                slashSound,
                transform.position,
                slashVolume,
                slashPitch,
                audioDuration
            );
        }
    }
    #endregion

    #region 3. Lógica de Inicialização e Colisão
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

        // Tenta encontrar o nosso novo componente unificado.
        var objetoInterativo = other.GetComponent<ObjetoInterativo>();
        if (objetoInterativo != null)
        {
            objetoInterativo.ReceberHit(TipoDeAtaqueAceito.ApenasMelee);

            // Adiciona à lista de alvos atingidos para não interagir novamente com o mesmo golpe.
            targetsHit.Add(other);

            // Retorna para não tentar, no mesmo objeto, procurar por um inimigo.
            return;
        }

        // Substituído AIController_Basic por EnemyHealth para corrigir o erro CS0246.
        var enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            targetsHit.Add(other);
            // Mantém sua lógica de direção pré-calculada
            enemyHealth.TakeDamage(damage, precalculatedKnockbackDirection, knockbackPower);
        }
    }

    /// <summary>
    /// Esta função pública é projetada para ser chamada por um Animation Event no final da animação do efeito de corte.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    #endregion
}