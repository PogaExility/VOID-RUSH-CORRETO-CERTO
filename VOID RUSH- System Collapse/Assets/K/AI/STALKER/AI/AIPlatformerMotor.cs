using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    #region REFERENCES & STATE
    private Rigidbody2D _rb;
    private CapsuleCollider2D _collider;

    [HideInInspector] public float currentFacingDirection = 1f;
    [HideInInspector] public bool isFacingRight = true;

    private float _currentSpeed = 0f;
    private float _originalGravityScale;

    private Vector2 _standingColliderSize;
    private Vector2 _crouchingColliderSize;

    public bool IsCrouching { get; private set; }
    public bool IsClimbing { get; private set; }
    #endregion

    #region CONFIGURATION
    [Header("▶ Atributos de Movimento")]
    [Tooltip("A velocidade com que a IA ganha velocidade.")]
    public float acceleration = 5f;
    [Tooltip("A velocidade com que a IA perde velocidade ao parar.")]
    public float deceleration = 8f;
    [Tooltip("A velocidade de escalada.")]
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
        // A física deve ser aplicada no FixedUpdate para consistência.
        if (!IsClimbing)
        {
            _rb.linearVelocity = new Vector2(_currentSpeed * currentFacingDirection, _rb.linearVelocity.y);
        }
    }
    #endregion

    #region PUBLIC API (COMMANDS)
    /// <summary>
    /// Acelera gradualmente até à velocidade máxima desejada.
    /// </summary>
    public void Move(float topSpeed)
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, topSpeed, acceleration * Time.deltaTime);
    }

    /// <summary>
    /// Desacelera gradualmente até parar.
    /// </summary>
    public void Stop()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime);
    }

    public void Brake()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime);
    }

    /// <summary>
    /// Aplica uma força de salto se estiver no chão.
    /// </summary>
    public void Jump(float jumpForce)
    {
        if (IsGrounded())
        {
            Debug.Log("[Motor] A executar SALTO!");
            _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// Inicia o estado de escalada, desativando a gravidade.
    /// </summary>
    public void StartClimb()
    {
        if (IsClimbing) return;
        IsClimbing = true;
        _rb.gravityScale = 0;
        _currentSpeed = 0; // Para o movimento horizontal
        _rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Move o corpo verticalmente enquanto escala.
    /// </summary>
    public void Climb(float verticalDirection)
    {
        if (!IsClimbing) return;
        _rb.linearVelocity = new Vector2(0, verticalDirection * climbSpeed);
    }

    /// <summary>
    /// Termina o estado de escalada, reativando a gravidade.
    /// </summary>
    public void StopClimb()
    {
        if (!IsClimbing) return;
        IsClimbing = false;
        _rb.gravityScale = _originalGravityScale;
    }

    /// <summary>
    /// Altera o colisor para o estado de agachado.
    /// </summary>
    public void StartCrouch()
    {
        if (IsCrouching) return;
        IsCrouching = true;
        _collider.size = _crouchingColliderSize;
    }

    /// <summary>
    /// Restaura o colisor para o estado normal.
    /// </summary>
    public void StopCrouch()
    {
        if (!IsCrouching) return;
        IsCrouching = false;
        _collider.size = _standingColliderSize;
    }

    /// <summary>
    /// Vira o corpo 180 graus.
    /// </summary>
    public void Flip()
    {
        isFacingRight = !isFacingRight;
        currentFacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }
    #endregion

    #region PUBLIC API (QUERIES)
    /// <summary>
    /// Verifica se a IA está a tocar no chão.
    /// </summary>
    public bool IsGrounded() => groundCheck != null && Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    #endregion
}