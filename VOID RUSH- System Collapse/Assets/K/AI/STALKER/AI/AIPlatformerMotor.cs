using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    private Rigidbody2D _rb;
    private CapsuleCollider2D _collider;

    [HideInInspector] public float currentFacingDirection = 1f;
    [HideInInspector] public bool isFacingRight = true;
    private float _currentSpeed = 0f;

    [Header("▶ Atributos de Movimento")]
    [Tooltip("A velocidade com que a IA ganha velocidade.")]
    public float acceleration = 2.5f;
    [Tooltip("A velocidade com que a IA perde velocidade ao parar.")]
    public float deceleration = 4f;

    [Header("▶ Atributos Físicos")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckDistance = 0.1f;

    // Variáveis para o agachamento
    private Vector2 _standingColliderSize;
    private Vector2 _standingColliderOffset;
    private Vector2 _crouchingColliderSize;
    private Vector2 _crouchingColliderOffset;
    public bool IsCrouching { get; private set; }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CapsuleCollider2D>();

        isFacingRight = transform.localScale.x > 0;
        currentFacingDirection = isFacingRight ? 1 : -1;

        // Guarda as dimensões originais do colisor
        _standingColliderSize = _collider.size;
        _standingColliderOffset = _collider.offset;

        // Define as dimensões do colisor ao agachar-se (squash and stretch)
        _crouchingColliderSize = new Vector2(_standingColliderSize.x * 1.5f, _standingColliderSize.y * 0.6f); // Mais largo, mais baixo
        _crouchingColliderOffset = new Vector2(_standingColliderOffset.x, -0.45f); // Ajusta o offset para baixo
    }

    void FixedUpdate()
    {
        // Aplica a velocidade atual ao Rigidbody. Isto deve estar no FixedUpdate.
        _rb.linearVelocity = new Vector2(_currentSpeed * currentFacingDirection, _rb.linearVelocity.y);
    }

    // A nova função de movimento que usa aceleração
    public void Move(float targetSpeed)
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);
    }

    public void Stop()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime);
    }

    public void Jump(float jumpForce)
    {
        if (IsGrounded())
        {
            _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void StartCrouch()
    {
        if (IsCrouching) return;
        IsCrouching = true;
        _collider.size = _crouchingColliderSize;
        _collider.offset = _crouchingColliderOffset;
    }

    public void StopCrouch()
    {
        if (!IsCrouching) return;
        IsCrouching = false;
        _collider.size = _standingColliderSize;
        _collider.offset = _standingColliderOffset;
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        currentFacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    }
}