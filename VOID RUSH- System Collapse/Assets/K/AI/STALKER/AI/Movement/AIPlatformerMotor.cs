using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    private Rigidbody2D _rb;
    private CapsuleCollider2D _collider;
    private float _originalGravity;

    [Header("▶ Referências Visuais")]
    public Transform bodyTransform;

    [Header("▶ Configuração Física")]
    public float maxSpeed = 5f;
    public float climbSpeed = 4f;
    public float acceleration = 20f;
    public float jumpForce = 14f;
    public float stopDistance = 0.1f;
    public float crouchHeight = 1.9f;

    [Tooltip("Força vertical extra para entrar no duto.")]
    public float ventEntryLiftForce = 2.0f;

    [Header("▶ Sensores")]
    public LayerMask groundLayer;
    public float groundCheckDist = 0.1f;

    private Vector2 _standSize;
    private Vector2 _standOffset;
    private Vector3 _standBodyPos;
    private Vector2 _crouchSize;
    private Vector2 _crouchOffset;

    public bool IsCrouching { get; private set; }
    public bool IsClimbing { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsBusy { get; private set; }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CapsuleCollider2D>();
        _originalGravity = _rb.gravityScale;

        _standSize = _collider.size;
        _standOffset = _collider.offset;
        if (bodyTransform != null) _standBodyPos = bodyTransform.localPosition;

        _crouchSize = new Vector2(_standSize.x, crouchHeight);
        float diff = _standSize.y - crouchHeight;
        _crouchOffset = new Vector2(_standOffset.x, _standOffset.y - (diff / 2));
    }

    void FixedUpdate()
    {
        CheckGround();

        if (IsClimbing)
        {
            _rb.gravityScale = 0;

            if (IsCrouching)
            {
                // Modo Híbrido (Duto): Força Y para entrar
                float curX = _rb.linearVelocity.x;
                _rb.linearVelocity = new Vector2(curX, ventEntryLiftForce);
            }
            else
            {
                _rb.linearVelocity = new Vector2(0, climbSpeed);
            }
        }
        else
        {
            _rb.gravityScale = _originalGravity;
        }
    }

    public void MoveTowards(float targetX)
    {
        float deltaX = targetX - transform.position.x;
        if (Mathf.Abs(deltaX) < stopDistance) { StopMoving(); return; }

        float dir = Mathf.Sign(deltaX);
        if (dir > 0 && transform.localScale.x < 0) Flip();
        else if (dir < 0 && transform.localScale.x > 0) Flip();

        float targetSpeed = dir * maxSpeed;
        if (IsCrouching) targetSpeed *= 0.5f;

        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        float newY = _rb.linearVelocity.y;

        if (IsClimbing && IsCrouching) newY = ventEntryLiftForce;

        _rb.linearVelocity = new Vector2(newX, newY);
    }

    public void StopMoving()
    {
        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, 0, acceleration * Time.fixedDeltaTime);
        float newY = _rb.linearVelocity.y;
        if (IsClimbing && IsCrouching) newY = ventEntryLiftForce;
        else if (IsClimbing) newY = 0;
        _rb.linearVelocity = new Vector2(newX, newY);
    }

    public void Jump()
    {
        if (IsClimbing || !IsGrounded || IsCrouching) return;
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void StartClimb()
    {
        if (IsClimbing) return;
        IsClimbing = true;
        if (IsCrouching) StopCrouch();
        _rb.linearVelocity = Vector2.zero;
    }

    public void StopClimb()
    {
        if (!IsClimbing) return;
        IsClimbing = false;
        _rb.gravityScale = _originalGravity;
    }

    public void StartCrouch()
    {
        if (IsCrouching) return;
        IsCrouching = true;
        _collider.size = _crouchSize;
        _collider.offset = _crouchOffset;
        if (bodyTransform != null)
        {
            float diff = _standSize.y - crouchHeight;
            bodyTransform.localPosition = new Vector3(_standBodyPos.x, _standBodyPos.y - (diff / 2), _standBodyPos.z);
        }
    }

    public void StopCrouch()
    {
        if (!IsCrouching) return;
        IsCrouching = false;
        _collider.size = _standSize;
        _collider.offset = _standOffset;
        if (bodyTransform != null) bodyTransform.localPosition = _standBodyPos;
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void CheckGround()
    {
        Vector2 center = (Vector2)transform.position + _collider.offset;
        Vector2 size = new Vector2(_collider.size.x * 0.9f, 0.05f);
        float dist = (_collider.size.y / 2f) + groundCheckDist;
        IsGrounded = Physics2D.BoxCast(center, size, 0, Vector2.down, dist, groundLayer);
    }
}