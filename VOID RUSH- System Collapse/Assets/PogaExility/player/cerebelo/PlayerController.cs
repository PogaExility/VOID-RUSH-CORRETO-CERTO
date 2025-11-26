


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(SkillRelease))]
public class PlayerController : MonoBehaviour
{
    [Header("Referências de Gerenciamento")]
    public CursorManager cursorManager;

    [Header("Referências de UI")]
    public GameObject inventoryPanel;
    public GameObject combatHUDPanel;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

    [Header("Referências de Movimento e Combate")]
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
    private PlayerSounds playerSounds;
    public void PlayFootstepSound()
    {
        // Se as referências não existirem, sai para evitar erros.
        if (AudioManager.Instance == null || playerSounds == null) return;

        // Pega um som de passo aleatório da nossa "mochila de sons".
        AudioClip footstepClip = playerSounds.GetRandomFootstep();

        // Se encontrou um clipe, toca ele.
        if (footstepClip != null)
        {
            // Toca o som audível para o jogador.
            AudioManager.Instance.PlaySoundEffect(footstepClip, transform.position);
        }

        // A lógica para o SoundEmitter para a IA ainda não foi adicionada,
        // mas quando formos fazer, será aqui.
    }

    public bool IsAttacking

    {
        get { return _isAttacking; }
        set
        {
            _isAttacking = value;
            // Quando IsAttacking é setado para TRUE, desabilita a física do movimento.
            // Quando é setado para FALSE, habilita a física novamente.
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
        playerSounds = GetComponent<PlayerSounds>(); // <-- ADICIONE ESTA LINHA
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
        // Se já estamos no estado desejado, não faz nada para evitar loops.
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

    // O parâmetro da função agora é do tipo ObjetoInterativo.
    public void RegistrarInteragivel(ObjetoInterativo interagivel)
    {
        interagivelProximo = interagivel;
    }

    public void RemoverInteragivel(ObjetoInterativo interagivel)
    {
        // Apenas remove se for o mesmo interagível que está registrado (evita bugs).
        if (interagivelProximo == interagivel)
        {
            interagivelProximo = null;
        }
    }


    private void HandleInteractionInput()
    {
        // Se a tecla E for pressionada E existe um interagível próximo...
        if (Input.GetKeyDown(KeyCode.E) && interagivelProximo != null)
        {
            // ...chama a função de interação do objeto.
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

        // --- ADIÇÃO ---
        // A lógica de rastejar é tratada primeiro para ter prioridade.
        HandleCrawlInput();
        // --- FIM DA ADIÇÃO ---

        // O resto da lógica de Update só roda se não estiver atacando ou rastejando.
        // A trava de movimento já está dentro de cada função Handle.
        movementScript.SetMoveInput(Input.GetAxisRaw("Horizontal"));

        bool isGroundedNow = movementScript.IsGrounded();
        bool justLanded = isGroundedNow && !wasGroundedLastFrame;

        // --- LÓGICA DE POUSO CORRIGIDA E FINAL ---
        if (justLanded)
        {
            Collider2D ground = movementScript.GetGroundCollider();
            bool landedOnPlatform = false;

            if (ground != null && (movementScript.platformLayer.value & (1 << ground.gameObject.layer)) > 0)
            {
                landedOnPlatform = true;
            }

            if (!landedOnPlatform || !movementScript.IsIgnoringPlatforms())
            {
                isLanding = true;

                // --- ADIÇÃO DA LÓGICA DE SOM DE POUSO ---
                if (AudioManager.Instance != null && playerSounds != null && playerSounds.landSound != null)
                {
                    AudioManager.Instance.PlaySoundEffect(playerSounds.landSound, transform.position);

                    // AINDA NÃO VAMOS CRIAR O EMISSOR DE SOM PARA O POUSO.
                    // Podemos adicionar isso depois, se necessário.
                }
            }
        }

        HandleInteractionInput();
        HandlePowerModeToggle();
        HandleSkillInput();
        HandleCombatInput();
        ProcessAttackBuffer();
        HandleWeaponSwitching();
        UpdateAnimations();

        wasGroundedLastFrame = isGroundedNow;

        if (Input.GetKeyUp(KeyCode.Space)) movementScript.CutJump();
    }
    private void HandleCrawlInput()
    {
        // Pressionou para começar a rastejar
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // Condições para poder rastejar: no chão e não estar fazendo outra ação
            if (movementScript.IsGrounded() && !movementScript.IsCrawling() && !movementScript.IsOnCrawlTransition())
            {
                // --- ADICIONADO: Força a saída do modo de mira ---
                if (isInAimMode)
                {
                    SetAimingState(false);
                }
                // --- FIM DA ADIÇÃO ---

                movementScript.BeginCrouchTransition();
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.abaixando);
            }
        }
        // Soltou a tecla para levantar
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (movementScript.IsCrawling())
            {
                // Garante que a velocidade do animator volte ao normal para a animação de "levantar"
                animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);
                movementScript.BeginStandUpTransition();
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.levantando);
            }
        }
    }

    /// <summary>
    /// Esta função DEVE ser chamada por um Animation Event no último frame da sua animação "abaixando".
    /// </summary>
    public void OnCrouchDownAnimationComplete()
    {
        movementScript.CompleteCrouch();
    }

    /// <summary>
    /// Esta função DEVE ser chamada por um Animation Event no último frame da sua animação "levantando".
    /// </summary>
    public void OnStandUpAnimationComplete()
    {
        movementScript.CompleteStandUp();
    }

    // --- FIM DAS NOVAS FUNÇÕES ---




    private Coroutine lungeCoroutine;

    // A função agora aceita DISTÂNCIA e VELOCIDADE.
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
            // A limpeza agora é feita pelo 'finally', mas garantimos aqui por segurança.
            movementScript.EnablePhysicsControl();
            lungeCoroutine = null;
        }
    }

    private IEnumerator LungeCoroutine(float distance, float speed)
    {
        // Se a velocidade for muito baixa, não faz nada para evitar erros.
        if (Mathf.Abs(speed) < 0.1f)
        {
            yield break;
        }

        Rigidbody2D rb = movementScript.GetRigidbody();

        try
        {
            // --- FASE DE EXECUÇÃO ---
            movementScript.DisablePhysicsControl();

            // Calcula a duração com base na distância e velocidade.
            float duration = Mathf.Abs(distance / speed);

            // Determina a direção do lunge (para frente ou para trás).
            float lungeDirection = Mathf.Sign(distance);
            float finalDirectionX = movementScript.GetFacingDirection().x * lungeDirection;

            float horizontalVelocity = finalDirectionX * speed;

            // Loop que força a velocidade a cada frame, mas preserva a gravidade.
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
        // ADICIONE ESTA LINHA NO TOPO DA FUNÇÃO
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
        // --- CORREÇÃO DEFINITIVA: Bloqueia a TENTATIVA de ativar skills ---
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition())
        {
            return;
        }
        // --- FIM DA CORREÇÃO ---

        // A função agora só tenta ativar skills se o bloqueio acima for passado.
        TryActivateMovementSkills();
    }

    // Crie esta nova função auxiliar
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
        // --- FIM DA ADIÇÃO ---

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
        // --- FIM DA ADIÇÃO ---

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

        // --- LÓGICA DE ANIMAÇÃO DE ESCALADA (COM ANIMAÇÕES SEPARADAS) ---
        if (movementScript.IsClimbing())
        {
            float verticalInput = movementScript.GetVerticalInput();

            // Se o jogador está se movendo na escada
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                // Garante que a velocidade da animação seja normal.
                animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);

                // Escolhe a animação correta com base na direção.
                if (verticalInput > 0)
                {
                    animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.subindoEscada);
                }
                else
                {
                    animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.descendoEscada);
                }
            }
            else // Se o jogador está parado na escada
            {
                // Pausa a animação no frame atual.
                animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 0f);
            }

            return; // Sai da função para não executar as outras lógicas.
        }
        else
        {
            // Garante que a velocidade do animator volte ao normal quando não estiver escalando.
            animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);
        }

        // --- LÓGICA DE ANIMAÇÃO DE RASTEJAR (PRIORIDADE 2) ---
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

        // --- LÓGICA DE ANIMAÇÃO NORMAL ---
        PlayerAnimState desiredState;

        if (playerStats.IsDead()) { desiredState = PlayerAnimState.morrendo; }
        else if (isLanding && !isInAimMode) { desiredState = PlayerAnimState.pousando; }
        else if (animatorController.GetCurrentAnimatorStateInfo(AnimatorTarget.PlayerBody, 0).IsName("dano")) { desiredState = PlayerAnimState.dano; }
        else if (!movementScript.IsGrounded() || movementScript.IsIgnoringPlatforms())
        {
            if (movementScript.IsWallSliding()) desiredState = PlayerAnimState.derrapagem;
            else if (movementScript.IsDashing() || movementScript.IsWallDashing()) desiredState = PlayerAnimState.dashAereo;
            else if (movementScript.GetVerticalVelocity() > 0.1f) desiredState = PlayerAnimState.pulando;
            else desiredState = PlayerAnimState.falling;
        }
        else // No chão
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
            if (!movementScript.IsGrounded() || movementScript.IsIgnoringPlatforms())
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
        // Pergunta para o WeaponHandler se a arma atual AINDA é uma arma de mira.
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
        Debug.Log("Animação de pouso TERMINOU. Liberando o jogador.");
        isLanding = false;
        movementScript.OnLandingComplete();
    }
    // Função atualizada para suportar Base vs Upgraded
    public void EquipSkill(SkillSO newSkill)
    {
        if (newSkill == null)
        {
            Debug.LogWarning("Tentativa de equipar uma Skill nula.");
            return;
        }

        // Verifica a categoria da habilidade
        if (newSkill.skillClass == SkillClass.Movimento)
        {
            // Verifica qual a AÇÃO específica para decidir o slot
            switch (newSkill.actionToPerform)
            {
                case MovementSkillType.SuperJump:
                    // Verifica o Nível (Tier) da habilidade no SO
                    if (newSkill.skillTier == SkillTier.Upgraded)
                    {
                        upgradedJumpSkill = newSkill;
                    }
                    else // Se for Base
                    {
                        baseJumpSkill = newSkill;
                    }
                    break;

                case MovementSkillType.Dash:
                    // Verifica o Nível (Tier) da habilidade no SO
                    if (newSkill.skillTier == SkillTier.Upgraded)
                    {
                        upgradedDashSkill = newSkill;
                    }
                    else // Se for Base
                    {
                        baseDashSkill = newSkill;
                    }
                    break;

                // As skills abaixo ainda não possuem slots "Upgraded" definidos nas variáveis,
                // então elas sempre vão para o slot padrão independentemente do Tier.
                case MovementSkillType.DashJump:
                    dashJumpSkill = newSkill;
                    break;

                case MovementSkillType.WallSlide:
                    wallSlideSkill = newSkill;
                    break;

                case MovementSkillType.WallJump:
                    wallJumpSkill = newSkill;
                    break;

                case MovementSkillType.WallDash:
                    wallDashSkill = newSkill;
                    break;

                case MovementSkillType.WallDashJump:
                    wallDashJumpSkill = newSkill;
                    break;

                default:
                    Debug.LogWarning($"Tipo de Movimento {newSkill.actionToPerform} não tem slot definido no EquipSkill.");
                    break;
            }
        }
        else if (newSkill.skillClass == SkillClass.Combate)
        {
            switch (newSkill.combatActionToPerform)
            {
                case CombatSkillType.Block:
                    blockSkill = newSkill;
                    break;

                // Futuros tipos de combate viriam aqui...

                default:
                    Debug.LogWarning($"Tipo de Combate {newSkill.combatActionToPerform} não tem slot definido no EquipSkill.");
                    break;
            }
        }

        // Atualiza as referências ativas (activeJumpSkill e activeDashSkill)
        // Isso garante que a mudança tenha efeito imediato no gameplay.
        SetPowerMode(isPowerModeActive);

        Debug.Log($"Skill '{newSkill.skillName}' (Tier: {newSkill.skillTier}) equipada com sucesso.");
    }
}