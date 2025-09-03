using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    #region REFERENCES
    private Rigidbody2D _rb;
    #endregion

    #region STATE
    [HideInInspector] public float currentFacingDirection = 1f;
    [HideInInspector] public bool isFacingRight = true;
    #endregion

    #region CONFIGURATION
    [Header("▶ Configuração de Movimento")]
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    #endregion

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    #region PUBLIC API (Commands)
    public void Move(float speed)
    {
        _rb.linearVelocity = new Vector2(currentFacingDirection * speed, _rb.linearVelocity.y);
    }

    public void Stop()
    {
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
    }

    public void Jump(float jumpForce)
    {
        if (IsGrounded())
        {
            _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void Climb(float climbSpeed)
    {
        _rb.linearVelocity = new Vector2(0, climbSpeed);
        _rb.gravityScale = 0; // Desativa a gravidade ao escalar
    }

    public void StopClimbing()
    {
        _rb.gravityScale = 1; // Reativa a gravidade
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        currentFacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        if ((targetPosition.x > transform.position.x && !isFacingRight) || (targetPosition.x < transform.position.x && isFacingRight))
        {
            Flip();
        }
    }
    #endregion

    #region PUBLIC API (Queries)
    public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
    }
    #endregion
}