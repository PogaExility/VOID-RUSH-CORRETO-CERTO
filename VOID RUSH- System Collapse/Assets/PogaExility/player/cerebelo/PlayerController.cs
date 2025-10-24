using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(SkillRelease))]
public class PlayerController : MonoBehaviour
{

    [Header("Refer�ncias de Gerenciamento")]
    public CursorManager cursorManager;

    [Header("Refer�ncias de UI")]
    public GameObject inventoryPanel;
    public GameObject combatHUDPanel;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

    [Header("Refer�ncias de Movimento e Combate")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public DefenseHandler defenseHandler;
    public WeaponHandler weaponHandler;

    [Header("Skills de Movimento")]
    public SkillSO baseJumpSkill;
    public SkillSO baseDashSkill;
    public SkillSO dashJumpSkill;
    public SkillSO upgradedJumpSkill;
    public SkillSO upgradedDashSkill;
    public SkillSO wallSlideSkill;
    public SkillSO wallJumpSkill;
    public SkillSO wallDashSkill;
    public SkillSO wallDashJumpSkill;

    [Header("Plataformas")]
    [Tooltip("A camada (Layer) em que as plataformas 'one-way' se encontram.")]
    [SerializeField] private LayerMask platformLayer;
    [Tooltip("Por quanto tempo a colis�o ficar� desativada ao descer.")]
    [SerializeField] private float dropDuration = 0.25f;

    [Header("Escada")]
    [SerializeField] private LayerMask ladderLayer;

    // --- Vari�veis de estado para Escada ---
    private bool isOnLadder = false;
    private float verticalInput;

    [Header("Skills de Combate")]
    public SkillSO blockSkill;


    private bool attackBuffered = false;
    private bool isInventoryOpen = false;
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;
    private bool isInAimMode = false;
    private PlayerStats playerStats;
    public bool inventoryLocked = false;
    private PlayerAnimState previousBodyState;
    private bool isActionInterruptingAim = false;
    private bool _isAttacking;
    private ObjetoInterativo interagivelProximo;
    private bool _isIgnoringPlatforms = false;


    public bool IsAttacking

    {
        get { return _isAttacking; }
        set
        {
            _isAttacking = value;
            // Quando IsAttacking � setado para TRUE, desabilita a f�sica do movimento.
            // Quando � setado para FALSE, habilita a f�sica novamente.
            if (value)
            {
                movementScript.DisablePhysicsControl();
            }
            else
            {
                movementScript.EnablePhysicsControl();
            }
        }
    }
    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>();
        skillRelease = GetComponent<SkillRelease>();
        defenseHandler = GetComponent<DefenseHandler>();
        playerStats = GetComponent<PlayerStats>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        if (cursorManager == null) cursorManager = FindAnyObjectByType<CursorManager>();
        if (weaponHandler == null) weaponHandler = GetComponent<WeaponHandler>();
    }

    void Start()
    {
        if (energyBar != null) energyBar.SetMaxEnergy(100f);
        SetPowerMode(false);
        if (inventoryPanel != null) { inventoryPanel.SetActive(false); isInventoryOpen = false; }
        if (cursorManager != null) cursorManager.SetDefaultCursor();
        if (animatorController != null) animatorController.SetAimLayerWeight(0);
    }

    public void SetAimingState(bool isNowAiming)
    {
        // Se j� estamos no estado desejado, n�o faz nada para evitar loops.
        if (isInAimMode == isNowAiming) return;

        isInAimMode = isNowAiming;

        // Comanda os outros componentes
        movementScript.allowMovementFlip = !isNowAiming;
        animatorController.SetAimLayerWeight(isNowAiming ? 1f : 0f);
        weaponHandler.UpdateAimVisuals(isNowAiming);
    }
    public void SetAimingStateVisuals(bool isNowAiming)
    {
        movementScript.allowMovementFlip = !isNowAiming;
        animatorController.SetAimLayerWeight(isNowAiming ? 1f : 0f);
    }
    // DENTRO DE PlayerController.cs

