// --- SCRIPT: AIMotor_Basic.cs ---
// Versão: 1.0 "Placeholder"

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AIMotor_Basic : MonoBehaviour
{
    private Rigidbody2D rb;
    [HideInInspector] public float currentFacingDirection = 1f;

    [Header("▶ REFERÊNCIAS DE SENSORES")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    
    [Header("▶ CONFIGURAÇÃO DOS SENSORES")]
    public float wallCheckDistance = 0.5f;
    public float groundCheckDistance = 0.3f;
    public float groundAheadProbeDistance = 0.6f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // EM: AIMotor_Basic.cs, #region Comandos de Ação (Músculos)

    public void ExecuteKnockback(float force, Vector2 direction)
    {
        // Verificação de segurança crucial!
        if (rb == null)
        {
            Debug.LogError("Não há Rigidbody2D para aplicar o knockback!");
            return;
        }

        // Se o Rigidbody for Kinematic, ele não será afetado por forças!
        // Ele PRECISA ser do tipo "Dynamic" para o knockback funcionar.
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            Debug.LogWarning("Rigidbody é Kinematic e não pode receber força de knockback.");
            return;
        }

        // Zera a velocidade atual para que o knockback seja mais impactante e consistente.
        rb.linearVelocity = Vector2.zero;

        // Adiciona a força. ForceMode2D.Impulse aplica a força instantaneamente, como um soco.
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        Debug.Log($"[AIMotor] Força de {force} aplicada na direção {direction}!");
    }
    // --- COMANDOS DE MOVIMENTO ---
    public void Move(float direction, float speed) { rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y); }
    public void Stop() { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); }
    public void Climb(float speed) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, speed); }

    // --- FUNÇÕES DE SENSORES ---
    public bool IsGrounded()
    {
        return Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    }

    public bool IsObstacleAhead()
    {
        return Physics2D.Raycast(wallCheck.position, Vector2.right * currentFacingDirection, wallCheckDistance, groundLayer);
    }
    
    public bool IsGroundAhead()
    {
        Vector2 probeOrigin = (Vector2)groundCheck.position + new Vector2(groundAheadProbeDistance * currentFacingDirection, 0);
        return Physics2D.Raycast(probeOrigin, Vector2.down, groundCheckDistance * 2, groundLayer);
    }
    
    // --- GIZMOS PARA FÁCIL CONFIGURAÇÃO ---
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null) {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
            
            Gizmos.color = IsGroundAhead() ? Color.green : Color.magenta;
            Vector2 probeOrigin = (Vector2)groundCheck.position + new Vector2(groundAheadProbeDistance * ((transform.localScale.x > 0) ? 1 : -1), 0);
            Gizmos.DrawLine(probeOrigin, probeOrigin + Vector2.down * groundCheckDistance * 2);
        }
        if (wallCheck != null) {
            Gizmos.color = IsObstacleAhead() ? Color.red : Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3.right * ((transform.localScale.x > 0) ? 1 : -1) * wallCheckDistance));
        }
    }
}