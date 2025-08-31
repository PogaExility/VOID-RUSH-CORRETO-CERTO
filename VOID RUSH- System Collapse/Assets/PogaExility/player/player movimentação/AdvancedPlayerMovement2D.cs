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
    public void SetMoveInput(float input)
    {
        moveInput = input;
    }

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
    private bool physicsControlDisabled = false; // Para pausar o controle do jogador
    private Coroutine currentPhysicsCoroutine; // Para garantir que só uma corotina de física rode por vez
                                               // Adicione estas DUAS funções em AdvancedPlayerMovement2D.cs
    public void DisablePhysicsControl()
    {
        physicsControlDisabled = true;
    }

    public void EnablePhysicsControl()
    {
        physicsControlDisabled = false;
    }

    // Adicione esta função em AdvancedPlayerMovement2D.cs
    public void FaceDirection(int direction)
    {
        // direction deve ser -1 para esquerda, 1 para direita
        if (direction > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction < 0 && isFacingRight)
        {
            Flip();
        }
    }
    // Adicione esta função em AdvancedPlayerMovement2D.cs
   


    // Dentro do AdvancedPlayerMovement2D.cs

    public bool CheckState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IsGrounded: return IsGrounded();
            case PlayerState.CanJumpFromGround: return CanJumpFromGround(); // <-- ADICIONE ESTA LINHA
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

    // Em AdvancedPlayerMovement2D.cs, modifique estas TRÊS funções.
    public void OnParabolaEnd()
    {
        isInParabolaArc = false;
        rb.linearDamping = 0f;
        // DELETE A LINHA ABAIXO
        // EnablePhysicsControl(); // <--- REMOVA ISTO
    }

    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs

    // Em AdvancedPlayerMovement2D.cs

    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    public void DoLaunch(float horizontalForce, float verticalForce, float damping, Vector2 direction)
    {
        // REMOVA AS TRÊS LINHAS ABAIXO. Elas são a causa do bug.
        // isLanding = false;
        // isGrounded = false;
        // coyoteTimeCounter = 0;

        // O código original e correto é este:
        isInParabolaArc = true;
        rb.linearDamping = damping;
        rb.linearVelocity = new Vector2(direction.x * horizontalForce, verticalForce);
    }
    // Adicione esta corotina em qualquer lugar dentro da classe PlayerController
    private IEnumerator LandingCoroutine()
    {
        isLanding = true;
        OnLandingStart(); // CORRIGIDO
        animatorController.PlayState(PlayerAnimState.pousando);

        yield return new WaitForSeconds(0.3f);

        isLanding = false;
        OnLandingComplete(); // CORRIGIDO
    }

    public void DoWallLaunch(float horizontalForce, float verticalForce, float damping)
    {
        
        isLanding = false;
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
        // SE O CONTROLE ESTIVER DESATIVADO, A ÚNICA COISA QUE ATUALIZAMOS SÃO OS TIMERS.
        if (physicsControlDisabled)
        {
            UpdateTimers(); // O Coyote time precisa continuar contando
            return;
        }

        // Se o controle está ativo, tudo funciona como antes.
       
        isJumping = rb.linearVelocity.y > 0.1f && !isGrounded;
        if (!isWallSliding && !isWallJumping && !isInKnockback)
        {
            HandleFlipLogic();
        }
        UpdateTimers();
        UpdateDebugUI();
    }

    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs, substitua a FixedUpdate inteira por esta
    void FixedUpdate()
    {
        // SEM TRAVAS AQUI
        CheckCollisions();

        // Lógica normal
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

    // Em AdvancedPlayerMovement2D.cs

    private bool IsTouchingWallOnSide(bool checkRight)
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        Vector2 direction = checkRight ? Vector2.right : Vector2.left;
        return Physics2D.Raycast(capsuleCenter, direction, (capsuleCollider.size.x * 0.5f) + wallCheckDistance, collisionLayer);
    }

    // Em AdvancedPlayerMovement2D.cs
    private void CheckCollisions()
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        Vector2 capsuleSize = capsuleCollider.size;
        RaycastHit2D hit = Physics2D.CapsuleCast(capsuleCenter, capsuleSize, capsuleCollider.direction, 0f, Vector2.down, 0.1f, collisionLayer);

        isGrounded = hit.collider != null && Vector2.Angle(hit.normal, Vector2.up) < 45f;
        isGrounded = PerformGroundCheck();

        if (isGrounded)
        {
            // Limpa os estados aéreos ao tocar o chão
            if (rb.linearVelocity.y <= 0.1f)
            {
                isInParabolaArc = false;
                isWallJumping = false;
                isWallSliding = false; // Garante que o WallSlide pare no chão
                if (rb.linearDamping > 0) rb.linearDamping = 0f;
            }
            coyoteTimeCounter = coyoteTime;
            isJumping = false;
        }

        // A checagem de parede que define isTouchingWallRight e isTouchingWallLeft
        float wallRayStartOffset = capsuleCollider.size.x * 0.5f;
        isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer);
        isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer);

        // Lógica de cancelamento da parábola ao tocar a parede
        if (isInParabolaArc && IsTouchingWall())
        {
            // Cancela a parábola apenas se estiver se movendo CONTRA a parede
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

    // Em AdvancedPlayerMovement2D.cs
    public void DoJump(float multiplier)
    {
        // --- A TRAVA DE SEGURANÇA ---
        // Se o personagem já estiver no meio de uma parábola, NÃO PERMITA UM PULO AÉREO.
        if (isInParabolaArc)
        {
            return;
        }

        // Se não, o pulo funciona normalmente.
        isJumping = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);
    }
    public void DoWallJump(Vector2 force)
    {
        StopAllCoroutines(); // Para o caso de alguma outra corotina de movimento estar ativa
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

        // DURAÇÃO DO ESTADO DE WALL JUMP - permite que o jogador se afaste da parede sem gravidade
        yield return new WaitForSeconds(0.2f); // Você pode ajustar este tempo

        // Fim do estado, a gravidade e o controle aéreo voltam ao normal
        isWallJumping = false;
    }
    public void Flip() { isFacingRight = !isFacingRight; transform.Rotate(0f, 180f, 0f); }
    private void HandleFlipLogic() { if (moveInput > 0.01f && !isFacingRight) Flip(); else if (moveInput < -0.01f && isFacingRight) Flip(); }

    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    public void StartWallSlide(float speed)
    {
        if (!IsTouchingWall() || isGrounded) return;
        isWallSliding = true;
        currentWallSlideSpeed = speed;

        // --- A LÓGICA DE FLIP CORRETA ---
        // Se está na parede da DIREITA, deve olhar para a ESQUERDA (de costas para a parede).
        if (isTouchingWallRight && isFacingRight) Flip();
        // Se está na parede da ESQUERDA, deve olhar para a DIREITA.
        else if (isTouchingWallLeft && !isFacingRight) Flip();
    }

    // Adicione esta função em AdvancedPlayerMovement2D.cs
    private bool PerformGroundCheck()
    {
        // Usa múltiplos raycasts para uma detecção de chão robusta
        float raycastDistance = 0.1f;
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        float halfWidth = (capsuleCollider.size.x / 2f) - 0.05f; // Um pouco para dentro

        // Posições dos raycasts: esquerda, centro, direita
        Vector2 leftOrigin = new Vector2(capsuleCenter.x - halfWidth, capsuleCenter.y);
        Vector2 centerOrigin = capsuleCenter;
        Vector2 rightOrigin = new Vector2(capsuleCenter.x + halfWidth, capsuleCenter.y);

        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);
        RaycastHit2D hitCenter = Physics2D.Raycast(centerOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);

        // Se qualquer um dos três tocar o chão, estamos no chão.
        return hitLeft.collider != null || hitCenter.collider != null || hitRight.collider != null;
    }
    // Dentro do AdvancedPlayerMovement2D.cs

    // Dentro do AdvancedPlayerMovement2D.cs

    // Em AdvancedPlayerMovement2D.cs, substitua HandleMovement por esta versão
    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    private void HandleMovement()
    {
        if (isDashing || isWallJumping) return;

        if (isInParabolaArc)
        {
            if ((moveInput > 0 && rb.linearVelocity.x >= parabolaMaxAirSpeed) || (moveInput < 0 && rb.linearVelocity.x <= -parabolaMaxAirSpeed))
            {
                return;
            }
            rb.AddForce(Vector2.right * moveInput * parabolaSteeringForce);
            return;
        }

        if (isWallSliding) return;

        // A correção está aqui: usa IsTouchingWall()
        bool isPushingAgainstWall = !isGrounded && IsTouchingWall() && ((moveInput > 0 && isTouchingWallRight) || (moveInput < 0 && isTouchingWallLeft));
        if (isPushingAgainstWall) return;

        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(speedDiff * accelRate * Vector2.right);
    }

    // Dentro do AdvancedPlayerMovement2D.cs

    // Em AdvancedPlayerMovement2D.cs
    // Em AdvancedPlayerMovement2D.cs
    private void HandleWallSlideLogic()
    {
        // Usa a função IsTouchingWall() que já existe e funciona
        if (isWallSliding && (!IsTouchingWall() || isGrounded))
        {
            StopWallSlide();
        }
    }
    // Dentro do AdvancedPlayerMovement2D.cs

    // Em AdvancedPlayerMovement2D.cs, substitua HandleGravity por esta versão
    // Em AdvancedPlayerMovement2D.cs
    private void HandleGravity()
    {
        if (isWallSliding)
        {
            rb.gravityScale = 0; // Desliga a gravidade
                                 // Usa a velocidade definida pela skill, ou a padrão se não tiver sido definida.
            float speed = currentWallSlideSpeed > 0 ? currentWallSlideSpeed : wallSlideSpeed;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -speed);
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