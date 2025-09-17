using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    #region REFERENCES & STATE
    private Rigidbody2D _rb;
    private CapsuleCollider2D _collider;
    [HideInInspector] public float currentFacingDirection = 1f;
    [HideInInspector] public bool isFacingRight = true;
    [HideInInspector] public float _currentSpeed = 0f;
    private float _originalGravityScale;
    private Vector2 _standingColliderSize;
    private Vector2 _crouchingColliderSize;
    public bool IsCrouching { get; private set; }
    public bool IsClimbing { get; private set; }
    #endregion

    #region CONFIGURATION
    [Header("▶ Atributos de Movimento")]
    public float acceleration = 5f;
    public float deceleration = 8f;
    public float climbSpeed = 4f;
    [Header("▶ Atributos Físicos")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckDistance = 0.1f;
    #endregion

    #region UNITY LIFECYCLE
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CapsuleCollider2D>();
        isFacingRight = transform.localScale.x > 0;
        currentFacingDirection = isFacingRight ? 1 : -1;
        _originalGravityScale = _rb.gravityScale;
        _standingColliderSize = _collider.size;
        _crouchingColliderSize = new Vector2(_standingColliderSize.x * 1.5f, _standingColliderSize.y * 0.6f);
        // --- DEBUG ---
        Debug.Log($"[MOTOR] Awake: Motor inicializado. Facing Direction: {currentFacingDirection}");
    }

    void FixedUpdate()
    {
        if (!IsClimbing) { _rb.linearVelocity = new Vector2(_currentSpeed * currentFacingDirection, _rb.linearVelocity.y); }
    }
    #endregion

    #region PUBLIC API (COMMANDS)
    public void Move(float topSpeed) { _currentSpeed = Mathf.MoveTowards(_currentSpeed, topSpeed, acceleration * Time.deltaTime); }
    public void Stop() { _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime); }
    public void HardStop() { if (_currentSpeed != 0) Debug.Log("[MOTOR] Command: HardStop()"); _currentSpeed = 0; _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y); }
    public void Brake() { Debug.Log("[MOTOR] Command: Brake()"); _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime); }
    public void Jump(float jumpForce) { if (IsGrounded()) { Debug.Log("[MOTOR] Command: Jump() with force: " + jumpForce); _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse); } }
    public void StartClimb() { if (IsClimbing) return; Debug.Log("[MOTOR] State Change: StartClimb()"); IsClimbing = true; _rb.gravityScale = 0; _currentSpeed = 0; _rb.linearVelocity = Vector2.zero; }
    public void Climb(float verticalDirection) { if (!IsClimbing) return; _rb.linearVelocity = new Vector2(0, verticalDirection * climbSpeed); }
    public void StopClimb() { if (!IsClimbing) return; Debug.Log("[MOTOR] State Change: StopClimb()"); IsClimbing = false; _rb.gravityScale = _originalGravityScale; }
    public void StartCrouch() { if (IsCrouching) return; Debug.Log("[MOTOR] State Change: StartCrouch()"); IsCrouching = true; _collider.size = _crouchingColliderSize; }
    public void StopCrouch() { if (!IsCrouching) return; Debug.Log("[MOTOR] State Change: StopCrouch()"); IsCrouching = false; _collider.size = _standingColliderSize; }
    public void Flip() { Debug.Log("[MOTOR] Command: Flip()"); isFacingRight = !isFacingRight; currentFacingDirection *= -1; transform.Rotate(0f, 180f, 0f); }
    #endregion

    #region PUBLIC API (QUERIES)
    public bool IsGrounded() => groundCheck != null && Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    #endregion
}