    // O par�metro da fun��o agora � do tipo ObjetoInterativo.
    public void RegistrarInteragivel(ObjetoInterativo interagivel)
    {
        interagivelProximo = interagivel;
    }

    public void RemoverInteragivel(ObjetoInterativo interagivel)
    {
        // Apenas remove se for o mesmo interag�vel que est� registrado (evita bugs).
        if (interagivelProximo == interagivel)
        {
            interagivelProximo = null;
        }
    }

 
    private void HandleInteractionInput()
    {
        // Se a tecla E for pressionada E existe um interag�vel pr�ximo...
        if (Input.GetKeyDown(KeyCode.E) && interagivelProximo != null)
        {
            // ...chama a fun��o de intera��o do objeto.
            interagivelProximo.Interagir();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }
        if (isInventoryOpen) { movementScript.SetMoveInput(0); return; }

        if (IsAttacking)
        {
            movementScript.SetMoveInput(0);
            return;
        }

        // Captura de inputs gerais
        verticalInput = Input.GetAxisRaw("Vertical");
        movementScript.SetMoveInput(Input.GetAxisRaw("Horizontal"));

        // Lida com as mec�nicas de estado
        HandleCrawlInput();
        HandleClimbing();

        // Tenta descer da plataforma. Se conseguir, 'dropped' ser� verdadeiro.
        bool dropped = HandlePlatformDrop();

        // Lida com inputs de a��o
        HandleInteractionInput();
        HandlePowerModeToggle();

        // S� tenta pular/usar skills se N�O tiver acabado de descer de uma plataforma.
        if (!dropped)
        {
            HandleSkillInput();
        }

        HandleCombatInput();
        ProcessAttackBuffer();
        HandleWeaponSwitching();

        // Atualiza anima��es e estados de frame
        UpdateAnimations();

        bool isGroundedNow = movementScript.IsGrounded();
        if (isGroundedNow && !wasGroundedLastFrame)
        {
            isLanding = true;
        }
        wasGroundedLastFrame = isGroundedNow;

        if (Input.GetKeyUp(KeyCode.Space)) movementScript.CutJump();
    }
    // --- NOVAS FUN��ES PARA DESCER DE PLATAFORMAS ---


    private bool HandlePlatformDrop()
    {
        // Verifica se o jogador est� segurando 'S' E apertou o bot�o de Pulo neste frame.
        // Tamb�m verifica se est� no ch�o para evitar ativar isso no ar.
        if (Input.GetKey(KeyCode.S) && Input.GetButtonDown("Jump") && movementScript.IsGrounded())
        {
            StartCoroutine(PlatformDropRoutine());
            return true; // Indica que a a��o de descer foi realizada
        }
        return false; // Nenhuma a��o de descer foi realizada
    }

    private IEnumerator PlatformDropRoutine()
    {
        // Pega a layer do jogador e a layer das plataformas
        int playerLayer = gameObject.layer;
        int platformLayerIndex = LayerMask.NameToLayer("Plataforma");

        // --- ADICIONADO: Empurr�o para baixo ---
        // For�a uma pequena velocidade para baixo para garantir que o jogador se descole
        movementScript.SetVelocity(movementScript.GetRigidbody().linearVelocity.x, -1f);
        // --- FIM DA ADI��O ---

        // Desliga a colis�o entre elas
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayerIndex, true);

        // Espera um pouquinho para a gravidade puxar o jogador para baixo da plataforma
        yield return new WaitForSeconds(dropDuration);

