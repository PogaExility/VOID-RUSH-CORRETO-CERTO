using UnityEngine;

// Versão FINAL do Motor, com a correção para o desenho do Gizmo.
[RequireComponent(typeof(Rigidbody2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    [Header("Referências de Componentes")]
    private Rigidbody2D rb;

    [Header("Parâmetros de Movimento")]
    public float moveSpeed = 4f;

    [Header("Verificação de Ambiente")]
    [Tooltip("Objeto filho nos pés do inimigo.")]
    public Transform groundCheck;
    [Tooltip("Objeto filho na frente do inimigo.")]
    public Transform wallCheck;
    [Tooltip("Objeto filho na frente e na altura dos pés.")]
    public Transform ledgeCheck;

    [Tooltip("A layer que o script considera como 'chão'.")]
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public float wallCheckDistance = 0.5f;

    // --- NOVO ---
    // Variável para saber a direção visual do personagem. O AIController irá atualizá-la.
    [HideInInspector] // Esconde do Inspector para não bagunçar
    public float currentFacingDirection = 1f; // 1 para direita, -1 para esquerda

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

    public void SetVelocity(float x, float y)
    {
        rb.linearVelocity = new Vector2(x, y);
    }

    // --- SENTIDOS ---

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public bool IsWallAhead()
    {
        if (wallCheck == null) return false;
        // O raio agora usa a direção correta
        return Physics2D.Raycast(wallCheck.position, Vector2.right * currentFacingDirection, wallCheckDistance, groundLayer);
    }

    public bool IsLedgeAhead()
    {
        if (ledgeCheck == null) return false;
        return !Physics2D.Raycast(ledgeCheck.position, Vector2.down, 2f, groundLayer);
    }

    // --- DEBUG VISUAL (COM A CORREÇÃO) ---
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
            // A linha do Gizmo agora usa a direção correta
            Vector3 wallCheckDirection = Vector3.right * currentFacingDirection;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + wallCheckDirection * wallCheckDistance);
        }
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * 2f);
        }
    }
}