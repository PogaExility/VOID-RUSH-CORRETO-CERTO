using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{

    [Header("Referências")]
    public PlayerAnimatorController animatorController;
    public Camera mainCamera;
    public TextMeshProUGUI groundCheckStatusText;
    public LayerMask collisionLayer;

    [Header("Movimento")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 60f;

    [Header("Pulo")]
    public float jumpForce = 12f;
    public float gravityScaleOnFall = 1.6f;
    public float baseGravity = 1f;
    public float coyoteTime = 0.1f;

    [Header("Parede")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    public float wallCheckDistance = 0.1f;

    [Header("Física do WallDashJump")]
    public float parabolaLinearDamping = 0.3f;
    public float parabolaSteeringForce = 100f;
    public float parabolaMaxAirSpeed = 20f;

    [Header("Física de Dano")]
    public float knockbackForce = 5f;
    public float knockbackUpwardForce = 3f;
    public float knockbackDuration = 0.2f;

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
    private float currentWallSlideSpeed;
    private bool isDashing = false;
    private bool isWallJumping = false;
    private bool isInParabolaArc = false;
    private bool isJumping = false;
    private bool isLanding = false;
    private bool isInKnockback = false;
    private Coroutine knockbackCoroutine;

    // Adicione estas 3 funções em qualquer lugar dentro da classe AdvancedPlayerMovement2D

    // Dentro do AdvancedPlayerMovement2D.cs

    public bool CheckState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IsGrounded: return IsGrounded();

            // --- AQUI ESTÁ A LÓGICA DA SUA NOVA CONDIÇÃO ---
            case PlayerState.IsInAir: return !IsGrounded();

            case PlayerState.IsTouchingWall: return IsTouchingWall();
            case PlayerState.IsWallSliding: return IsWallSliding();
            case PlayerState.IsDashing: return IsDashing();
            case PlayerState.IsJumping: return IsJumping();
            case PlayerState.IsInParabola: return IsInParabolaArc();
            case PlayerState.IsWallJumping: return IsWallJumping();
            default: return false;
        }
    }
    public void OnParabolaStart(float damping)
    {
        isInParabolaArc = true;
        rb.linearDamping = damping;
    }

    public void OnParabolaEnd()
    {
        isInParabolaArc = false;
        rb.linearDamping = 0f;
    }
    public void DoLaunch(float horizontalForce, float verticalForce, float damping)
    {
        OnParabolaStart(damping); // Reutiliza o estado de parábola
        rb.linearVelocity = new Vector2(GetFacingDirection().x * horizontalForce, verticalForce);
    }

    public void DoWallLaunch(float horizontalForce, float verticalForce, float damping)
    {
        StopWallSlide();
        OnParabolaStart(damping);
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * horizontalForce, verticalForce);
        if ((ejectDirection.x > 0 && !isFacingRight) || (ejectDirection.x < 0 && isFacingRight)) { Flip(); }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
    }

    void Update()
    {
        if (isInKnockback || isLanding) { moveInput = 0; }
        else { moveInput = Input.GetAxisRaw("Horizontal"); }

        isJumping = rb.linearVelocity.y > 0.1f && !isGrounded;

        if (!isWallSliding && !isWallJumping && !isInKnockback)
        {
            HandleFlipLogic();
        }

        UpdateTimers();
        UpdateDebugUI();
    }

    void FixedUpdate()
    {
        CheckCollisions();

        if (isLanding) { rb.linearVelocity = Vector2.zero; return; }
        if (isDashing || isWallJumping || isInKnockback) return;

        HandleWallSlideLogic();
        HandleMovement();
        HandleGravity();
    }

    public void Freeze() { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
    public void Unfreeze() { rb.bodyType = RigidbodyType2D.Dynamic; }

    public void ApplyKnockback(Vector2 attackDirection)
    {
        if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
        knockbackCoroutine = StartCoroutine(KnockbackCoroutine(attackDirection));
    }

    private IEnumerator KnockbackCoroutine(Vector2 attackDirection)
    {
        isInKnockback = true;
        rb.linearVelocity = Vector2.zero;
        Vector2 knockbackDirection = -attackDirection.normalized;
        knockbackDirection.y += knockbackUpwardForce;
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        isInKnockback = false;
        knockbackCoroutine = null;
    }

    public void OnLandingStart() { isLanding = true; }
    public void OnLandingComplete() { isLanding = false; }
    public void StopWallSlide()
    {
        isWallSliding = false;
        rb.gravityScale = baseGravity; // Restaura a gravidade imediatamente
    }
    public void DoWallDashJump(float horizontalForce, float verticalForce, float damping)
    {
        isWallSliding = false;
        isInParabolaArc = true;
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(ejectDirection.x * horizontalForce, verticalForce);
        if ((ejectDirection.x > 0 && !isFacingRight) || (ejectDirection.x < 0 && isFacingRight)) { Flip(); }
        rb.linearDamping = damping;
    }

    private void CheckCollisions() { Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset; Vector2 capsuleSize = capsuleCollider.size; RaycastHit2D hit = Physics2D.CapsuleCast(capsuleCenter, capsuleSize, capsuleCollider.direction, 0f, Vector2.down, 0.1f, collisionLayer); isGrounded = hit.collider != null && Vector2.Angle(hit.normal, Vector2.up) < 45f; if (isGrounded) { if (isInParabolaArc || isWallJumping) rb.linearDamping = 0f; isWallJumping = false; isInParabolaArc = false; coyoteTimeCounter = coyoteTime; isJumping = false; } float wallRayStartOffset = capsuleCollider.size.x * 0.5f; isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer); isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer); }
    public void DoJump(float multiplier) { isJumping = true; rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier); }
    public void DoWallJump(Vector2 force)
    {
        isJumping = true;
        isWallSliding = false;
        isWallJumping = true;
        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * force.x, force.y);
        Flip();
    }
    public void Flip() { isFacingRight = !isFacingRight; transform.Rotate(0f, 180f, 0f); }
    private void HandleFlipLogic() { if (moveInput > 0.01f && !isFacingRight) Flip(); else if (moveInput < -0.01f && isFacingRight) Flip(); }

    public void StartWallSlide(float speed)
    {
        isWallSliding = true;
        currentWallSlideSpeed = speed; // Armazena a velocidade do SO
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Zera a velocidade vertical para "grudar"
        if ((isFacingRight && isTouchingWallRight) || (!isFacingRight && isTouchingWallLeft))
        {
            Flip();
        }
    }

    // Dentro do AdvancedPlayerMovement2D.cs

    // Dentro do AdvancedPlayerMovement2D.cs

    private void HandleMovement()
    {
        if (isInParabolaArc)
        {
            // Sua lógica de parábola, que já está boa
            if ((moveInput > 0 && rb.linearVelocity.x >= parabolaMaxAirSpeed) || (moveInput < 0 && rb.linearVelocity.x <= -parabolaMaxAirSpeed))
            {
                return;
            }
            rb.AddForce(Vector2.right * moveInput * parabolaSteeringForce);
        }
        else
        {
            // Se estiver deslizando na parede, o movimento horizontal é bloqueado.
            if (isWallSliding) return;

            bool isPushingAgainstWall = !isGrounded && ((moveInput > 0 && isTouchingWallRight) || (moveInput < 0 && isTouchingWallLeft));
            if (isPushingAgainstWall) return;

            // --- AQUI ESTÁ A CORREÇÃO DO "MOONWALK" ---
            // A velocidade alvo usa 'moveSpeed', não 'moveInput' de novo.
            float targetSpeed = moveInput * moveSpeed;
            float speedDiff = targetSpeed - rb.linearVelocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            rb.AddForce(speedDiff * accelRate * Vector2.right);
        }
    }

    // Dentro do AdvancedPlayerMovement2D.cs

    private void HandleWallSlideLogic()
    {
        // Se o jogador está no estado de WallSliding, mas não está mais tocando a parede
        // ou se ele tocou o chão, ele deve parar de deslizar.
        if (isWallSliding && (!IsTouchingWall() || isGrounded))
        {
            StopWallSlide();
        }
    }
    // Dentro do AdvancedPlayerMovement2D.cs

    private void HandleGravity()
    {
        if (isWallSliding)
        {
            rb.gravityScale = 0; // Gravidade zero durante o slide
                                 // USA A VELOCIDADE CORRETA AGORA!
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -currentWallSlideSpeed);
        }
        else
        {
            rb.gravityScale = rb.linearVelocity.y < 0 ? gravityScaleOnFall : baseGravity;
        }
    }
    private void UpdateTimers() { if (!isGrounded) coyoteTimeCounter -= Time.deltaTime; }
    public void CutJump() { if (rb.linearVelocity.y > 0) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); } }

    public Vector2 GetWallEjectDirection() { return isTouchingWallLeft ? Vector2.right : Vector2.left; }
    public bool IsGrounded() { return isGrounded; }
    public bool IsWallSliding() { return isWallSliding; }
    public bool IsInParabolaArc() { return isInParabolaArc; }
    public float GetVerticalVelocity() { return rb.linearVelocity.y; }
    public bool IsMoving() { return Mathf.Abs(moveInput) > 0.1f; }
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
    // Dentro do seu AdvancedPlayerMovement2D.cs

    private void UpdateDebugUI()
    {
        if (groundCheckStatusText != null)
        {
            // Adicionamos os novos estados ao texto de debug.
            // IsInAir é simplesmente o contrário de IsGrounded.
            groundCheckStatusText.text = $"Grounded: {IsGrounded()}\n" +
                                         $"IsInAir: {!IsGrounded()}\n" + // ADICIONADO
                                         $"WallSliding: {IsWallSliding()}\n" +
                                         $"IsTouchingWall: {IsTouchingWall()}\n" + // ADICIONADO
                                         $"IsJumping: {IsJumping()}\n" +
                                         $"Dashing: {IsDashing()}";
        }
    }
}