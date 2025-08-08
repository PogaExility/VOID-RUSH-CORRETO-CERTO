using UnityEngine;

// Versão FINAL do Motor. Usa um Transform dedicado para o LedgeCheck.
[RequireComponent(typeof(Rigidbody2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    [Header("Referências de Componentes")]
    private Rigidbody2D rb;

    [Header("Parâmetros de Movimento")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;

    [Header("Verificação de Ambiente")]
    [Tooltip("Objeto filho nos pés do inimigo.")]
    public Transform groundCheck;
    [Tooltip("Objeto filho na frente do inimigo.")]
    public Transform wallCheck;
    [Tooltip("NOVO: Objeto filho na frente e na altura dos pés.")]
    public Transform ledgeCheck; // <- NOVA REFERÊNCIA

    [Tooltip("A layer que o script considera como 'chão'.")]
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public float wallCheckDistance = 0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // --- AÇÕES (MÚSCULOS) ---

    public void Move(float direction)
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void DirectionalJump(float direction)
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            float horizontalJumpForce = direction * (moveSpeed * 0.75f);
            rb.AddForce(new Vector2(horizontalJumpForce, jumpForce), ForceMode2D.Impulse);
        }
    }

    // --- SENTIDOS ---

    public bool IsGrounded() => Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    public bool IsWallAhead() => Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayer);
    public bool IsLedgeAhead()
    {
        // Agora usa a posição do objeto ledgeCheck.
        return !Physics2D.Raycast(ledgeCheck.position, Vector2.down, 2f, groundLayer);
    }

    // --- DEBUG VISUAL ---
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (transform.right * wallCheckDistance));
        }
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.magenta;
            // CORREÇÃO: Usamos (Vector3)Vector2.down para garantir que a soma seja entre dois Vector3.
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + (Vector3)Vector2.down * 2f);
        }
    }
}