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
    public Transform groundCheck_A; // Renomeie o antigo para A
    public Transform groundCheck_B; // Adicione este novo
    public Transform wallCheck;
    public Transform ledgeCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public float wallCheckDistance = 1.0f;

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
        if (groundCheck_A == null || groundCheck_B == null) return false;
        // A IA está no chão se o PÉ A OU o PÉ B estiverem tocando o chão.
        return Physics2D.OverlapCircle(groundCheck_A.position, groundCheckRadius, groundLayer) ||
               Physics2D.OverlapCircle(groundCheck_B.position, groundCheckRadius, groundLayer);
    }

    public bool IsObstacleAhead()
    {
        if (wallCheck == null) return false;
        return Physics2D.Raycast(wallCheck.position, Vector2.right * currentFacingDirection, wallCheckDistance, groundLayer);
    }
    public void Nudge(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
    }
    public bool IsLedgeAhead()
    {
        if (ledgeCheck == null)
        {
            Debug.LogError("Referência do LedgeCheck NÃO FOI ATRIBUÍDA no Inspector!");
            return false;
        }

        Vector2 rayOrigin = ledgeCheck.position;

        // --- CORREÇÃO DA MIOPIA ---
        // Aumentamos a distância para dar uma margem de segurança maior.
        // 1.5f é uma distância boa para não detectar o chão de um abismo,
        // mas longa o suficiente para nunca falhar na beirada.
        float rayDistance = 1.5f; // <--- VALOR AUMENTADO

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayer);

        if (hit.collider != null)
        {
            Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, Color.green);
            return false;
        }
        else
        {
            Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, Color.red);
            // Descomente a linha abaixo se quiser spam no console para depuração
            // Debug.Log($"FRAME {Time.frameCount}: BEIRADA DETECTADA!");
            return true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (groundCheck_A != null) { Gizmos.DrawWireSphere(groundCheck_A.position, groundCheckRadius); }
        if (groundCheck_B != null) { Gizmos.DrawWireSphere(groundCheck_B.position, groundCheckRadius); }
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