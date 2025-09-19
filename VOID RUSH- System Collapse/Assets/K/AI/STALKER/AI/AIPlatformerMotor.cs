using UnityEngine;
using System.Collections;

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
    private Vector2 _standingColliderOffset;
    private Vector2 _crouchingColliderSize;
    private Vector2 _crouchingColliderOffset;

    private Vector3 _standingBodyLocalPosition;

    public bool IsCrouching { get; private set; }
    public bool IsClimbing { get; private set; }

    public bool IsTransitioningState { get; private set; } = false;

    // --- ALTERAÇÃO: Propriedades públicas para que outros scripts possam ler as alturas com segurança ---
    public float StandingHeight { get; private set; }
    public float CrouchHeight_Inspector { get { return crouchHeight; } } // Apenas para expor o valor do Inspector

    #endregion

    #region CONFIGURATION
    [Header("▶ Referências de Componentes")]
    [Tooltip("Arraste aqui o GameObject filho 'Body' que contém os sprites e o DetectionRig.")]
    public Transform bodyTransform;

    [Header("▶ Atributos de Movimento")]
    public float acceleration = 5f;
    public float deceleration = 8f;
    public float climbSpeed = 4f;

    [Header("▶ Atributos de Agachar")]
    [Tooltip("A altura exata do colisor quando o Stalker está agachado.")]
    public float crouchHeight = 1.9f;
    [Tooltip("Duração em segundos da imunidade a decisões após se levantar.")]
    public float standUpImmunityDuration = 0.2f; // Período de carência


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

        if (bodyTransform == null)
        {
            Debug.LogError("ERRO CRÍTICO: O 'bodyTransform' não foi atribuído no Inspector do AIPlatformerMotor!", this);
            this.enabled = false;
            return;
        }

        isFacingRight = transform.localScale.x > 0;
        currentFacingDirection = isFacingRight ? 1 : -1;
        _originalGravityScale = _rb.gravityScale;

        _standingColliderSize = _collider.size;
        _standingColliderOffset = _collider.offset;

        // --- ALTERAÇÃO: Armazena a altura de pé na nova propriedade pública ---
        StandingHeight = _standingColliderSize.y;

        _standingBodyLocalPosition = bodyTransform.localPosition;

        _crouchingColliderSize = new Vector2(_standingColliderSize.x, crouchHeight);
        float heightDifference = _standingColliderSize.y - crouchHeight;
        _crouchingColliderOffset = new Vector2(_standingColliderOffset.x, _standingColliderOffset.y - (heightDifference / 2));
    }

    void FixedUpdate()
    {
        if (!IsClimbing) { _rb.linearVelocity = new Vector2(_currentSpeed * currentFacingDirection, _rb.linearVelocity.y); }
    }
    #endregion

    #region PUBLIC API (COMMANDS)
    public void Move(float topSpeed) { _currentSpeed = Mathf.MoveTowards(_currentSpeed, topSpeed, acceleration * Time.deltaTime); }
    public void Stop() { _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime); }
    public void HardStop() { _currentSpeed = 0; _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y); }
    public void Brake() { _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime); }
    public void Jump(float jumpForce) { if (IsGrounded()) { _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse); } }
    public void StartClimb() { if (IsClimbing) return; IsClimbing = true; _rb.gravityScale = 0; _currentSpeed = 0; _rb.linearVelocity = Vector2.zero; }
    public void Climb(float verticalDirection) { if (!IsClimbing) return; _rb.linearVelocity = new Vector2(0, verticalDirection * climbSpeed); }
    public void StopClimb() { if (!IsClimbing) return; IsClimbing = false; _rb.gravityScale = _originalGravityScale; }

    public void StartCrouch()
    {
        if (IsCrouching) return;
        IsCrouching = true;
        _collider.size = _crouchingColliderSize;
        _collider.offset = _crouchingColliderOffset;

        float heightDifference = StandingHeight - crouchHeight;
        bodyTransform.localPosition = new Vector3(
            _standingBodyLocalPosition.x,
            _standingBodyLocalPosition.y - (heightDifference / 2),
            _standingBodyLocalPosition.z
        );
    }
    public void StopCrouch()
    {
        if (!IsCrouching) return;
        IsCrouching = false;
        _collider.size = _standingColliderSize;
        _collider.offset = _standingColliderOffset;
        bodyTransform.localPosition = _standingBodyLocalPosition;

        // --- ALTERAÇÃO: Inicia a corrotina de imunidade ---
        StartCoroutine(StateTransitionCooldownRoutine());
    }

    // --- ALTERAÇÃO: Nova corrotina para o período de carência ---
    private IEnumerator StateTransitionCooldownRoutine()
    {
        IsTransitioningState = true;
        yield return new WaitForSeconds(standUpImmunityDuration);
        IsTransitioningState = false;
    }


    public void Flip() { isFacingRight = !isFacingRight; currentFacingDirection *= -1; transform.Rotate(0f, 180f, 0f); }
    #endregion

    #region PUBLIC API (QUERIES)
    public bool IsGrounded() => groundCheck != null && Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    #endregion
}