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
    private float _currentSpeed = 0f;
    private float _originalGravityScale;
    private Vector2 _standingColliderSize;
    private Vector2 _standingColliderOffset;
    private Vector3 _standingBodyLocalPosition;
    private Vector2 _crouchingColliderSize;
    private Vector2 _crouchingColliderOffset;
    public bool IsCrouching { get; private set; }
    public bool IsTransitioningState { get; private set; } = false;
    public float StandingHeight { get; private set; }
    #endregion

    #region CONFIGURATION
    [Header("▶ Referências de Componentes")]
    public Transform bodyTransform;
    [Header("▶ Atributos de Movimento")]
    public float acceleration = 5f;
    public float deceleration = 8f;
    public float jumpForce = 15f; // ADICIONADO PARA PATHFINDING
    [Header("▶ Atributos de Agachar")]
    public float crouchHeight = 1.9f;
    public float standUpImmunityDuration = 0.2f;
    [Header("▶ ATRIBUTOS DA NOVA ESCALADA TÁTICA")]
    public float climbUpSpeed = 5f;
    public float climbForwardSpeed = 5f;
    public float climbForwardDistance = 1.0f;
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
        _standingColliderOffset = _collider.offset;
        StandingHeight = _standingColliderSize.y;
        if (bodyTransform != null) _standingBodyLocalPosition = bodyTransform.localPosition;
        _crouchingColliderSize = new Vector2(_standingColliderSize.x, crouchHeight);
        float heightDifference = _standingColliderSize.y - crouchHeight;
        _crouchingColliderOffset = new Vector2(_standingColliderOffset.x, _standingColliderOffset.y - (heightDifference / 2));
    }

    void FixedUpdate()
    {
        if (!IsTransitioningState)
        {
            // Mantendo compatibilidade com Unity antigo e novo (linearVelocity vs velocity)
            _rb.linearVelocity = new Vector2(_currentSpeed * currentFacingDirection, _rb.linearVelocity.y);
        }
    }
    #endregion

    #region PUBLIC COMMANDS
    public void Move(float topSpeed) { _currentSpeed = Mathf.MoveTowards(_currentSpeed, topSpeed, acceleration * Time.deltaTime); }
    public void HardStop() { _currentSpeed = 0; _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y); }

    // --- INTEGRAÇÃO PATHFINDING ---
    public void MoveTo(Vector3 targetPos, float speed)
    {
        if (IsTransitioningState) return;

        float distX = targetPos.x - transform.position.x;
        float distY = targetPos.y - transform.position.y;
        float distTotal = Vector2.Distance(transform.position, targetPos);

        // Zona morta para evitar tremedeira (Jitter)
        if (Mathf.Abs(distX) < 0.15f) distX = 0;

        // 1. Movimento Horizontal
        if (distX > 0)
        {
            if (!isFacingRight) Flip();
            Move(speed);
        }
        else if (distX < 0)
        {
            if (isFacingRight) Flip();
            Move(speed);
        }
        else
        {
            // Só para se estiver no chão. No ar, mantém inércia.
            if (IsGrounded()) _currentSpeed = 0;
        }

        // 2. Lógica de Pulo Melhorada
        // Pula se o alvo está alto E (estamos no chão OU estamos caindo mas o alvo é próximo)
        bool needToJump = distY > 0.5f; // Alvo está acima
        bool closeEnoughToJump = Mathf.Abs(distX) < 1.5f; // Estamos perto horizontalmente

        if (needToJump && IsGrounded() && closeEnoughToJump)
        {
            // Aplica força apenas se não estiver já subindo
            if (_rb.linearVelocity.y <= 0.1f)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            }
        }
    }

    public bool IsGrounded()
    {
        // Torna público para o Controller usar
        return Physics2D.Raycast(transform.position, Vector2.down, 1.2f, groundLayer);
    }
    // ------------------------------

    public void StartVault(float vaultHeight)
    {
        if (IsTransitioningState) return;
        Vector2 targetPosition = new Vector2(transform.position.x, transform.position.y + vaultHeight);
        StartCoroutine(ClimbToPositionCoroutine(targetPosition, false));
    }

    public void ClimbToPosition(Vector2 targetPosition, bool crouchAtEnd)
    {
        if (IsTransitioningState) return;
        StartCoroutine(ClimbToPositionCoroutine(targetPosition, crouchAtEnd));
    }

    public void StartPerch(Vector2 targetPosition)
    {
        Debug.LogWarning("MOTOR: Iniciando manobra de PERCH.");
        StartCoroutine(ClimbToPositionCoroutine(targetPosition, false));
    }

    private IEnumerator ClimbToPositionCoroutine(Vector2 ledgePosition, bool crouchAtEnd)
    {
        IsTransitioningState = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.gravityScale = 0;

        Vector2 startPos = transform.position;
        Vector2 verticalTargetPos = new Vector2(transform.position.x, ledgePosition.y);
        float verticalDistance = Mathf.Abs(verticalTargetPos.y - startPos.y);
        float verticalDuration = verticalDistance / climbUpSpeed;

        float elapsedTime = 0f;
        while (elapsedTime < verticalDuration)
        {
            transform.position = Vector2.Lerp(startPos, verticalTargetPos, elapsedTime / verticalDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = verticalTargetPos;

        Vector2 forwardTargetPos = new Vector2(transform.position.x + (climbForwardDistance * currentFacingDirection), transform.position.y);
        float forwardDuration = climbForwardDistance / climbForwardSpeed;
        elapsedTime = 0f;
        while (elapsedTime < forwardDuration)
        {
            transform.position = Vector2.Lerp(verticalTargetPos, forwardTargetPos, elapsedTime / forwardDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = forwardTargetPos;

        if (crouchAtEnd) StartCrouch();
        _rb.gravityScale = _originalGravityScale;
        IsTransitioningState = false;
    }

    public void StartCrouch()
    {
        if (IsCrouching || bodyTransform == null) return;
        IsCrouching = true;
        _collider.size = _crouchingColliderSize;
        _collider.offset = _crouchingColliderOffset;
        float heightDifference = StandingHeight - crouchHeight;
        bodyTransform.localPosition = new Vector3(_standingBodyLocalPosition.x, _standingBodyLocalPosition.y - (heightDifference / 2), _standingBodyLocalPosition.z);
    }

    public void StopCrouch()
    {
        if (!IsCrouching || bodyTransform == null) return;
        IsCrouching = false;
        _collider.size = _standingColliderSize;
        _collider.offset = _standingColliderOffset;
        bodyTransform.localPosition = _standingBodyLocalPosition;
        StartCoroutine(StateTransitionCooldownRoutine());
    }

    private IEnumerator StateTransitionCooldownRoutine()
    {
        IsTransitioningState = true;
        yield return new WaitForSeconds(standUpImmunityDuration);
        IsTransitioningState = false;
    }

    public void Flip() { isFacingRight = !isFacingRight; currentFacingDirection *= -1; transform.Rotate(0f, 180f, 0f); }
    #endregion
}