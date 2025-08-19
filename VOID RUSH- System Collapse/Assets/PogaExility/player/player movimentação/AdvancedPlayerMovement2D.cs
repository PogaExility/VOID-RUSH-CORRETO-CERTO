using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{
    [Header("Referências")] public PlayerAnimatorController animatorController; public Camera mainCamera; public TextMeshProUGUI groundCheckStatusText; public LayerMask collisionLayer;
    [Header("Movimento")] public float moveSpeed = 8f; public float acceleration = 50f; public float deceleration = 60f;
    [Header("Pulo")] public float jumpForce = 10f; public float gravityScaleOnFall = 2.5f; public float baseGravity = 1f; public float coyoteTime = 0.1f;
    [Header("Parede")] public float wallSlideSpeed = 2f; public Vector2 wallJumpForce = new Vector2(10f, 12f); public float wallCheckDistance = 0.1f;

    [Header("Física do WallDashJump")]
    [Tooltip("O atrito do ar (linear damping) que causa a perda gradual de velocidade.")]
    public float parabolaLinearDamping = 0.3f;
    [Tooltip("A velocidade horizontal MÁXIMA que o jogador pode atingir com o controle aéreo.")]
    public float parabolaMaxAirSpeed = 20f;
    [Tooltip("A força do controle aéreo (steering).")]
    public float parabolaSteeringForce = 100f;

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
    private bool isInParabolaArc = false;
    private bool isJumping = false;
    private bool isLanding = false;

    void Awake() { rb = GetComponent<Rigidbody2D>(); capsuleCollider = GetComponent<CapsuleCollider2D>(); if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>(); }

    // Corrige o bug do flip no ar
    void Update() { isJumping = rb.linearVelocity.y > 0.1f && !isGrounded; if (!isLanding) { moveInput = Input.GetAxisRaw("Horizontal"); } else { moveInput = 0; } if (!isWallSliding && !isWallJumping && !isInParabolaArc) { HandleFlipLogic(); } UpdateTimers(); UpdateDebugUI(); }

    void FixedUpdate() { CheckCollisions(); if (isLanding) { rb.linearVelocity = Vector2.zero; return; } if (isDashing || isWallJumping) return; HandleWallSlideLogic(); HandleMovement(); HandleGravity(); }

    public void OnLandingStart() { isLanding = true; }
    public void OnLandingComplete() { isLanding = false; }
    public void StopWallSlide() { isWallSliding = false; }

    public void DoWallDashJump(float horizontalForce, float verticalForce)
    {
        isWallSliding = false;
        isInParabolaArc = true;
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(ejectDirection.x * horizontalForce, verticalForce);
        if ((ejectDirection.x > 0 && !isFacingRight) || (ejectDirection.x < 0 && isFacingRight)) { Flip(); }
        rb.linearDamping = parabolaLinearDamping;
    }

    private void CheckCollisions() { Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset; Vector2 capsuleSize = capsuleCollider.size; RaycastHit2D hit = Physics2D.CapsuleCast(capsuleCenter, capsuleSize, capsuleCollider.direction, 0f, Vector2.down, 0.1f, collisionLayer); isGrounded = hit.collider != null && Vector2.Angle(hit.normal, Vector2.up) < 45f; if (isGrounded) { if (isInParabolaArc || isWallJumping) rb.linearDamping = 0f; isWallJumping = false; isInParabolaArc = false; coyoteTimeCounter = coyoteTime; isJumping = false; } float wallRayStartOffset = capsuleCollider.size.x * 0.5f; isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer); isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer); }
    public void DoJump(float multiplier) { isJumping = true; rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier); }
    public void DoWallJump(float multiplier) { isJumping = true; isWallSliding = false; isWallJumping = true; Vector2 ejectDirection = GetWallEjectDirection(); rb.linearVelocity = new Vector2(ejectDirection.x * wallJumpForce.x, wallJumpForce.y * multiplier); Flip(); }
    public void Flip() { isFacingRight = !isFacingRight; transform.Rotate(0f, 180f, 0f); }
    private void HandleFlipLogic() { if (moveInput > 0.01f && !isFacingRight) Flip(); else if (moveInput < -0.01f && isFacingRight) Flip(); }
    public void StartWallSlide() { isWallSliding = true; if ((isFacingRight && isTouchingWallRight) || (!isFacingRight && isTouchingWallLeft)) { Flip(); } }

    // ===== FÍSICA FINAL E CORRETA =====
    private void HandleMovement()
    {
        if (isInParabolaArc)
        {
            // Se o jogador estiver tentando acelerar na direção em que já está se movendo,
            // e já estiver acima da velocidade máxima, não faz nada.
            if ((moveInput > 0 && rb.linearVelocity.x >= parabolaMaxAirSpeed) ||
                (moveInput < 0 && rb.linearVelocity.x <= -parabolaMaxAirSpeed))
            {
                return;
            }

            // Caso contrário, aplica a força de controle aéreo.
            rb.AddForce(Vector2.right * moveInput * parabolaSteeringForce);
        }
        else
        {
            // Lógica de movimento original, com a proteção restaurada para não travar
            if (isWallSliding) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed); return; }

            bool isPushingAgainstWall = !isGrounded && ((moveInput > 0 && isTouchingWallRight) || (moveInput < 0 && isTouchingWallLeft));
            if (isPushingAgainstWall) return;

            float targetSpeed = moveInput * moveSpeed;
            float speedDiff = targetSpeed - rb.linearVelocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            rb.AddForce(speedDiff * accelRate * Vector2.right);
        }
    }

    private void HandleWallSlideLogic() { if (isWallSliding && (!IsTouchingWall() || isGrounded)) { isWallSliding = false; } }
    private void HandleGravity() { rb.gravityScale = isWallSliding ? 0 : (rb.linearVelocity.y < 0 ? gravityScaleOnFall : baseGravity); }
    private void UpdateTimers() { if (!isGrounded) coyoteTimeCounter -= Time.deltaTime; }
    public void CutJump() { if (rb.linearVelocity.y > 0) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); } }
    public Vector2 GetWallEjectDirection() { return isTouchingWallLeft ? Vector2.right : Vector2.left; }
    public bool IsGrounded() { return isGrounded; }
    public bool IsWallSliding() { return isWallSliding; }
    public bool IsInParabolaArc() { return isInParabolaArc; }
    public float GetVerticalVelocity() { return rb.linearVelocity.y; }
    public bool IsMoving() { return Mathf.Abs(moveInput) > 0.1f; }
    public Vector2 GetDashDirection() { return Mathf.Abs(moveInput) > 0.01f ? (moveInput > 0 ? Vector2.right : Vector2.left) : GetFacingDirection(); }
    public Vector2 GetFacingDirection() { return isFacingRight ? Vector2.right : Vector2.left; }
    public void SetGravityScale(float scale) { rb.gravityScale = scale; }
    public void SetVelocity(float x, float y) { rb.linearVelocity = new Vector2(x, y); }
    public bool CanJumpFromGround() { return coyoteTimeCounter > 0f; }
    public bool IsTouchingWall() { return isTouchingWallLeft || isTouchingWallRight; }
    public void OnDashStart() { isDashing = true; }
    public void OnDashEnd() { isDashing = false; }
    public bool IsDashing() { return isDashing; }
    public void OnWallJumpEnd() { isWallJumping = false; rb.linearDamping = 0f; }
    public bool IsWallJumping() { return isWallJumping; }
    public bool IsJumping() { return isJumping; }
    public bool IsFacingRight() { return isFacingRight; }
    public Rigidbody2D GetRigidbody() { return rb; }
    private void UpdateDebugUI() { if (groundCheckStatusText != null) { groundCheckStatusText.text = $"Grounded: {isGrounded}\nWallSliding: {isWallSliding}\nIsJumping: {isJumping}\nDashing: {isDashing}"; } }
}