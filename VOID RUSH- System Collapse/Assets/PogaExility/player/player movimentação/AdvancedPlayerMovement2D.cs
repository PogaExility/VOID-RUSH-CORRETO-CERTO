using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{
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
    private bool isTouchingWall;
    private bool isWallSliding;
    private float coyoteTimeCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        // A leitura de input agora é feita pelo PlayerController, mas o movimento horizontal ainda é lido aqui.
        moveInput = Input.GetAxisRaw("Horizontal");

        HandleFlipLogic();
        UpdateTimers();
        UpdateDebugUI();
    }

    void FixedUpdate()
    {
        CheckCollisions();
        HandleWallSlideLogic();
        HandleMovement();
        HandleGravity();
    }

    // --- LÓGICAS DE MOVIMENTO E COLISÃO (FIXED UPDATE) ---

    private void CheckCollisions()
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        Vector2 capsuleSize = capsuleCollider.size;

        RaycastHit2D hit = Physics2D.CapsuleCast(capsuleCenter, capsuleSize, capsuleCollider.direction, 0f, Vector2.down, groundCheckDistance, collisionLayer);

        isGrounded = false;
        if (hit.collider != null)
        {
            if (Vector2.Angle(hit.normal, Vector2.up) < maxGroundAngle)
            {
                isGrounded = true;
                coyoteTimeCounter = coyoteTime;
            }
        }

        float direction = isFacingRight ? 1f : -1f;
        float wallRayStartOffset = capsuleCollider.size.x * 0.5f;
        RaycastHit2D wallHit = Physics2D.Raycast(capsuleCenter, new Vector2(direction, 0f), wallRayStartOffset + wallCheckDistance, collisionLayer);
        isTouchingWall = wallHit.collider != null && !isGrounded; // Parede não é detectada se estiver no chão
    }

    private void HandleWallSlideLogic()
    {
        bool isPressingAgainstWall = (isFacingRight && moveInput > 0) || (!isFacingRight && moveInput < 0);
        isWallSliding = isTouchingWall && !isGrounded && isPressingAgainstWall;
    }

    private void HandleMovement()
    {
        if (isWallSliding)
        {
            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
            return;
        }

        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = speedDiff * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void HandleGravity()
    {
        if (rb.linearVelocity.y < 0 && !isWallSliding)
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
        if (isWallSliding) return;

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

    // --- MÉTODOS PÚBLICOS (API para outros scripts) ---

    public bool IsGrounded() => isGrounded;
    public bool IsWallSliding() => isWallSliding;
    public float GetVerticalVelocity() => rb.linearVelocity.y;
    public float GetHorizontalInput() => moveInput;
    public Vector2 GetFacingDirection() => isFacingRight ? Vector2.right : Vector2.left;

    public void SetGravityScale(float scale) => rb.gravityScale = scale;
    public float GetGravityScale() => rb.gravityScale;
    public void SetVelocity(float x, float y) => rb.linearVelocity = new Vector2(x, y);

    public bool CanJump()
    {
        // Retorna true se o jogador pode pular do chão, da parede ou usando coyote time.
        return coyoteTimeCounter > 0f || isWallSliding;
    }

    public void DoJump(float multiplier)
    {
        // Pulo normal/aéreo
        if (!isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);
            coyoteTimeCounter = 0f; // Impede uso de coyote time depois de um pulo aéreo
        }
        // Pulo da parede
        else
        {
            isWallSliding = false;
            float forceX = isFacingRight ? -wallJumpForce.x : wallJumpForce.x;
            rb.linearVelocity = new Vector2(forceX, wallJumpForce.y * multiplier);
            Flip();
        }
    }

    // --- DEBUG ---
    private void UpdateDebugUI()
    {
        if (groundCheckStatusText != null)
        {
            groundCheckStatusText.text = $"Grounded: {isGrounded}\nWallSliding: {isWallSliding}\nTouchingWall: {isTouchingWall}";
        }
    }
}