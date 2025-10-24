using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{

    [Header("Referências")]
    public PlayerAnimatorController animatorController;
    public PlayerController playerController;
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

    [Header("Controle de Lógica Externa")]
    [Tooltip("Permite que outros scripts (como o PlayerController) travem o flip por movimento.")]
    public bool allowMovementFlip = true;

    [Header("Física da Parábola")]
    public float parabolaSteeringForce = 5f;

    [Tooltip("A velocidade máxima que o jogador pode atingir no ar durante um lançamento.")]
    public float parabolaMaxAirSpeed = 20f;
    [Tooltip("Atrito do ar. Controla o quão rápido o jogador perde velocidade. 1 = sem atrito, 0.98 = atrito suave.")]
    [Range(0.9f, 1f)]
    public float parabolaAirDrag = 0.98f;
    private bool physicsControlDisabled = false;
    private float currentGravityScaleOnFall;

    [Header("Rastejar")]
    public float crawlSpeed = 4f;

    // --- Variáveis de estado para o Rastejar ---
    private bool isCrawling = false;
    private bool isCrouchingDown = false;
    private bool isStandingUp = false;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    [Header("Física de Dano")]

    private DefenseHandler defenseHandler;
    public void SetMoveInput(float input)
    {
        moveInput = input;
    }
    [Header("Escada")]
    public float climbingSpeed = 5f;

    // --- Variáveis de estado para Escada ---
    private bool isClimbing = false;


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
    private bool isWallDashing = false;
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

    public void Flip()
    {
        // A trava "if (!allowMovementFlip) return;" foi REMOVIDA daqui.
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }


    // 3. SUA FUNÇÃO FaceDirection() NÃO PRECISA DE MUDANÇAS
    // -----------------------------------------------------------------
    // Apenas garanta que ela esteja chamando a nova função Flip() protegida.
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
    public void FaceTowards(Vector3 worldPoint)
    {
        // A trava de flip normal não se aplica aqui, pois a mira tem prioridade.
        // No entanto, a lógica de flip do movimento (A/D) já está bloqueada pelo PlayerController.

        bool shouldFaceRight = (worldPoint.x > transform.position.x);

        if (shouldFaceRight && !isFacingRight)
        {

            Flip();
        }
        else if (!shouldFaceRight && isFacingRight)
        {
            Flip();
        }
    }
    public void FaceTowardsPoint(Vector3 worldPoint)
    {
        // Determina se o ponto está à direita ou à esquerda do jogador.
        bool shouldFaceRight = (worldPoint.x > transform.position.x);

        // Compara com a direção atual e chama a função Flip() se for necessário.
        if (shouldFaceRight && !isFacingRight)
        {
            Flip();
        }
        else if (!shouldFaceRight && isFacingRight)
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
            case PlayerState.IsBlocking: return defenseHandler != null && defenseHandler.IsBlocking();
            case PlayerState.IsParrying: return defenseHandler != null && defenseHandler.CanParry();

            // --- ADICIONE OS NOVOS CASES QUE AGORA SÃO VÁLIDOS ---
            case PlayerState.IsWallDashing: return IsWallDashing();
            case PlayerState.IsTakingDamage: return IsTakingDamage();
            // --------------------------------------------------------

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

    // --- NOVAS FUNÇÕES PARA CONTROLE DE ESCADA ---

    public void StartClimbing()
    {
        isClimbing = true;
    }

    public void Climb(float verticalInput)
    {
        if (!isClimbing) return;

        // Aplica o movimento vertical e zera o horizontal
        rb.linearVelocity = new Vector2(0, verticalInput * climbingSpeed);
    }

    public void StopClimbing()
    {
        isClimbing = false;
        // Restaura a gravidade base imediatamente ao sair da escada
        rb.gravityScale = baseGravity;
    }

    public bool IsClimbing()
    {
        return isClimbing;
    }

    // --- FIM DAS NOVAS FUNÇÕES ---

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        currentGravityScaleOnFall = gravityScaleOnFall;
        defenseHandler = GetComponent<DefenseHandler>();

        // --- ADICIONADO: Salva as dimensões originais do collider ---
        originalColliderSize = capsuleCollider.size;
        originalColliderOffset = capsuleCollider.offset;
    }

    void Update()
    {
        Debug.Log("Estou no ar? " + CheckState(PlayerState.IsInAir));
        Debug.Log("Estou tocando a parede? " + CheckState(PlayerState.IsTouchingWall));
        // FIM DA ADIÇÃO
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
            //rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        if (isWallSliding && (!IsTouchingWall() || IsGrounded()))
        {
            StopWallSlide();
        }
        // --- FIM DA CORREÇÃO ---

        CheckCollisions();
        HandleMovement();
        HandleGravity();
    }

    public void Freeze() { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
    public void Unfreeze() { rb.bodyType = RigidbodyType2D.Dynamic; }


    public void ExecuteKnockback(float force, Vector2 attackDirection, float upwardModifier = 0.5f, float duration = 0.2f)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }
        knockbackCoroutine = StartCoroutine(ExecuteKnockbackCoroutine(force, attackDirection, upwardModifier, duration));
    }

    // --- ESTA É A CORROTINA QUE PRECISA SER SUBSTITUÍDA ---
    // Substitua sua corrotina de knockback por esta versão corrigida

    private IEnumerator ExecuteKnockbackCoroutine(float force, Vector2 direction, float upwardModifier, float duration)
    {
        // LIGA o estado de stun.
        isInKnockback = true;

        try
        {
            // 1. Preserva a gravidade, zerando apenas a velocidade horizontal.
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // 2. Calcula a direção final do impulso.
            Vector2 finalDirection = new Vector2(direction.x, direction.y + upwardModifier).normalized;

            // 3. Aplica a força como um impulso único e instantâneo.
            rb.AddForce(finalDirection * force, ForceMode2D.Impulse);

            // 4. Espera a duração do "stun". Durante este tempo, a checagem em HandleMovement()
            //    estará bloqueando o input do jogador.
            yield return new WaitForSeconds(duration);
        }
        finally
        {
            // 5. DESLIGA o estado de stun.
            // O bloco 'finally' garante que esta linha SEMPRE será executada,
            // mesmo se a corrotina for interrompida, devolvendo o controle ao jogador.
            isInKnockback = false;
            knockbackCoroutine = null;
        }
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


        // Detecta a parede da direita
        isTouchingWallRight = Physics2D.Raycast(capsuleCenter, Vector2.right, wallRayStartOffset + wallCheckDistance, collisionLayer);

        // Detecta a parede da esquerda
        isTouchingWallLeft = Physics2D.Raycast(capsuleCenter, Vector2.left, wallRayStartOffset + wallCheckDistance, collisionLayer);

        // ADICIONE ESTAS LINHAS DE DEBUG AQUI:
        Color rayColorRight = isTouchingWallRight ? Color.green : Color.red;
        Color rayColorLeft = isTouchingWallLeft ? Color.green : Color.red;
        Debug.DrawRay(capsuleCenter, Vector2.right * (wallRayStartOffset + wallCheckDistance), rayColorRight);
        Debug.DrawRay(capsuleCenter, Vector2.left * (wallRayStartOffset + wallCheckDistance), rayColorLeft);
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
        // --- ADICIONADO: Bloqueio físico de pulo ao rastejar ---
        if (isCrawling || isCrouchingDown || isStandingUp)
        {
            return;
        }
        // --- FIM DA ADIÇÃO ---

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

    // Em AdvancedPlayerMovement2D.cs
    private void HandleFlipLogic()
    {
        // Se o flip por movimento (A/D) estiver travado, a função para aqui.
        if (!allowMovementFlip) return;

        // A lógica original de ler o moveInput continua aqui.
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


    private void HandleMovement()
    {
        if (playerController != null && playerController.IsAttacking)
        {
            return;
        }

        // --- MODIFICADO: Adicionado isClimbing ---
        if (isDashing || isWallDashing || isWallJumping || isWallSliding || isInKnockback || isCrouchingDown || isStandingUp || isClimbing)
        {
            return;
        }

        if (isInParabolaArc)
        {
            if (isIgnoringSteeringInput)
            {
                return;
            }

            bool wantsToChangeDirection = (moveInput > 0 && rb.linearVelocity.x < -0.1f) || (moveInput < 0 && rb.linearVelocity.x > 0.1f);

            if (wantsToChangeDirection)
            {
                rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
                Flip();
            }

            return;
        }

        bool isPushingAgainstWall = !isGrounded && IsTouchingWall() && ((moveInput > 0 && isTouchingWallRight) || (moveInput < 0 && isTouchingWallLeft));
        if (isPushingAgainstWall) return;

        float currentSpeed = isCrawling ? crawlSpeed : moveSpeed;
        float targetSpeed = moveInput * currentSpeed;

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(speedDiff * accelRate * Vector2.right);
    }
    private void HandleGravity()
    {
        // --- MODIFICADO: Adicionado lógica para isClimbing ---
        if (isClimbing)
        {
            rb.gravityScale = 0;
        }
        else if (isWallSliding)
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
    public void BeginCrouchTransition()
    {
        isCrouchingDown = true;
        // Desabilitamos o controle para que a animação toque sem o jogador escorregar
        physicsControlDisabled = true;
        // Zeramos a velocidade para garantir que ele pare
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void CompleteCrouch()
    {
        isCrouchingDown = false;
        isCrawling = true;

        // Reduz o tamanho e ajusta o offset do collider
        float newHeight = originalColliderSize.y / 2;
        capsuleCollider.size = new Vector2(originalColliderSize.x, newHeight);
        
        // Desloca o centro do collider para baixo para que ele permaneça no chão
        float newOffsetY = originalColliderOffset.y - (originalColliderSize.y / 4);
        capsuleCollider.offset = new Vector2(originalColliderOffset.x, newOffsetY);

        // Devolve o controle da física ao jogador
        physicsControlDisabled = false;
    }

    public void BeginStandUpTransition()
    {
        isStandingUp = true;
        // Desabilitamos o controle para a animação de levantar
        physicsControlDisabled = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void CompleteStandUp()
    {
        isStandingUp = false;
        isCrawling = false;

        // Restaura o collider ao seu tamanho e posição originais
        capsuleCollider.size = originalColliderSize;
        capsuleCollider.offset = originalColliderOffset;

        // Devolve o controle da física
        physicsControlDisabled = false;
    }
    
    // Funções de verificação para o PlayerController
    public bool IsCrawling()
    {
        return isCrawling;
    }

    public bool IsOnCrawlTransition()
    {
        return isCrouchingDown || isStandingUp;
    }
    
    // --- FIM DAS NOVAS FUNÇÕES ---

    private void UpdateTimers() { if (!isGrounded) coyoteTimeCounter -= Time.deltaTime; }
    public void CutJump() { if (rb.linearVelocity.y > 0) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); } }

    public Vector2 GetWallEjectDirection() { return isTouchingWallLeft ? Vector2.right : Vector2.left; }
    public bool IsGrounded() { return isGrounded; }
    public bool IsWallSliding() { return isWallSliding; }
    public bool IsInParabolaArc() { return isInParabolaArc; }
    public float GetVerticalVelocity() { return rb.linearVelocity.y; }
    public bool IsMoving() { return Mathf.Abs(moveInput) > 0.1f; }
    public Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }
    public void SetGravityScale(float scale) { rb.gravityScale = scale; }
    public void SetVelocity(float x, float y) { rb.linearVelocity = new Vector2(x, y); }
    public bool CanJumpFromGround() { return coyoteTimeCounter > 0f; }
    public bool IsTouchingWall() { return isTouchingWallLeft || isTouchingWallRight; }
    public void OnDashStart()
    {
        // --- ADICIONADO: Bloqueio físico de dash ao rastejar ---
        if (isCrawling || isCrouchingDown || isStandingUp)
        {
            return;
        }
        // --- FIM DA ADIÇÃO ---

        isDashing = true;
    }
    public void OnDashEnd() { isDashing = false; }
    public void OnWallDashStart() { isWallDashing = true; }
    public void OnWallDashEnd() { isWallDashing = false; }
    public bool IsDashing() { return isDashing; }

    public bool IsWallDashing() { return isWallDashing; }
    public bool IsWallJumping() { return isWallJumping; }
    public bool IsJumping() { return isJumping; }

    public bool IsTakingDamage() { return isInKnockback; }
    public bool IsFacingRight() { return isFacingRight; }
    public Rigidbody2D GetRigidbody() { return rb; }

    private void UpdateDebugUI()
    {
        if (stateCheckText != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            // CORREÇÃO: A terceira linha estava checando IsWallSliding em vez de IsTouchingWall
            stateCheckText.text = $"Grounded: {CheckState(PlayerState.IsGrounded)}\n" +
                                  $"WallSliding: {CheckState(PlayerState.IsWallSliding)}\n" +
                                  $"TouchingWall: {CheckState(PlayerState.IsTouchingWall)}\n" + // <-- CORRIGIDO
                                  $"Dashing: {CheckState(PlayerState.IsDashing)}\n" +
                                  $"InParabola: {CheckState(PlayerState.IsInParabola)}\n" +
                                  $"--- VELOCIDADE ---\n" +
                                  $"X Speed: {currentVelocity.x:F2}\n" +
                                  $"Y Speed: {currentVelocity.y:F2}";
        }
    }
}