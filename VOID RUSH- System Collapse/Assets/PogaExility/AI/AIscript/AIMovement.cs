using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AIMovement : MonoBehaviour
{
    [Header("▶ Referências dos Sensores")]
    [Tooltip("Ponto de origem para detectar paredes à frente.")]
    [SerializeField] private Transform wallCheck;
    [Tooltip("Ponto de origem para detectar beiradas de plataformas.")]
    [SerializeField] private Transform groundCheck;

    [Header("▶ Configuração dos Sensores")]
    [Tooltip("A que distância da parede o sensor deve detectar.")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [Tooltip("A que distância à frente a sonda de chão verifica.")]
    [SerializeField] private float groundAheadProbeDistance = 0.6f;
    [Tooltip("A distância para baixo que as sondas de chão verificam.")]
    [SerializeField] private float groundCheckRayLength = 0.5f;
    [Tooltip("A LayerMask que representa o chão e obstáculos.")]
    [SerializeField] private LayerMask groundLayer;

    // Componentes e Estado Interno
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    // --- Propriedades Públicas (Apenas Leitura) ---
    public bool IsFacingRight => isFacingRight;
    public Vector2 Velocity => rb.linearVelocity;

    #region Unity Lifecycle & Inicialização

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Garante que os sensores foram atribuídos no Inspector.
        if (wallCheck == null || groundCheck == null)
        {
            Debug.LogError($"O inimigo '{gameObject.name}' não tem os Transforms 'wallCheck' ou 'groundCheck' atribuídos no AIMovement. O motor será desativado.", this);
            this.enabled = false;
        }
    }

    #endregion

    #region Comandos do "Cérebro" (AIController)

    /// <summary>
    /// Move o Rigidbody na direção atual com a velocidade especificada.
    /// </summary>
    public void Move(float speed)
    {
        float direction = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Para o movimento horizontal do Rigidbody.
    /// </summary>
    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    /// <summary>
    /// Inverte a direção para a qual a IA está virada.
    /// </summary>
    public void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    /// <summary>
    /// Aplica uma força de knockback instantânea.
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb.bodyType != RigidbodyType2D.Dynamic) return;

        // Zera a velocidade para um impacto limpo e aplica o impulso.
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    #endregion

    #region Funções de Sensores (Perguntas para o "Cérebro")

    /// <summary>
    /// Verifica se há um obstáculo diretamente à frente.
    /// </summary>
    public bool IsFacingWall()
    {
        if (wallCheck == null) return false;
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        return Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, groundLayer);
    }

    /// <summary>
    /// Verifica se há chão um pouco à frente, para evitar quedas.
    /// </summary>
    public bool HasGroundAhead()
    {
        if (groundCheck == null) return false;
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 probeOrigin = (Vector2)groundCheck.position + (direction * groundAheadProbeDistance);
        return Physics2D.Raycast(probeOrigin, Vector2.down, groundCheckRayLength, groundLayer);
    }

    #endregion

    #region Gizmos para Debug

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Vector2 direction = (Application.isPlaying ? isFacingRight : transform.rotation.y == 0) ? Vector2.right : Vector2.left;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(wallCheck.position, (Vector2)wallCheck.position + (direction * wallCheckDistance));
        }

        if (groundCheck != null)
        {
            Vector2 direction = (Application.isPlaying ? isFacingRight : transform.rotation.y == 0) ? Vector2.right : Vector2.left;
            Vector2 probeOrigin = (Vector2)groundCheck.position + (direction * groundAheadProbeDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(probeOrigin, probeOrigin + (Vector2.down * groundCheckRayLength));
        }
    }
#endif

    #endregion
}