using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{
    // --- Variáveis do Inspector (sem alterações) ---
    [Header("Referências")]
    public Camera mainCamera;
    public TextMeshProUGUI groundCheckStatusText;
    [Header("Movimento Horizontal")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 60f;
    [Header("Pulo")]
    public float jumpForce = 15f;
    public float gravityScaleOnFall = 2.5f;
    public float baseGravity = 1f;
    [Header("Verificações (Checks)")]
    public LayerMask collisionLayer;
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.1f;
    public float coyoteTime = 0.1f;
    [Range(0f, 90f)] public float maxGroundAngle = 45f;
    [Header("Parede (Wall Mechanics)")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(10f, 18f);

    // --- Componentes e Estado Interno ---
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private float moveInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private bool isDashing = false;
    private bool isTouchingWallRight;
    private bool isTouchingWallLeft;
    private bool isWallSliding;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (isDashing) return;
        moveInput = Input.GetAxisRaw("Horizontal");
        if (!isWallSliding)
        {
            HandleFlipLogic();
        }
        UpdateTimers();
        UpdateDebugUI();
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        CheckCollisions();
        HandleWallSlideLogic();
        HandleMovement();
        HandleGravity();
    }

    private void CheckCollisions()
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        Vector2 capsuleSize = capsuleCollider.size;
        RaycastHit2D hit = Physics2D.CapsuleCast(capsuleCenter, capsuleSize, capsuleCollider.direction, 0f, Vector2.down, groundCheckDistance, collisionLayer);
        isGrounded = false;
        if (hit.collider != null && Vector2.Angle(hit.normal, Vector2.up) < maxGroundAngle)
        {
            isGrounded = true;
            coyoteTimeCounter = coyoteTime;
        }
        float wallRayStartOffset = capsuleCollider.size.x * 0.5f;
        isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer);
        isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer);
    }

    private void HandleWallSlideLogic()
    {
        bool wasSliding = isWallSliding;
        bool isSlidingOnRightWall = isTouchingWallRight && !isGrounded && moveInput > 0.1f;
        bool isSlidingOnLeftWall = isTouchingWallLeft && !isGrounded && moveInput < -0.1f;
        isWallSliding = isSlidingOnRightWall || isSlidingOnLeftWall;

        // SE COMEÇOU A DESLIZAR AGORA, ANULA TODO O MOVIMENTO
        if (!wasSliding && isWallSliding)
        {
            rb.linearVelocity = Vector2.zero; // Regra #1: Zera a velocidade

            // Força a orientação correta
            if (isSlidingOnRightWall && isFacingRight) Flip();
            else if (isSlidingOnLeftWall && !isFacingRight) Flip();
        }
    }

    private void HandleMovement()
    {
        if (isWallSliding)
        {
            // Aplica a velocidade de deslize constante
            rb.linearVelocity = new Vector2(0, -wallSlideSpeed);
            return;
        }

        // Movimento normal
        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = speedDiff * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void HandleGravity()
    {
        // A gravidade é controlada pelo HandleMovement durante o Wall Slide
        if (isWallSliding)
        {
            rb.gravityScale = 0;
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravityScaleOnFall;
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private void UpdateTimers()
    {
        if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleFlipLogic()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (mouseWorldPosition.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        else if (mouseWorldPosition.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    // --- MÉTODOS PÚBLICOS (API) ---
    public bool IsGrounded() => isGrounded;
    public bool IsWallSliding() => isWallSliding;
    public float GetVerticalVelocity() => rb.linearVelocity.y;
    public float GetHorizontalInput() => moveInput;
    public Vector2 GetFacingDirection() => isFacingRight ? Vector2.right : Vector2.left;
    public Vector2 GetDashDirection()
    {
        if (Mathf.Abs(moveInput) > 0.01f) return moveInput > 0 ? Vector2.right : Vector2.left;
        return GetFacingDirection();
    }
    public void SetGravityScale(float scale) => rb.gravityScale = scale;
    public float GetGravityScale() => rb.gravityScale;
    public void SetVelocity(float x, float y) => rb.linearVelocity = new Vector2(x, y);
    public bool CanJump()
    {
        return coyoteTimeCounter > 0f || isWallSliding;
    }

    public void DoJump(float multiplier)
    {
        if (isWallSliding)
        {
            // Regra #2: PULO DA PAREDE É FIXO E IGNORA INPUT
            isWallSliding = false;
            rb.gravityScale = baseGravity;

            // Determina a força para a direção oposta da parede que estava tocando
            float forceX = isTouchingWallLeft ? wallJumpForce.x : -wallJumpForce.x;

            // Zera a velocidade antes de aplicar a nova força para um pulo consistente
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(forceX, wallJumpForce.y * multiplier), ForceMode2D.Impulse);

            // Vira o personagem para a nova direção do pulo
            if ((forceX > 0 && !isFacingRight) || (forceX < 0 && isFacingRight))
            {
                Flip();
            }
        }
        else // Pulo normal/aéreo
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);
            coyoteTimeCounter = 0f;
        }
    }

    public void OnDashStart()
    {
        isDashing = true;
        rb.gravityScale = 0f;
    }
    public void OnDashEnd()
    {
        isDashing = false;
        rb.gravityScale = baseGravity;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    public bool IsDashing()
    {
        return isDashing;
    }

    private void UpdateDebugUI()
    {
        if (groundCheckStatusText != null)
        {
            groundCheckStatusText.text = $"Grounded: {isGrounded}\nWallSliding: {isWallSliding}\nIsDashing: {isDashing}";
        }
    }
}