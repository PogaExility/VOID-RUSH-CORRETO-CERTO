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
    public float jumpForce = 10f;
    public float gravityScaleOnFall = 2.5f;
    public float baseGravity = 1f;
    public float coyoteTime = 0.1f;
    [Header("Parede")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    public float wallCheckDistance = 0.1f;

    // --- Variáveis de Estado Internas ---
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

    // NOVO ESTADO: Controla se o jogador está carregando a velocidade do dash no ar.
    private bool isCarryingDashMomentum = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        isJumping = rb.linearVelocity.y > 0.1f && !isGrounded;

        // O input do jogador é zerado se qualquer estado de "travamento" estiver ativo.
        if (isDashing || isWallJumping || isWallSliding || isCarryingDashMomentum)
        {
            moveInput = 0;
        }
        else
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }

        if (!isWallSliding && !isDashing && !isWallJumping) HandleFlipLogic();
        UpdateTimers();
        UpdateDebugUI();
    }

    void FixedUpdate()
    {
        if (isDashing || isWallJumping) return;
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

        if (isGrounded)
        {
            isWallJumping = false;
            coyoteTimeCounter = coyoteTime;
            // Ao tocar o chão, o momento do dash é perdido.
            isCarryingDashMomentum = false;
        }

        float wallRayStartOffset = capsuleCollider.size.x * 0.5f;
        isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer);
        isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer);
    }

    private void HandleWallSlideLogic()
    {
        if (isWallSliding && (!IsTouchingWall() || isGrounded))
        {
            isWallSliding = false;
        }
    }

    private void HandleMovement()
    {
        // Se o jogador estiver carregando o momento do dash, nenhuma força horizontal
        // de aceleração ou desaceleração é aplicada. A velocidade X é mantida.
        if (isCarryingDashMomentum)
        {
            return;
        }

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            return;
        }

        bool isPushingAgainstWall = !isGrounded && ((moveInput > 0 && isTouchingWallRight) || (moveInput < 0 && isTouchingWallLeft));
        if (isPushingAgainstWall)
        {
            return;
        }

        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        if (isGrounded)
        {
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            rb.AddForce(speedDiff * accelRate * Vector2.right);
        }
        else
        {
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                rb.AddForce(speedDiff * acceleration * Vector2.right);
            }
        }
    }

    private void HandleGravity()
    {
        rb.gravityScale = isWallSliding ? 0 : (rb.linearVelocity.y < 0 ? gravityScaleOnFall : baseGravity);
    }

    private void UpdateTimers() { if (!isGrounded) coyoteTimeCounter -= Time.deltaTime; }

    private void HandleFlipLogic()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if ((mouseWorldPosition.x > transform.position.x && !isFacingRight) || (mouseWorldPosition.x < transform.position.x && isFacingRight)) Flip();
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public void StartWallSlide()
    {
        isWallSliding = true;
        isCarryingDashMomentum = false; // Garante que agarrar na parede cancele o momento
        rb.linearVelocity = new Vector2(0, 0);
        if ((isTouchingWallRight && isFacingRight) || (isTouchingWallLeft && !isFacingRight))
        {
            Flip();
        }
    }

    public void DoJump(float multiplier)
    {
        isCarryingDashMomentum = false; // Pular cancela o momento
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);
    }

    public void DoWallJump(float multiplier)
    {
        isCarryingDashMomentum = false; // Pular da parede cancela o momento
        isWallSliding = false;
        isWallJumping = true;
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * wallJumpForce.x, wallJumpForce.y * multiplier);
        Flip();
    }

    public void CutJump()
    {
        // Cortar o pulo não deve cancelar o momento do dash
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    // --- Funções Públicas ---

    // NOVA FUNÇÃO: Chamada pelo SkillRelease para iniciar o estado de momento.
    public void StartMomentumCarry()
    {
        isCarryingDashMomentum = true;
    }

    public Vector2 GetWallEjectDirection() => isTouchingWallLeft ? Vector2.right : Vector2.left;
    public bool IsGrounded() => isGrounded;
    public bool IsWallSliding() => isWallSliding;
    public float GetVerticalVelocity() => rb.linearVelocity.y;
    public bool IsMoving() => Mathf.Abs(moveInput) > 0.1f;
    public Vector2 GetDashDirection() => Mathf.Abs(moveInput) > 0.01f ? (moveInput > 0 ? Vector2.right : Vector2.left) : GetFacingDirection();
    public Vector2 GetFacingDirection() => isFacingRight ? Vector2.right : Vector2.left;
    public void SetGravityScale(float scale) => rb.gravityScale = scale;
    public void SetVelocity(float x, float y) => rb.linearVelocity = new Vector2(x, y);
    public bool CanJumpFromGround() => coyoteTimeCounter > 0f;
    public bool IsTouchingWall() => isTouchingWallLeft || isTouchingWallRight;
    public void OnDashStart() => isDashing = true;
    public void OnDashEnd() => isDashing = false;
    public bool IsDashing() => isDashing;
    public void OnWallJumpEnd() => isWallJumping = false;
    public bool IsWallJumping() => isWallJumping;
    public bool IsJumping() => isJumping;
    public bool IsFacingRight() => isFacingRight;
    public Rigidbody2D GetRigidbody() => rb;

    private void UpdateDebugUI()
    {
        if (groundCheckStatusText != null)
        {
            groundCheckStatusText.text = $"Grounded: {isGrounded}\nWallSliding: {isWallSliding}\nIsJumping: {isJumping}\nDashMomentum: {isCarryingDashMomentum}";
        }
    }
}