        // Religa a colis�o
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayerIndex, false);
    }


    private void HandleCrawlInput()
    {
        // Pressionou para come�ar a rastejar
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // Condi��es para poder rastejar: no ch�o e n�o estar fazendo outra a��o
            if (movementScript.IsGrounded() && !movementScript.IsCrawling() && !movementScript.IsOnCrawlTransition())
            {
                // --- ADICIONADO: For�a a sa�da do modo de mira ---
                if (isInAimMode)
                {
                    SetAimingState(false);
                }
                // --- FIM DA ADI��O ---

                movementScript.BeginCrouchTransition();
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.abaixando);
            }
        }
        // Soltou a tecla para levantar
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (movementScript.IsCrawling())
            {
                // Garante que a velocidade do animator volte ao normal para a anima��o de "levantar"
                animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);
                movementScript.BeginStandUpTransition();
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.levantando);
            }
        }
    }

    /// <summary>
    /// Esta fun��o DEVE ser chamada por um Animation Event no �ltimo frame da sua anima��o "abaixando".
    /// </summary>
    public void OnCrouchDownAnimationComplete()
    {
        movementScript.CompleteCrouch();
    }

    /// <summary>
    /// Esta fun��o DEVE ser chamada por um Animation Event no �ltimo frame da sua anima��o "levantando".
    /// </summary>
    public void OnStandUpAnimationComplete()
    {
        movementScript.CompleteStandUp();
    }

    // --- FIM DAS NOVAS FUN��ES ---
  



    private Coroutine lungeCoroutine;

    // A fun��o agora aceita DIST�NCIA e VELOCIDADE.
    public void PerformLunge(float distance, float speed)
    {
        if (lungeCoroutine != null) StopCoroutine(lungeCoroutine);
        lungeCoroutine = StartCoroutine(LungeCoroutine(distance, speed));
    }

    public void CancelLunge()
    {
        if (lungeCoroutine != null)
        {
            StopCoroutine(lungeCoroutine);
            // A limpeza agora � feita pelo 'finally', mas garantimos aqui por seguran�a.
            movementScript.EnablePhysicsControl();
            lungeCoroutine = null;
        }
    }

    private IEnumerator LungeCoroutine(float distance, float speed)
    {
        // Se a velocidade for muito baixa, n�o faz nada para evitar erros.
        if (Mathf.Abs(speed) < 0.1f)
        {
            yield break;
        }

        Rigidbody2D rb = movementScript.GetRigidbody();

        try
        {
            // --- FASE DE EXECU��O ---
            movementScript.DisablePhysicsControl();

            // Calcula a dura��o com base na dist�ncia e velocidade.
            float duration = Mathf.Abs(distance / speed);

            // Determina a dire��o do lunge (para frente ou para tr�s).
            float lungeDirection = Mathf.Sign(distance);
            float finalDirectionX = movementScript.GetFacingDirection().x * lungeDirection;

            float horizontalVelocity = finalDirectionX * speed;

            // Loop que for�a a velocidade a cada frame, mas preserva a gravidade.
            float timeElapsed = 0f;
            while (timeElapsed < duration)
            {
                float currentVerticalSpeed = rb.linearVelocity.y;
                rb.linearVelocity = new Vector2(horizontalVelocity, currentVerticalSpeed);

                timeElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
        finally
        {
            // --- FASE DE LIMPEZA (GARANTIDA) ---
            // Este bloco SEMPRE roda, mesmo se a corrotina for interrompida.
            movementScript.SetVelocity(0, rb.linearVelocity.y);
            movementScript.EnablePhysicsControl();
            lungeCoroutine = null;
        }
    }

    private void HandleWeaponSwitching()
    {
        // ADICIONE ESTA LINHA NO TOPO DA FUN��O
        // Impede a troca de armas durante a recarga.
        if (weaponHandler.IsReloading) return;

        if (isInventoryOpen || weaponHandler == null) return;
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0f)
        {
            weaponHandler.CycleWeapon();
        }
        else if (scrollInput < 0f)
        {
            weaponHandler.CycleWeapon();
        }
    }
    private void ToggleInventory()
    {
        if (inventoryLocked) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        if (combatHUDPanel != null)
            combatHUDPanel.SetActive(!isInventoryOpen);

        Time.timeScale = isInventoryOpen ? 0f : 1f;

        if (cursorManager != null)
        {
            if (isInventoryOpen)
                cursorManager.SetInventoryCursor();
            else
                cursorManager.SetDefaultCursor();
        }
    }

    private void HandleSkillInput()
    {
        // --- CORRE��O DEFINITIVA: Bloqueia a TENTATIVA de ativar skills ---
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition())
        {
            return;
        }
        // --- FIM DA CORRE��O ---

        // A fun��o agora s� tenta ativar skills se o bloqueio acima for passado.
        TryActivateMovementSkills();
    }

    // Crie esta nova fun��o auxiliar
    private bool TryActivateMovementSkills()
    {
        if (skillRelease.TryActivateSkill(wallDashJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(dashJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(wallJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(wallDashSkill)) return true;
        if (skillRelease.TryActivateSkill(wallSlideSkill)) return true;
        if (skillRelease.TryActivateSkill(activeJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(activeDashSkill)) return true;
        return false;
    }



    // DENTRO DE PlayerController.cs

    private void HandleCombatInput()
    {
        // --- ADICIONADO: Bloqueia combate ao rastejar ---
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition()) return;
        // --- FIM DA ADI��O ---

        if (weaponHandler.IsReloading) return;

        var activeWeapon = weaponHandler.GetActiveWeaponSlot()?.item;
        if (activeWeapon == null) return;

        if (activeWeapon.weaponType == WeaponType.Meelee)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                attackBuffered = true;
            }
        }
        else
        {
            if (Input.GetButton("Fire1"))
            {
                attackBuffered = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponHandler.HandleReloadInput();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            defenseHandler.StartBlock(blockSkill);
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            defenseHandler.EndBlock();
        }
    }

    private void ProcessAttackBuffer()
    {
        // --- ADICIONADO: Bloqueia ataque ao rastejar ---
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition())
        {
            attackBuffered = false; // Limpa o buffer se o jogador tentar rastejar
            return;
        }
        // --- FIM DA ADI��O ---

        if (!attackBuffered)
        {
            return;
        }

        bool canAttackNow = !IsAttacking &&
                            !movementScript.IsDashing() &&
                            weaponHandler.IsWeaponObjectActive();

        if (canAttackNow)
        {
            weaponHandler.HandleAttackInput();
            attackBuffered = false;
        }
    }

    private void UpdateAnimations()
    {
        // Trava de ataque meelee
        if (IsAttacking) return;

        // --- L�GICA DE ANIMA��O DE ESCADA (PRIORIDADE M�XIMA) ---
        if (movementScript.IsClimbing())
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.subindoEscada);
            // Controla a velocidade da anima��o para subir, descer ou parar
            animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, verticalInput);
            return;
        }
        else
        {
            // Garante que a velocidade do animator volte ao normal ao sair da escada
            animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);
        }

        // --- L�GICA DE ANIMA��O DE RASTEJAR (PRIORIDADE ALTA) ---
        if (movementScript.IsOnCrawlTransition())
        {
            return;
        }

        if (movementScript.IsCrawling())
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.rastejando);

            if (movementScript.IsMoving())
            {
                animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);
            }
            else
            {
                animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 0f);
            }
            return;
        }

        // --- L�GICA DE ANIMA��O NORMAL ---
        PlayerAnimState desiredState;

        if (playerStats.IsDead()) { desiredState = PlayerAnimState.morrendo; }
        else if (isLanding && !isInAimMode) { desiredState = PlayerAnimState.pousando; }
        else if (animatorController.GetCurrentAnimatorStateInfo(AnimatorTarget.PlayerBody, 0).IsName("dano")) { desiredState = PlayerAnimState.dano; }
        else if (!movementScript.IsGrounded())
        {
            if (movementScript.IsWallSliding()) desiredState = PlayerAnimState.derrapagem;
            else if (movementScript.IsDashing() || movementScript.IsWallDashing()) desiredState = PlayerAnimState.dashAereo;
            else if (movementScript.GetVerticalVelocity() > 0.1f) desiredState = PlayerAnimState.pulando;
            else desiredState = PlayerAnimState.falling;
        }
        else // No ch�o
        {
            if (movementScript.IsDashing()) desiredState = PlayerAnimState.dash;
            else if (movementScript.IsMoving()) desiredState = PlayerAnimState.andando;
            else
            {
                if (playerStats.IsHealthLow()) desiredState = PlayerAnimState.poucaVidaParado;
                else desiredState = PlayerAnimState.parado;
            }
        }

        bool isDesiredStateAnAction = IsActionState(desiredState);

        if (isDesiredStateAnAction)
        {
            isActionInterruptingAim = true;
            SetAimingState(false);
        }
        else
        {
            if (isActionInterruptingAim)
            {
                isActionInterruptingAim = false;
                if (weaponHandler.IsAimWeaponEquipped())
                {
                    SetAimingState(true);
                }
            }
        }


        if (isInAimMode)
        {
            if (!movementScript.IsGrounded())
            {
                if (movementScript.GetVerticalVelocity() > 0.1f)
                    animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.pulandoCotoco, 1);
                else
                    animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.fallingCotoco, 1);
            }
            else if (movementScript.IsMoving())
            {
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.andarCotoco, 1);
            }
            else
            {
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.paradoCotoco, 1);
            }
        }
        else
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, desiredState, 0);
        }
    }
    private bool IsActionState(PlayerAnimState state)
    {
        switch (state)
        {
            case PlayerAnimState.dash:
            case PlayerAnimState.dashAereo:
            case PlayerAnimState.pousando:
            case PlayerAnimState.derrapagem:
            case PlayerAnimState.dano:
            case PlayerAnimState.flip:
            case PlayerAnimState.block:
            case PlayerAnimState.parry:
            case PlayerAnimState.morrendo:
                return true;
            default:
                return false;
        }
    }

    public void OnActionAnimationComplete()
    {
        // Pergunta para o WeaponHandler se a arma atual AINDA � uma arma de mira.
        if (weaponHandler.IsAimWeaponEquipped())
        {
            // Se for, REATIVA o modo de mira.
            SetAimingState(true);
        }
    }
    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetPowerMode(!isPowerModeActive);
        }
    }

    private void SetPowerMode(bool isActive)
    {
        isPowerModeActive = isActive;
        activeJumpSkill = isPowerModeActive ? upgradedJumpSkill : baseJumpSkill;
        activeDashSkill = isPowerModeActive ? upgradedDashSkill : baseDashSkill;
        if (powerModeIndicator != null)
        {
            powerModeIndicator.SetActive(isActive);
        }
        Debug.Log("Power Mode Ativo: " + isPowerModeActive);
    }

    public SkillSO GetActiveJumpSkill()
    {
        return activeJumpSkill;
    }
    public void OnLandingAnimationEnd()
    {
        Debug.Log("Anima��o de pouso TERMINOU. Liberando o jogador.");
        isLanding = false;
        movementScript.OnLandingComplete();
    }
    // --- NOVAS FUN��ES PARA MEC�NICA DE ESCADA ---

    private void HandleClimbing()
    {
        // Se o jogador est� em contato com a escada e pressiona para cima ou para baixo
        if (isOnLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            movementScript.StartClimbing();
        }

        // Se o jogador est� no estado de escalada, aplica o movimento
        if (movementScript.IsClimbing())
        {
            movementScript.Climb(verticalInput);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto com o qual colidiu est� na camada da escada
        if ((ladderLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isOnLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Verifica se o objeto que est� deixando est� na camada da escada
        if ((ladderLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isOnLadder = false;
            movementScript.StopClimbing();
        }
    }

    // --- FIM DAS NOVAS FUN��ES ---
}