using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{
    [Header("Referências")]
    public Camera mainCamera;
    public TextMeshProUGUI groundCheckStatusText;
    public LayerMask collisionLayer;

    [Header("Movimento")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 60f;

    [Header("Pulo")]
    public float jumpForce = 15f;
    public float gravityScaleOnFall = 2.5f;
    public float baseGravity = 1f;
    public float coyoteTime = 0.1f;

    [Header("Parede")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(12f, 20f);
    public float wallCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private float moveInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private bool isTouchingWallRight;
    private bool isTouchingWallLeft;
    private bool isWallSliding;
    private bool isJumping;

    // A nova variável única para bloquear controle
    private bool isInAction = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (isInAction) { moveInput = 0; }
        else { moveInput = Input.GetAxisRaw("Horizontal"); }

        isJumping = rb.linearVelocity.y > 0.1f && !isGrounded && !isWallSliding;

        if (!isWallSliding && !isInAction) HandleFlipLogic();
        UpdateTimers();
        UpdateDebugUI();
    }

    void FixedUpdate()
    {
        if (isInAction) return;
        CheckCollisions();
        HandleWallSlideLogic();
        HandleMovement();
        HandleGravity();
    }

    private void CheckCollisions()
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        Vector2 capsuleSize = capsuleCollider.size;
        RaycastHit2D hit = Physics2D.CapsuleCast(capsuleCenter, capsuleSize, capsuleCollider.direction, 0f, Vector2.down, 0.1f, collisionLayer);
        isGrounded = hit.collider != null && Vector2.Angle(hit.normal, Vector2.up) < 45f;
        if (isGrounded) coyoteTimeCounter = coyoteTime;

        float wallRayStartOffset = capsuleCollider.size.x * 0.5f;
        isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer);
        isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer);
    }

    private void HandleWallSlideLogic()
    {
        bool wantsToSlide = (isTouchingWallRight && moveInput > 0.1f) || (isTouchingWallLeft && moveInput < -0.1f);
        if (wantsToSlide && !isGrounded)
        {
            if (!isWallSliding)
            {
                isWallSliding = true;
                rb.linearVelocity = Vector2.zero;
                if ((isTouchingWallRight && isFacingRight) || (isTouchingWallLeft && !isFacingRight)) Flip();
            }
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void HandleMovement()
    {
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(0, -wallSlideSpeed);
            return;
        }
        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(speedDiff * accelRate * Vector2.right);
    }

    private void HandleGravity()
    {
        rb.gravityScale = isWallSliding ? 0 : (rb.linearVelocity.y < 0 ? gravityScaleOnFall : baseGravity);
    }

    private void UpdateTimers()
    {
        if (!isGrounded) coyoteTimeCounter -= Time.deltaTime;
    }

    private void HandleFlipLogic()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if ((mouseWorldPosition.x > transform.position.x && !isFacingRight) || (mouseWorldPosition.x < transform.position.x && isFacingRight))
        {
            Flip();
        }
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public bool IsGrounded() => isGrounded;
    public bool IsWallSliding() => isWallSliding;
    public float GetVerticalVelocity() => rb.linearVelocity.y;
    public bool IsMoving() => Mathf.Abs(moveInput) > 0.1f;
    public Vector2 GetDashDirection() => Mathf.Abs(moveInput) > 0.01f ? (moveInput > 0 ? Vector2.right : Vector2.left) : GetFacingDirection();
    public Vector2 GetFacingDirection() => isFacingRight ? Vector2.right : Vector2.left;
    public void SetGravityScale(float scale) => rb.gravityScale = scale;
    public void SetVelocity(float x, float y) => rb.linearVelocity = new Vector2(x, y);
    public bool CanJumpFromGround() => coyoteTimeCounter > 0f;

    public void DoJump(float multiplier)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);
    }

    public void DoWallJump(float multiplier)
    {
        isWallSliding = false;
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * wallJumpForce.x, wallJumpForce.y * multiplier);
        Flip();
    }

    public Vector2 GetWallEjectDirection() => isTouchingWallLeft ? Vector2.right : Vector2.left;
    public void StartAction() => isInAction = true;
    public void EndAction() => isInAction = false;
    public bool IsInAction() => isInAction;
    public bool IsJumping() => isJumping;
    public bool IsFacingRight() => isFacingRight;

    private void UpdateDebugUI()
    {
        if (groundCheckStatusText != null)
        {
            groundCheckStatusText.text = $"Grounded: {isGrounded}\nWallSliding: {isWallSliding}\nInAction: {isInAction}";
        }
    }
}