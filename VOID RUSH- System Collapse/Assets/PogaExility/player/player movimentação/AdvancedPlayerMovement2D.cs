using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{
    [Header("Movimento Base")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 60f;
    public float baseGravity = 1f;

    [Header("Detecção de Colisão")]
    public LayerMask collisionLayer;
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private float moveInput;
    private bool isFacingRight = true;
    private float coyoteTimeCounter;

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public bool IsWallSliding { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsInParabola { get; private set; }
    public bool IsFacingRight => isFacingRight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        if (!IsDashing && !IsInParabola)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            HandleFlipLogic();
        }

        var jumpSkill = GetComponent<PlayerController>().baseJumpSkill;
        if (IsGrounded)
        {
            if (jumpSkill != null) coyoteTimeCounter = jumpSkill.coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        CheckCollisions();
        if (IsDashing || IsInParabola) return;
        HandleHorizontalMovement();
        HandleGravity();
    }

    public void DoJump(float force)
    {
        coyoteTimeCounter = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    public void DoWallJump(Vector2 force)
    {
        StopWallSlide();
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * force.x, force.y);
        Flip();
    }

    public void DoWallDashJump(float hForce, float vForce, float damping)
    {
        StopWallSlide();
        OnParabolaStart(damping);
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = new Vector2(hForce * ejectDirection.x, vForce);
        Flip();
    }

    public void StartWallSlide(float slideSpeed)
    {
        if (IsGrounded || IsDashing) return;
        IsWallSliding = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -slideSpeed);
    }

    public void StopWallSlide() { IsWallSliding = false; }

    public void OnDashStart() { IsDashing = true; }
    public void OnDashEnd() { IsDashing = false; }
    public void OnParabolaStart(float damping) { IsInParabola = true; rb.linearDamping = damping; }
    public void OnParabolaEnd() { IsInParabola = false; rb.linearDamping = 0f; }

    public void SetVelocity(float x, float y) { rb.linearVelocity = new Vector2(x, y); }
    public void SetGravityScale(float scale) { rb.gravityScale = scale; }

    private void HandleHorizontalMovement()
    {
        if (IsWallSliding) return;
        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(speedDiff * accelRate * Vector2.right);
    }

    private void HandleGravity()
    {
        if (IsWallSliding) rb.gravityScale = 0;
        else
        {
            var jumpSkill = GetComponent<PlayerController>().baseJumpSkill;
            float fallMultiplier = (jumpSkill != null) ? jumpSkill.gravityScaleOnFall : 2.5f;
            rb.gravityScale = rb.linearVelocity.y < 0 ? baseGravity * fallMultiplier : baseGravity;
        }
    }

    private void HandleFlipLogic()
    {
        if (moveInput > 0.01f && !isFacingRight) Flip();
        else if (moveInput < -0.01f && isFacingRight) Flip();
    }

    // --- AQUI ESTÁ A CORREÇÃO ---
    // A função agora é 'public' para que o SkillRelease possa chamá-la.
    public void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void CheckCollisions()
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        RaycastHit2D groundHit = Physics2D.BoxCast(capsuleCenter, capsuleCollider.size, 0, Vector2.down, groundCheckDistance, collisionLayer);
        IsGrounded = groundHit.collider != null;

        if (IsGrounded && (IsInParabola || IsDashing))
        {
            OnParabolaEnd();
            OnDashEnd();
        }

        bool isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, capsuleCollider.size.x * 0.5f + wallCheckDistance, collisionLayer);
        bool isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, capsuleCollider.size.x * 0.5f + wallCheckDistance, collisionLayer);
        IsTouchingWall = isTouchingWallRight || isTouchingWallLeft;

        if (IsWallSliding && !IsTouchingWall) StopWallSlide();
    }

    public bool CanJumpFromGround() { return coyoteTimeCounter > 0f; }
    public Vector2 GetFacingDirection() { return isFacingRight ? Vector2.right : Vector2.left; }
    public Vector2 GetWallEjectDirection() { return IsTouchingWallOnLeft() ? Vector2.right : Vector2.left; }
    public float GetVerticalVelocity() { return rb.linearVelocity.y; }
    public Rigidbody2D GetRigidbody() { return rb; }

    private bool IsTouchingWallOnLeft()
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        return Physics2D.Raycast(capsuleCenter, Vector2.left, capsuleCollider.size.x * 0.5f + wallCheckDistance, collisionLayer);
    }
}