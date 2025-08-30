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
    public void DoLaunch(float horizontalForce, float verticalForce, float damping, Vector2 direction)
    {
        isGrounded = false;
        coyoteTimeCounter = 0;
        isInParabolaArc = true;
        rb.linearDamping = damping;
        // USA A DIREÇÃO FORNECIDA, NÃO DECIDE MAIS SOZINHO
        rb.linearVelocity = new Vector2(direction.x * horizontalForce, verticalForce);
    }

    public void DoWallLaunch(float horizontalForce, float verticalForce, float damping)
    {
        // DELETE A LINHA ABAIXO
        // DisablePhysicsControl(); // <--- REMOVA ISTO

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
    private void CheckCollisions()
    {
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        Vector2 capsuleSize = capsuleCollider.size;
        RaycastHit2D hit = Physics2D.CapsuleCast(capsuleCenter, capsuleSize, capsuleCollider.direction, 0f, Vector2.down, 0.1f, collisionLayer);

        isGrounded = hit.collider != null && Vector2.Angle(hit.normal, Vector2.up) < 45f;

        if (isGrounded)
        {
            // --- A LÓGICA DE CANCELAMENTO CORRETA E DEFINITIVA ---

            // A parábola SÓ é cancelada se nós estivermos LANDING (pousando),
            // ou seja, caindo com velocidade vertical negativa.
            // Se a velocidade for POSITIVA, significa que estamos SUBINDO (começando o DashJump).
            if (rb.linearVelocity.y <= 0.1f)
            {
                if (isInParabolaArc)
                {
                    Debug.Log("<color=orange>MOVEMENT (CheckCollisions):</color> Pouso detectado. Parábola cancelada.");
                }
                isInParabolaArc = false;
            }

            // O resto da lógica de quando está no chão
            if (isWallJumping) rb.linearDamping = 0f;
            isWallJumping = false;
            coyoteTimeCounter = coyoteTime;
            isJumping = false;
        }

        float wallRayStartOffset = capsuleCollider.size.x * 0.5f;
        isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer);
        isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer);
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

    public void StartWallSlide(float speed)
    {
        Debug.Log("<color=lime>CONFISSÃO:</color> A função StartWallSlide foi chamada! isWallSliding agora é TRUE.");
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

    // Em AdvancedPlayerMovement2D.cs, substitua HandleMovement por esta versão
    // Em AdvancedPlayerMovement2D.cs
    private void HandleMovement()
    {
        if (isInParabolaArc)
        {
            if ((moveInput > 0 && rb.linearVelocity.x >= parabolaMaxAirSpeed) ||
                (moveInput < 0 && rb.linearVelocity.x <= -parabolaMaxAirSpeed))
            {
                return;
            }
            rb.AddForce(Vector2.right * moveInput * parabolaSteeringForce);
        }
        else
        {
            // REMOVEMOS a linha "if (isWallSliding)..." daqui.
            bool isPushingAgainstWall = !isGrounded && ((moveInput > 0 && isTouchingWallRight) || (moveInput < 0 && isTouchingWallLeft));
            if (isPushingAgainstWall) return;
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

    // Em AdvancedPlayerMovement2D.cs, substitua HandleGravity por esta versão
    // Em AdvancedPlayerMovement2D.cs
    private void HandleGravity()
    {
        if (isWallSliding)
        {
            rb.gravityScale = 0; // Desliga a gravidade
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed); // Controla a velocidade de queda
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