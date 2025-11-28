using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{
    #region 1. Referências e Configurações
    [Header("Referências")]
    public PlayerAnimatorController animatorController;
    public PlayerController playerController;
    public Camera mainCamera;
    public TextMeshProUGUI stateCheckText;
    public LayerMask collisionLayer;
    [Tooltip("Defina aqui qual layer é usada para as plataformas atravessáveis.")]
    public LayerMask platformLayer;

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
    // (Vazio no original, mantido vazio)

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

    [Header("Rastejar")]
    public float crawlSpeed = 4f;

    [Header("Física de Dano")]
    // (Vazio no original, mantido vazio)

    [Header("Escalada")]
    [SerializeField] private float climbingSpeed = 5f;
    [SerializeField] private LayerMask ladderLayer;

    [Header("Efeitos Sonoros")]
    [SerializeField] private GameObject soundEmitterPrefab;
    #endregion

    #region 2. Variáveis Privadas e Controle
    // --- Variáveis para o sistema de plataforma ---
    private int playerLayer;
    private int platformLayerInt;
    private bool isIgnoringPlatformsTemporarily = false;

    private bool physicsControlDisabled = false;
    private float currentGravityScaleOnFall;

    // --- Variáveis de estado para o Rastejar ---
    private bool isCrawling = false;
    private bool isCrouchingDown = false;
    private bool isStandingUp = false;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    private DefenseHandler defenseHandler;

    // Componentes e Inputs
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private float moveInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private float coyoteTimeCounter;

    // Estados de Parede e Movimento
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
    private bool isWallDashing = false;
    private bool isIgnoringSteeringInput = false;

    // Corrotinas
    private Coroutine knockbackCoroutine;
    private Coroutine steeringGraceCoroutine;

    // Colisões e Filtros
    private Collider2D groundCollider;
    private ContactFilter2D platformContactFilter;
    private Collider2D[] overlapResults = new Collider2D[1];

    // Escalada
    private bool isClimbing = false;
    private float verticalInput;
    private bool isInLadderZone = false;

    // Som
    private PlayerSounds playerSounds;
    #endregion

    #region 3. Ciclo de Vida (Unity)
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        playerSounds = GetComponent<PlayerSounds>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        currentGravityScaleOnFall = gravityScaleOnFall;
        defenseHandler = GetComponent<DefenseHandler>();

        // --- ADICIONADO: Salva as dimensões originais do collider ---
        originalColliderSize = capsuleCollider.size;
        originalColliderOffset = capsuleCollider.offset;

        // --- LÓGICA DA PLATAFORMA ---
        playerLayer = gameObject.layer;
        platformLayerInt = LayerMask.NameToLayer("Plataforma");

        // --- NOVA CONFIGURAÇÃO ---
        platformContactFilter.SetLayerMask(platformLayer);
    }

    void Update()
    {
        // Pega o input vertical no início do Update para ser usado por outras funções.
        verticalInput = Input.GetAxisRaw("Vertical");

        if (physicsControlDisabled)
        {
            UpdateTimers();
            return;
        }

        // --- NOVA LÓGICA DE ESCALADA ---
        HandleClimbingInput();

        HandlePlatformDropInput();
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
            return;
        }

        // A lógica agora é dividida: ou o jogador está escalando, ou está em movimento normal.
        if (isClimbing)
        {
            HandleClimbingMovement();
        }
        else
        {
            if (isWallSliding && (!IsTouchingWall() || IsGrounded()))
            {
                StopWallSlide();
            }

            CheckCollisions();
            HandlePlatformPassthrough();
            HandleMovement();
            HandleGravity();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se o colisor que entramos pertence à layer de escada...
        if ((ladderLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isInLadderZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Se o colisor que saímos pertence à layer de escada...
        if ((ladderLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isInLadderZone = false;
            // Forçamos a saída do modo de escalada por segurança.
            isClimbing = false;
        }
    }
    #endregion

    #region 4. Lógica de Movimentação (Chão)
    public void SetMoveInput(float input)
    {
        moveInput = input;
    }

    private void HandleMovement()
    {
        if (playerController != null && playerController.IsAttacking)
        {
            return;
        }

        // --- ADICIONADO: Bloqueia movimento durante transições de rastejar e outros estados ---
        if (isDashing || isWallDashing || isWallJumping || isWallSliding || isInKnockback || isCrouchingDown || isStandingUp)
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

        // --- MODIFICADO: Usa crawlSpeed se estiver rastejando ---
        float currentSpeed = isCrawling ? crawlSpeed : moveSpeed;
        float targetSpeed = moveInput * currentSpeed;

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        rb.AddForce(speedDiff * accelRate * Vector2.right);
    }

    private void HandleGravity()
    {
        if (isWallSliding)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            // Restaura a gravidade correta (base ou de queda).
            rb.gravityScale = rb.linearVelocity.y < 0 ? currentGravityScaleOnFall : baseGravity;
        }
    }

    public void Freeze() { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
    public void Unfreeze() { rb.bodyType = RigidbodyType2D.Dynamic; }
    #endregion

    #region 5. Lógica de Pulo
    public void DoJump(float multiplier)
    {
        // --- ADICIONADO: Bloqueio físico de pulo ao rastejar ---
        if (isCrawling || isCrouchingDown || isStandingUp)
        {
            return;
        }

        if (isInParabolaArc)
        {
            return;
        }

        isJumping = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * multiplier);

        // --- LÓGICA DE SOM CORRIGIDA ---
        if (AudioManager.Instance != null && playerSounds != null && playerSounds.jumpSound != null)
        {
            AudioManager.Instance.PlaySoundEffect(playerSounds.jumpSound, transform.position);
        }

        if (soundEmitterPrefab != null)
        {
            Instantiate(soundEmitterPrefab, transform.position, Quaternion.identity);
        }
    }

    public void CutJump() { if (rb.linearVelocity.y > 0) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); } }
    public bool CanJumpFromGround() { return coyoteTimeCounter > 0f; }
    #endregion

    #region 6. Lógica de Parede (Wall)
    public void StartWallSlide(float speed)
    {
        if (!IsTouchingWall() || isGrounded) return;
        isWallSliding = true;
        currentWallSlideSpeed = speed;

        if (isTouchingWallRight && isFacingRight) Flip();
        else if (isTouchingWallLeft && !isFacingRight) Flip();
    }

    public void StopWallSlide()
    {
        isWallSliding = false;
        rb.gravityScale = baseGravity;
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

    public Vector2 GetWallEjectDirection() { return isTouchingWallLeft ? Vector2.right : Vector2.left; }
    public bool IsTouchingWall() { return isTouchingWallLeft || isTouchingWallRight; }
    public bool IsWallSliding() { return isWallSliding; }
    public bool IsWallJumping() { return isWallJumping; }
    #endregion

    #region 7. Lógica de Dash
    public void OnDashStart()
    {
        // --- ADICIONADO: Bloqueio físico de dash ao rastejar ---
        if (isCrawling || isCrouchingDown || isStandingUp)
        {
            return;
        }

        isDashing = true;

        // --- LÓGICA DE SOM DE DASH ---
        if (AudioManager.Instance != null && playerSounds != null && playerSounds.dashSound != null)
        {
            AudioManager.Instance.PlaySoundEffect(playerSounds.dashSound, transform.position);
        }

        if (soundEmitterPrefab != null)
        {
            Instantiate(soundEmitterPrefab, transform.position, Quaternion.identity);
        }
    }

    public void OnDashEnd() { isDashing = false; }

    public void OnWallDashStart()
    {
        isWallDashing = true;

        // --- LÓGICA DE SOM DE DASH (REUTILIZADA) ---
        if (AudioManager.Instance != null && playerSounds != null && playerSounds.dashSound != null)
        {
            AudioManager.Instance.PlaySoundEffect(playerSounds.dashSound, transform.position);
        }

        if (soundEmitterPrefab != null)
        {
            Instantiate(soundEmitterPrefab, transform.position, Quaternion.identity);
        }
    }

    public void OnWallDashEnd() { isWallDashing = false; }
    public bool IsDashing() { return isDashing; }
    public bool IsWallDashing() { return isWallDashing; }
    #endregion

    #region 8. Sistema de Escalada (Ladder)
    private void HandleClimbingInput()
    {
        // A condição para começar a escalar agora usa a nova variável.
        if (isInLadderZone && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }

        // CONDIÇÃO PARA SAIR DA ESCALADA (PULANDO)
        if (isClimbing && Input.GetButtonDown("Jump"))
        {
            isClimbing = false;
            // --- ADIÇÃO PRINCIPAL AQUI ---
            DoJump(1f);
        }
    }

    private void HandleClimbingMovement()
    {
        // A checagem de segurança agora também usa a nova variável.
        if (!isInLadderZone)
        {
            isClimbing = false;
            return;
        }

        // O resto da lógica de movimento permanece o mesmo.
        rb.gravityScale = 0f;
        float horizontalSpeed = moveInput * moveSpeed;
        float verticalSpeed = verticalInput * climbingSpeed;
        rb.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);
    }

    public bool IsClimbing()
    {
        return isClimbing;
    }

    public float GetVerticalInput()
    {
        return verticalInput;
    }
    #endregion

    #region 9. Sistema de Rastejar (Crawl)
    public void BeginCrouchTransition()
    {
        // 1. Define o estado de transição (Bloqueia o input de andar no HandleMovement)
        isCrouchingDown = true;

        // 2. Zera a velocidade horizontal para ele não "deslizar" enquanto abaixa.
        // IMPORTANTE: Mantemos a velocidade Y para a gravidade continuar agindo.
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // 3. Força a animação a tocar diretamente aqui para garantir
        if (animatorController != null)
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.abaixando);
        }
    }

    /// <summary>
    /// ATENÇÃO: Esta função DEVE ser chamada por um Animation Event no final da animação "abaixando".
    /// Se o evento não estiver configurado na Unity, o player vai travar.
    /// </summary>
    public void CompleteCrouch()
    {
        isCrouchingDown = false; // Libera a trava de transição
        isCrawling = true;       // Ativa o estado de rastejar

        // Ajusta o Collider para metade do tamanho
        float newHeight = originalColliderSize.y / 2f;
        capsuleCollider.size = new Vector2(originalColliderSize.x, newHeight);

        // Ajusta o Offset para o pé continuar no chão (baixa o centro do collider)
        // Cálculo: Offset Original - (1/4 da altura original)
        float newOffsetY = originalColliderOffset.y - (originalColliderSize.y / 4f);
        capsuleCollider.offset = new Vector2(originalColliderOffset.x, newOffsetY);
    }

    public void BeginStandUpTransition()
    {
        isStandingUp = true; // Bloqueia movimento

        // Zera velocidade horizontal, mantém gravidade
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Força a animação de levantar
        if (animatorController != null)
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.levantando);
        }
    }

    /// <summary>
    /// ATENÇÃO: Esta função DEVE ser chamada por um Animation Event no final da animação "levantando".
    /// </summary>
    public void CompleteStandUp()
    {
        isStandingUp = false; // Libera movimento
        isCrawling = false;   // Sai do estado de rastejar

        // Restaura o collider ao tamanho e posição originais
        capsuleCollider.size = originalColliderSize;
        capsuleCollider.offset = originalColliderOffset;
    }

    public bool IsCrawling() { return isCrawling; }

    // Esta função é usada pelo UpdateAnimations para não interromper a animação de transição
    public bool IsOnCrawlTransition() { return isCrouchingDown || isStandingUp; }
    #endregion

    #region 10. Sistema de Plataformas
    public bool IsIgnoringPlatforms()
    {
        bool isJumpingUp = rb.linearVelocity.y > 0.01f;
        return isJumpingUp || Input.GetKey(KeyCode.S) || isIgnoringPlatformsTemporarily;
    }

    private void HandlePlatformPassthrough()
    {
        // PRIORIDADE 1: Se o período de carência temporário estiver ativo.
        if (isIgnoringPlatformsTemporarily)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayerInt, true);
            return;
        }

        // PRIORIDADE 2: Se o jogador estiver segurando 'S'.
        if (Input.GetKey(KeyCode.S))
        {
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayerInt, true);
            return;
        }

        // PRIORIDADE 3 (COMPORTAMENTO PADRÃO).
        bool isJumpingUp = rb.linearVelocity.y > 0.01f;
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayerInt, isJumpingUp);
    }

    private void HandlePlatformDropInput()
    {
        if (Input.GetKeyDown(KeyCode.S) && isGrounded && !isIgnoringPlatformsTemporarily)
        {
            if (groundCollider != null && (platformLayer.value & (1 << groundCollider.gameObject.layer)) > 0)
            {
                StartCoroutine(DropDownCoroutine());
            }
        }
    }

    private IEnumerator DropDownCoroutine()
    {
        isIgnoringPlatformsTemporarily = true;

        yield return new WaitForFixedUpdate();

        while (capsuleCollider.Overlap(platformContactFilter, overlapResults) > 0)
        {
            yield return new WaitForFixedUpdate();
        }

        isIgnoringPlatformsTemporarily = false;
    }
    #endregion

    #region 11. Física Avançada (Parábola e Knockback)
    public void OnParabolaStart(float damping)
    {
        isInParabolaArc = true;
        rb.linearDamping = damping;
    }

    public void OnParabolaEnd()
    {
        isInParabolaArc = false;
        currentGravityScaleOnFall = gravityScaleOnFall;
        rb.linearDamping = 0f;
    }

    public void DoLaunch(float horizontalForce, float verticalForce, Vector2 direction, float customGravityOnFall, float damping)
    {
        isInParabolaArc = true;
        rb.linearVelocity = new Vector2(direction.x * horizontalForce, verticalForce);
        currentGravityScaleOnFall = customGravityOnFall;
        rb.linearDamping = damping;
    }

    public void DoWallLaunch(float horizontalForce, float verticalForce, float customGravityOnFall, float damping)
    {
        StopWallSlide();
        isInParabolaArc = true;

        Vector2 ejectDirection = GetFacingDirection();
        rb.linearVelocity = new Vector2(ejectDirection.x * horizontalForce, verticalForce);

        currentGravityScaleOnFall = customGravityOnFall;
        rb.linearDamping = damping;

        if (steeringGraceCoroutine != null)
        {
            StopCoroutine(steeringGraceCoroutine);
        }
        steeringGraceCoroutine = StartCoroutine(SteeringGracePeriodCoroutine(0.2f));
    }

    public void ExecuteKnockback(float force, Vector2 attackDirection, float upwardModifier = 0.5f, float duration = 0.2f)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }
        knockbackCoroutine = StartCoroutine(ExecuteKnockbackCoroutine(force, attackDirection, upwardModifier, duration));
    }

    private IEnumerator ExecuteKnockbackCoroutine(float force, Vector2 direction, float upwardModifier, float duration)
    {
        isInKnockback = true;

        try
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            Vector2 finalDirection = new Vector2(direction.x, direction.y + upwardModifier).normalized;
            rb.AddForce(finalDirection * force, ForceMode2D.Impulse);
            yield return new WaitForSeconds(duration);
        }
        finally
        {
            isInKnockback = false;
            knockbackCoroutine = null;
        }
    }

    private IEnumerator SteeringGracePeriodCoroutine(float duration)
    {
        isIgnoringSteeringInput = true;
        yield return new WaitForSeconds(duration);
        isIgnoringSteeringInput = false;
        steeringGraceCoroutine = null;
    }

    public void OnLandingStart()
    {
        isLanding = true;
        physicsControlDisabled = true;
    }

    public void OnLandingComplete()
    {
        isLanding = false;
        physicsControlDisabled = false;
    }

    public bool IsInParabolaArc() { return isInParabolaArc; }
    #endregion

    #region 12. Visual e Debug
    public void Flip()
    {
        // A trava "if (!allowMovementFlip) return;" foi REMOVIDA daqui.
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
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

    public void FaceTowards(Vector3 worldPoint)
    {
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

    private void HandleFlipLogic()
    {
        if (!allowMovementFlip) return;

        if (moveInput > 0.01f && !isFacingRight) Flip();
        else if (moveInput < -0.01f && isFacingRight) Flip();
    }

    private void UpdateTimers() { if (!isGrounded) coyoteTimeCounter -= Time.deltaTime; }

    public void DisablePhysicsControl() { physicsControlDisabled = true; }
    public void EnablePhysicsControl() { physicsControlDisabled = false; }

    public Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }

    public bool IsFacingRight() { return isFacingRight; }

    private void UpdateDebugUI()
    {
        if (stateCheckText != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            stateCheckText.text = $"Grounded: {CheckState(PlayerState.IsGrounded)}\n" +
                                  $"WallSliding: {CheckState(PlayerState.IsWallSliding)}\n" +
                                  $"TouchingWall: {CheckState(PlayerState.IsTouchingWall)}\n" +
                                  $"Dashing: {CheckState(PlayerState.IsDashing)}\n" +
                                  $"InParabola: {CheckState(PlayerState.IsInParabola)}\n" +
                                  $"--- VELOCIDADE ---\n" +
                                  $"X Speed: {currentVelocity.x:F2}\n" +
                                  $"Y Speed: {currentVelocity.y:F2}";
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
            case PlayerState.IsWallDashing: return IsWallDashing();
            case PlayerState.IsTakingDamage: return IsTakingDamage();

            default: return false;
        }
    }
    #endregion

    #region 13. Checagens de Física (Colisões)
    public Collider2D GetGroundCollider()
    {
        return groundCollider;
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

    private bool PerformGroundCheck()
    {
        float raycastDistance = 0.1f;
        Vector2 capsuleCenter = (Vector2)transform.position + capsuleCollider.offset;
        float halfWidth = (capsuleCollider.size.x / 2f) - 0.05f;
        Vector2 leftOrigin = new Vector2(capsuleCenter.x - halfWidth, capsuleCenter.y);
        Vector2 centerOrigin = capsuleCenter;
        Vector2 rightOrigin = new Vector2(capsuleCenter.x + halfWidth, capsuleCenter.y);

        // Dispara os três raios para baixo, como antes.
        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);
        RaycastHit2D hitCenter = Physics2D.Raycast(centerOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, capsuleCollider.size.y / 2f + raycastDistance, collisionLayer);

        if (hitLeft.collider != null)
        {
            groundCollider = hitLeft.collider;
            return true;
        }
        if (hitCenter.collider != null)
        {
            groundCollider = hitCenter.collider;
            return true;
        }
        if (hitRight.collider != null)
        {
            groundCollider = hitRight.collider;
            return true;
        }

        groundCollider = null;
        return false;
    }

    public bool IsGrounded() { return isGrounded; }
    public void SetGravityScale(float scale) { rb.gravityScale = scale; }
    public void SetVelocity(float x, float y) { rb.linearVelocity = new Vector2(x, y); }
    public float GetVerticalVelocity() { return rb.linearVelocity.y; }
    public bool IsMoving() { return Mathf.Abs(moveInput) > 0.1f; }
    public bool IsTakingDamage() { return isInKnockback; }
    public bool IsJumping() { return isJumping; }
    public Rigidbody2D GetRigidbody() { return rb; }
    #endregion
}