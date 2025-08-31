using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{

    [Header("Referências")]
    public PlayerAnimatorController animatorController;
    public Camera mainCamera;
    public TextMeshProUGUI stateCheckText;
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

    [Header("Física da Parábola")]
    public float parabolaSteeringForce = 5f;

    [Tooltip("A velocidade máxima que o jogador pode atingir no ar durante um lançamento.")]
    public float parabolaMaxAirSpeed = 20f;
    [Tooltip("Atrito do ar. Controla o quão rápido o jogador perde velocidade. 1 = sem atrito, 0.98 = atrito suave.")]
    [Range(0.9f, 1f)]
    public float parabolaAirDrag = 0.98f;
    private bool physicsControlDisabled = false;
    private float currentGravityScaleOnFall;
    


    [Header("Física de Dano")]
    public float knockbackForce = 5f;
    public float knockbackUpwardForce = 3f;
    public float knockbackDuration = 0.2f;
    public void SetMoveInput(float input)
    {
        moveInput = input;
    }


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
    private bool isIgnoringSteeringInput = false;
    private Coroutine steeringGraceCoroutine;



    public void DisablePhysicsControl()
    {
        physicsControlDisabled = true;
    }

    public void EnablePhysicsControl()
    {
        physicsControlDisabled = false;
    }

    public void FaceDirection(int direction)
    {
        if (direction > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction < 0 && isFacingRight)
        {
            Flip();
        }
    }


    public bool CheckState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IsGrounded: return IsGrounded();
            case PlayerState.CanJumpFromGround: return CanJumpFromGround();
            case PlayerState.IsInAir: return !IsGrounded();
            case PlayerState.IsTouchingWall: return IsTouchingWall();
            case PlayerState.IsWallSliding: return IsWallSliding();
            case PlayerState.IsDashing: return IsDashing();
            case PlayerState.IsJumping: return IsJumping();
            case PlayerState.IsInParabola: return IsInParabolaArc();
            case PlayerState.IsWallJumping: return IsWallJumping();
            case PlayerState.IsLanding: return isLanding;
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
        currentGravityScaleOnFall = gravityScaleOnFall;
        rb.linearDamping = 0f; // <-- RESETA O DAMPING DA UNITY
    }
    // Em AdvancedPlayerMovement2D.cs
    public void DoLaunch(float horizontalForce, float verticalForce, Vector2 direction, float customGravityOnFall, float damping)
    {
        isInParabolaArc = true;
        rb.linearVelocity = new Vector2(direction.x * horizontalForce, verticalForce);
        currentGravityScaleOnFall = customGravityOnFall;
        rb.linearDamping = damping; // <-- DEFINE O DAMPING DA UNITY
    }

    // Em AdvancedPlayerMovement2D.cs
    public void DoWallLaunch(float horizontalForce, float verticalForce, float customGravityOnFall, float damping)
    {
        StopWallSlide();
        isInParabolaArc = true;

        // SUA LÓGICA: O impulso é SEMPRE para longe da parede,
        // usando a direção que o Flip já estabeleceu.
        Vector2 ejectDirection = GetFacingDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * horizontalForce, verticalForce);

        currentGravityScaleOnFall = customGravityOnFall;
        rb.linearDamping = damping;

        // INICIA O PERÍODO DE IMUNIDADE
        // Se já houver uma corrotina rodando, ela é parada para evitar bugs.
        if (steeringGraceCoroutine != null)
        {
            StopCoroutine(steeringGraceCoroutine);
        }
        // O tempo de 0.05s que você pediu.
        steeringGraceCoroutine = StartCoroutine(SteeringGracePeriodCoroutine(0.2f));
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        currentGravityScaleOnFall = gravityScaleOnFall;
    }

    void Update()
    {

        if (physicsControlDisabled)
        {
            UpdateTimers();
            return;
        }



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

        if (physicsControlDisabled)
        {

            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        CheckCollisions();
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
    // Em AdvancedPlayerMovement2D.cs, adicione esta função inteira
    private IEnumerator SteeringGracePeriodCoroutine(float duration)
    {
        isIgnoringSteeringInput = true;
        yield return new WaitForSeconds(duration);
        isIgnoringSteeringInput = false;
        steeringGraceCoroutine = null;
    }

    public void OnLandingStart()
    {
        isLanding = true; // <-- Define o estado
        physicsControlDisabled = true;
    }

    public void OnLandingComplete()
    {
        isLanding = false; // <-- Limpa o estado
        physicsControlDisabled = false;
    }

    public void StopWallSlide()
    {
        isWallSliding = false;
        rb.gravityScale = baseGravity;
    }







    private void CheckCollisions()
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        Vector2 capsuleSize = capsuleCollider.size;
        RaycastHit2D hit = Physics2D.CapsuleCast(capsuleCenter, capsuleSize, capsuleCollider.direction, 0f, Vector2.down, 0.1f, collisionLayer);

        isGrounded = hit.collider != null && Vector2.Angle(hit.normal, Vector2.up) < 45f;
        isGrounded = PerformGroundCheck();

        if (isGrounded)
        {

            if (rb.linearVelocity.y <= 0.1f)
            {
                isInParabolaArc = false;
                isWallJumping = false;
                isWallSliding = false;
                if (rb.linearDamping > 0) rb.linearDamping = 0f;
            }
            coyoteTimeCounter = coyoteTime;
            isJumping = false;
        }


        float wallRayStartOffset = capsuleCollider.size.x * 0.5f;
        isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer);
        isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer);

        if (isInParabolaArc && IsTouchingWall())
        {

            bool hitWallWhileMovingRight = isTouchingWallRight && rb.linearVelocity.x > 0;
            bool hitWallWhileMovingLeft = isTouchingWallLeft && rb.linearVelocity.x < 0;

            if (hitWallWhileMovingRight || hitWallWhileMovingLeft)
            {
                isInParabolaArc = false;
                rb.linearDamping = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }


    public void DoJump(float multiplier)
    {

        if (isInParabolaArc)
        {
            return;
        }

        isJumping = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);
    }
    public void DoWallJump(Vector2 force)
    {
        StopAllCoroutines();
        StartCoroutine(WallJumpCoroutine(force));
    }
    private IEnumerator WallJumpCoroutine(Vector2 force)
    {
        isJumping = true;
        isWallSliding = false;
        isWallJumping = true;

        Vector2 ejectDirection = GetWallEjectDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * force.x, force.y);
        Flip();


        yield return new WaitForSeconds(0.2f);


        isWallJumping = false;
    }
    public void Flip() { isFacingRight = !isFacingRight; transform.Rotate(0f, 180f, 0f); }
    // Em AdvancedPlayerMovement2D.cs
    private void HandleFlipLogic()
    {
        // --- ADICIONE ESTA CONDIÇÃO ---
        // Se a imunidade ao input estiver ativa, não faça nada.
        if (isIgnoringSteeringInput)
        {
            return;
        }
        // --- FIM DA ADIÇÃO ---

        // A lógica original continua aqui
        if (moveInput > 0.01f && !isFacingRight) Flip();
        else if (moveInput < -0.01f && isFacingRight) Flip();
    }

    public void StartWallSlide(float speed)
    {
        if (!IsTouchingWall() || isGrounded) return;
        isWallSliding = true;
        currentWallSlideSpeed = speed;

        if (isTouchingWallRight && isFacingRight) Flip();

        else if (isTouchingWallLeft && !isFacingRight) Flip();
    }


    private bool PerformGroundCheck()
    {

        float raycastDistance = 0.1f;
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        float halfWidth = (capsuleCollider.size.x / 2f) - 0.05f;
        Vector2 leftOrigin = new Vector2(capsuleCenter.x - halfWidth, capsuleCenter.y);
        Vector2 centerOrigin = capsuleCenter;
        Vector2 rightOrigin = new Vector2(capsuleCenter.x + halfWidth, capsuleCenter.y);
        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);
        RaycastHit2D hitCenter = Physics2D.Raycast(centerOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);
        return hitLeft.collider != null || hitCenter.collider != null || hitRight.collider != null;
    }


    // Em AdvancedPlayerMovement2D.cs
    private void HandleMovement()
    {
        if (isDashing || isWallJumping || isWallSliding)
        {
            return;
        }

        if (isInParabolaArc)
        {
            // Se a imunidade estiver ativa, o input do jogador é completamente ignorado.
            if (isIgnoringSteeringInput)
            {
                return;
            }

            // Após a imunidade, a lógica de troca de direção funciona como antes.
            bool wantsToChangeDirection = (moveInput > 0 && rb.linearVelocity.x < -0.1f) || (moveInput < 0 && rb.linearVelocity.x > 0.1f);

            if (wantsToChangeDirection)
            {
                rb.linearVelocity = new Vector2(-rb.linearVelocity .x, rb.linearVelocity.y);
                Flip();
            }

            return;
        }

        // 3. Lógica de movimento padrão no chão/ar (inalterada)
        bool isPushingAgainstWall = !isGrounded && IsTouchingWall() && ((moveInput > 0 && isTouchingWallRight) || (moveInput < 0 && isTouchingWallLeft));
        if (isPushingAgainstWall) return;

        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(speedDiff * accelRate * Vector2.right);
    }
    private void HandleGravity()
    {
        if (isWallSliding)
        {
            rb.gravityScale = 0;
            float speed = currentWallSlideSpeed > 0 ? currentWallSlideSpeed : wallSlideSpeed;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -speed);
        }
        else
        {
            rb.gravityScale = rb.linearVelocity.y < 0 ? currentGravityScaleOnFall : baseGravity;
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
    public bool IsWallJumping() { return isWallJumping; }
    public bool IsJumping() { return isJumping; }
    public bool IsFacingRight() { return isFacingRight; }
    public Rigidbody2D GetRigidbody() { return rb; }


    // Em AdvancedPlayerMovement2D.cs
    private void UpdateDebugUI()
    {
        // Usa a nova variável 'stateCheckText'. Se você não a renomeou, mantenha 'groundCheckStatusText'.
        if (stateCheckText != null)
        {
            // Pega a velocidade atual do Rigidbody
            Vector2 currentVelocity = rb.linearVelocity;

            // Monta o texto com os estados e a nova informação de velocidade
            stateCheckText.text = $"Grounded: {CheckState(PlayerState.IsGrounded)}\n" + $"WallSliding: {CheckState(PlayerState.IsWallSliding)}\n" + $"Dashing: {CheckState(PlayerState.IsDashing)}\n" + $"InParabola: {CheckState(PlayerState.IsInParabola)}\n" + $"--- VELOCIDADE ---\n" + $"X Speed: {currentVelocity.x:F2}\n" +  $"Y Speed: {currentVelocity.y:F2}";   
        }
    }
}