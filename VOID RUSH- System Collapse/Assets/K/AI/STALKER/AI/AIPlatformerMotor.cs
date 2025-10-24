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
    private Vector3 _standingBodyLocalPosition;
    private Vector2 _crouchingColliderSize;
    private Vector2 _crouchingColliderOffset;
    public bool IsCrouching { get; private set; }
    public bool IsClimbing { get; private set; }
    public bool IsTransitioningState { get; private set; } = false;
    public float StandingHeight { get; private set; }
    #endregion

    #region CONFIGURATION
    [Header("▶ Referências de Componentes")]
    public Transform bodyTransform;
    [Header("▶ Atributos de Movimento")]
    public float acceleration = 5f;
    public float deceleration = 8f;
    public float climbSpeed = 4f;
    [Header("▶ Atributos de Agachar")]
    public float crouchHeight = 1.9f;
    public float standUpImmunityDuration = 0.2f;
    [Header("▶ Atributos de Escalar")]
    public float vaultHeight = 1.2f;
    public float vaultForwardDistance = 1.0f;
    public float vaultDuration = 0.5f;
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
        if (bodyTransform == null) { Debug.LogError("ERRO CRÍTICO: O 'bodyTransform' não foi atribuído no Inspector!", this); this.enabled = false; return; }

        isFacingRight = transform.localScale.x > 0;
        currentFacingDirection = isFacingRight ? 1 : -1;
        _originalGravityScale = _rb.gravityScale;

        _standingColliderSize = _collider.size;
        _standingColliderOffset = _collider.offset;
        StandingHeight = _standingColliderSize.y;
        _standingBodyLocalPosition = bodyTransform.localPosition;

        _crouchingColliderSize = new Vector2(_standingColliderSize.x, crouchHeight);
        float heightDifference = _standingColliderSize.y - crouchHeight;
        _crouchingColliderOffset = new Vector2(_standingColliderOffset.x, _standingColliderOffset.y - (heightDifference / 2));
    }

    void FixedUpdate()
    {
        if (!IsClimbing && !IsTransitioningState) { _rb.linearVelocity = new Vector2(_currentSpeed * currentFacingDirection, _rb.linearVelocity.y); }
    }
    #endregion

    #region PUBLIC API (COMMANDS)
    public void Move(float topSpeed) { _currentSpeed = Mathf.MoveTowards(_currentSpeed, topSpeed, acceleration * Time.deltaTime); }
    public void Stop() { _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime); }
    public void HardStop() { _currentSpeed = 0; _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y); }
    public void Brake() { _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime); }
    public void Jump(float jumpForce) { if (IsGrounded()) { _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse); } }

    public void StartVault()
    {
        if (IsTransitioningState) return;
        StartCoroutine(VaultCoroutine());
    }

    private IEnumerator VaultCoroutine()
    {
        IsTransitioningState = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.gravityScale = 0;

        Vector2 startPos = transform.position;
        Vector2 endPos = new Vector2(
            transform.position.x + (vaultForwardDistance * currentFacingDirection),
            transform.position.y + vaultHeight
        );

        float elapsedTime = 0f;
        while (elapsedTime < vaultDuration)
        {
            transform.position = Vector2.Lerp(startPos, endPos, (elapsedTime / vaultDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        _rb.gravityScale = _originalGravityScale;
        IsTransitioningState = false;
    }

    public void StartCrouch()
    {
        if (IsCrouching) return;
        IsCrouching = true;
        _collider.size = _crouchingColliderSize;
        _collider.offset = _crouchingColliderOffset;
        float heightDifference = StandingHeight - crouchHeight;
        bodyTransform.localPosition = new Vector3(_standingBodyLocalPosition.x, _standingBodyLocalPosition.y - (heightDifference / 2), _standingBodyLocalPosition.z);
    }

    public void StopCrouch()
    {
        if (!IsCrouching) return;
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

    #region PUBLIC API (QUERIES)
    public bool IsGrounded() => groundCheck != null && Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    #endregion
}