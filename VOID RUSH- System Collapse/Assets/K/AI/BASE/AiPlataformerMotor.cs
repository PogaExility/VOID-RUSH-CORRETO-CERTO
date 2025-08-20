using UnityEngine;

// Versão FINAL do Motor, como estava antes.
[RequireComponent(typeof(Rigidbody2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    [Header("Referências de Componentes")]
    private Rigidbody2D rb;

    [Header("Parâmetros de Movimento")]
    public float moveSpeed = 4f;

    [Header("Verificação de Ambiente")]
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ledgeCheck;
    public LayerMask groundLayer;

    public float groundCheckRadius = 0.2f;
    public float wallCheckDistance = 0.5f;

    [HideInInspector]
    public float currentFacingDirection = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(float direction) { rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y); }
    public void Stop() { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); }
    public void AddJumpForce(Vector2 force)
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, 0); // Mantém um pouco do momento, zera o Y
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public bool IsObstacleAhead()
    {
        if (wallCheck == null) return false;
        return Physics2D.Raycast(wallCheck.position, Vector2.right * currentFacingDirection, wallCheckDistance, groundLayer);
    }

    public bool IsLedgeAhead()
    {
        if (ledgeCheck == null) return false;
        return !Physics2D.Raycast(ledgeCheck.position, Vector2.down, 2f, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (wallCheck != null)
        {
            Vector3 wallCheckDirection = Vector3.right * currentFacingDirection;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + wallCheckDirection * wallCheckDistance);
        }
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * 2f);
        }
    }
}