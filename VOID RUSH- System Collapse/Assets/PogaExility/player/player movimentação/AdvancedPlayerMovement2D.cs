using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{
    [Header("Referências")] public PlayerAnimatorController animatorController; public Camera mainCamera; public TextMeshProUGUI groundCheckStatusText; public LayerMask collisionLayer;
    [Header("Movimento")] public float moveSpeed = 8f; public float acceleration = 50f; public float deceleration = 60f;
    [Header("Pulo")] public float jumpForce = 10f; public float gravityScaleOnFall = 2.5f; public float baseGravity = 1f; public float coyoteTime = 0.1f;
    [Header("Parede")] public float wallSlideSpeed = 2f; public Vector2 wallJumpForce = new Vector2(10f, 12f); public float wallCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private float moveInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private bool isTouchingWallRight;
    private bool isTouchingWallLeft;
    private bool isWallSliding;
    private bool isDashing = false;
    private bool isWallJumping = false;
    private bool isJumping = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
    }

    void Update()
    {
        isJumping = rb.linearVelocity.y > 0.1f && !isGrounded;
        moveInput = Input.GetAxisRaw("Horizontal");

        // CORREÇÃO: A lógica de virar agora só acontece se não estivermos fazendo uma ação na parede.
        if (!isWallSliding && !isWallJumping)
        {
            HandleFlipLogic();
        }

        UpdateTimers();
        UpdateDebugUI();
    }

    void FixedUpdate()
    {
        CheckCollisions();
        if (isDashing || isWallJumping) return;
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

        if (isGrounded)
        {
            isWallJumping = false;
            coyoteTimeCounter = coyoteTime;
            isJumping = false;
        }

        float wallRayStartOffset = capsuleCollider.size.x * 0.5f;
        isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer);
        isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer);
    }

    public void DoJump(float multiplier)
    {
        isJumping = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);
    }

    public void DoWallJump(float multiplier)
    {
        isJumping = true;
        isWallSliding = false;
        isWallJumping = true;
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * wallJumpForce.x, wallJumpForce.y * multiplier);

        // CORREÇÃO: Força o flip ao pular da parede.
        Flip();
    }

    public void Flip() { isFacingRight = !isFacingRight; transform.Rotate(0f, 180f, 0f); }
    private void HandleFlipLogic() { if (moveInput > 0.01f && !isFacingRight) Flip(); else if (moveInput < -0.01f && isFacingRight) Flip(); }

    public void StartWallSlide()
    {
        isWallSliding = true;

        // CORREÇÃO: Esta lógica garante que o jogador SEMPRE vire para longe da parede ao começar a deslizar.
        if ((isFacingRight && isTouchingWallRight) || (!isFacingRight && isTouchingWallLeft))
        {
            Flip();
        }
    }

    private void HandleMovement() { if (isWallSliding) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed); return; } bool isPushingAgainstWall = !isGrounded && ((moveInput > 0 && isTouchingWallRight) || (moveInput < 0 && isTouchingWallLeft)); if (isPushingAgainstWall) return; float targetSpeed = moveInput * moveSpeed; float speedDiff = targetSpeed - rb.linearVelocity.x; float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration; rb.AddForce(speedDiff * accelRate * Vector2.right); }
    private void HandleWallSlideLogic() { if (isWallSliding && (!IsTouchingWall() || isGrounded)) { isWallSliding = false; } }
    private void HandleGravity() { rb.gravityScale = isWallSliding ? 0 : (rb.linearVelocity.y < 0 ? gravityScaleOnFall : baseGravity); }
    private void UpdateTimers() { if (!isGrounded) coyoteTimeCounter -= Time.deltaTime; }
    public void CutJump() { if (rb.linearVelocity.y > 0) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); } }
    public Vector2 GetWallEjectDirection() => isTouchingWallLeft ? Vector2.right : Vector2.left;
    public bool IsGrounded() => isGrounded; public bool IsWallSliding() => isWallSliding; public float GetVerticalVelocity() => rb.linearVelocity.y; public bool IsMoving() => Mathf.Abs(moveInput) > 0.1f; public Vector2 GetDashDirection() => Mathf.Abs(moveInput) > 0.01f ? (moveInput > 0 ? Vector2.right : Vector2.left) : GetFacingDirection(); public Vector2 GetFacingDirection() => isFacingRight ? Vector2.right : Vector2.left; public void SetGravityScale(float scale) => rb.gravityScale = scale; public void SetVelocity(float x, float y) => rb.linearVelocity = new Vector2(x, y); public bool CanJumpFromGround() => coyoteTimeCounter > 0f; public bool IsTouchingWall() => isTouchingWallLeft || isTouchingWallRight; public void OnDashStart() => isDashing = true; public void OnDashEnd() => isDashing = false; public bool IsDashing() => isDashing; public void OnWallJumpEnd() => isWallJumping = false; public bool IsWallJumping() => isWallJumping; public bool IsJumping() => isJumping; public bool IsFacingRight() => isFacingRight; public Rigidbody2D GetRigidbody() => rb;
    private void UpdateDebugUI() { if (groundCheckStatusText != null) { groundCheckStatusText.text = $"Grounded: {isGrounded}\nWallSliding: {isWallSliding}\nIsJumping: {isJumping}\nDashing: {isDashing}"; } }
}