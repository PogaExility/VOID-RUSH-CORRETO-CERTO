using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    #region REFERENCES & STATE
    private Rigidbody2D _rb;
    private CapsuleCollider2D _collider;

    [HideInInspector] public float currentFacingDirection = 1f;
    [HideInInspector] public bool isFacingRight = true;

    // Tornada pública para que o Controller a possa verificar
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
    }

    void FixedUpdate()
    {
        if (!IsClimbing)
        {
            _rb.linearVelocity = new Vector2(_currentSpeed * currentFacingDirection, _rb.linearVelocity.y);
        }
    }
    #endregion

    #region PUBLIC API (COMMANDS)
    public void Move(float topSpeed)
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, topSpeed, acceleration * Time.deltaTime);
    }

    public void Stop()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime);
    }

    public void HardStop()
    {
        _currentSpeed = 0;
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
    }

    public void Brake()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime);
    }

    public void Jump(float jumpForce)
    {
        if (IsGrounded()) { _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse); }
    }

    public void StartClimb()
    {
        if (IsClimbing) return;
        IsClimbing = true;
        _rb.gravityScale = 0;
        _currentSpeed = 0;
        _rb.linearVelocity = Vector2.zero;
    }

    public void Climb(float verticalDirection)
    {
        if (!IsClimbing) return;
        _rb.linearVelocity = new Vector2(0, verticalDirection * climbSpeed);
    }

    public void StopClimb()
    {
        if (!IsClimbing) return;
        IsClimbing = false;
        _rb.gravityScale = _originalGravityScale;
    }

    public void StartCrouch()
    {
        if (IsCrouching) return;
        IsCrouching = true;
        _collider.size = _crouchingColliderSize;
    }

    public void StopCrouch()
    {
        if (!IsCrouching) return;
        IsCrouching = false;
        _collider.size = _standingColliderSize;
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        currentFacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }
    #endregion

    #region PUBLIC API (QUERIES)
    public bool IsGrounded() => groundCheck != null && Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    #endregion
}