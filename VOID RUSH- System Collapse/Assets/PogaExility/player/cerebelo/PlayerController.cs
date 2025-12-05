using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(SkillRelease))]
public class PlayerController : MonoBehaviour
{
    #region 1. Referências e Variáveis
    [Header("Referências de Gerenciamento")]
    public CursorManager cursorManager;

    [Header("Referências de UI")]
    public GameObject inventoryPanel;
    public GameObject combatHUDPanel;
    public EnergyBarController energyBar;


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
    public SkillSO wallSlideSkill;
    public SkillSO wallJumpSkill;
    public SkillSO wallDashSkill;
    public SkillSO wallDashJumpSkill;

    [Header("Skills de Combate")]
    public SkillSO blockSkill;

    [Header("Configuração de Áudio")]
    [Tooltip("Tempo em segundos entre cada som de passo (ex: 0.35 para caminhada normal).")]
    public float footstepInterval = 0.35f;
    // LINHA REMOVIDA: private float footstepTimer = 0f;

    // Estados Privados
    private bool attackBuffered = false;
    private bool isInventoryOpen = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;
    private bool isInAimMode = false;
    public bool inventoryLocked = false;
    private bool isActionInterruptingAim = false;
    private bool _isAttacking;

    // Referências Privadas e Auxiliares
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private PlayerStats playerStats;
    private PlayerAnimState previousBodyState;
    private ObjetoInterativo interagivelProximo;
    private PlayerSounds playerSounds;
    private Coroutine lungeCoroutine;
    private bool isTakingDamage = false;
    #endregion

    #region 2. Propriedades e Getters/Setters Públicos
    public bool IsAttacking
    {
        get { return _isAttacking; }
        set
        {
            _isAttacking = value;
            // Quando IsAttacking é setado para TRUE, desabilita a física do movimento.
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

    public void SetAimingState(bool isNowAiming)
    {
        if (isInAimMode == isNowAiming) return;

        isInAimMode = isNowAiming;

        movementScript.allowMovementFlip = !isNowAiming;
        animatorController.SetAimLayerWeight(isNowAiming ? 1f : 0f);
        weaponHandler.UpdateAimVisuals(isNowAiming);
    }

    public void SetAimingStateVisuals(bool isNowAiming)
    {
        movementScript.allowMovementFlip = !isNowAiming;
        animatorController.SetAimLayerWeight(isNowAiming ? 1f : 0f);

        // CORREÇÃO: Atualiza também o visual da arma (Sprite), não só a animação.
        if (weaponHandler != null)
        {
            weaponHandler.UpdateAimVisuals(isNowAiming);
        }
    }

    public SkillSO GetActiveJumpSkill()
    {
        return activeJumpSkill;
    }
    #endregion

    #region 3. Ciclo de Vida (Unity)
    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>();
        skillRelease = GetComponent<SkillRelease>();
        defenseHandler = GetComponent<DefenseHandler>();
        playerStats = GetComponent<PlayerStats>();
        playerSounds = GetComponent<PlayerSounds>();

        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        if (cursorManager == null) cursorManager = FindAnyObjectByType<CursorManager>();
        if (weaponHandler == null) weaponHandler = GetComponent<WeaponHandler>();
    }

    void Start()
    {
        if (energyBar != null) energyBar.SetMaxEnergy(100f);
        if (inventoryPanel != null) { inventoryPanel.SetActive(false); isInventoryOpen = false; }
        if (cursorManager != null) cursorManager.SetDefaultCursor();
        if (animatorController != null) animatorController.SetAimLayerWeight(0);

        // NÃO precisamos mais assinar o OnHealthChanged para animação de dano.
        // A animação será controlada pelo estado físico (Knockback) no Update.

        if (playerStats != null)
        {
            // Apenas morte precisa de evento específico
            playerStats.OnDeath += HandleDeath;
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= HandleHealthChanged;
            playerStats.OnDeath -= HandleDeath;
        }
    }
    private float _lastHealthKnown; // Variável auxiliar para saber se perdemos vida
    private void HandleHealthChanged(float current, float max)
    {
        // Se a vida atual for menor que a última conhecida, levamos dano
        // (Você pode precisar inicializar _lastHealthKnown no Start com a vida cheia)
        if (current < _lastHealthKnown && !playerStats.IsDead())
        {
            StartCoroutine(DamageAnimationRoutine());
        }
        _lastHealthKnown = current;
    }

    // Adicione esta inicialização no final do Start() também: 
    // _lastHealthKnown = playerStats.MaxHealth; 

    private void HandleDeath()
    {
        // A lógica de morte já é pega no UpdateAnimations via playerStats.IsDead(),
        // mas aqui você pode travar controles extras se necessário.
    }

    private IEnumerator DamageAnimationRoutine()
    {
        isTakingDamage = true;
        // Tempo estimado da animação de dano (ajuste conforme seu clipe)
        yield return new WaitForSeconds(0.25f);
        isTakingDamage = false;
    }


    void Update()
    {
        // 1. TRAVA DE MORTE (Prioridade Máxima)
        if (playerStats.IsDead())
        {
            movementScript.SetMoveInput(0);
            UpdateAnimations();
            return;
        }

        // 2. TRAVA DE DANO (Sincronizada com a Física)
        // Agora perguntamos diretamente ao script de movimento se ele está em Knockback.
        // Isso garante sincronia PERFEITA: Enquanto houver força física de dano, há animação e trava.
        if (movementScript.IsTakingDamage())
        {
            movementScript.SetMoveInput(0);
            if (playerSounds != null) playerSounds.UpdateWalkingSound(false);

            UpdateAnimations(); // Isso vai chamar SetAimingStateVisuals(false)
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }

        // ... (Resto do Update continua igual: Inventory, Attacking, Inputs...)
        if (isInventoryOpen)
        {
            movementScript.SetMoveInput(0);
            if (playerSounds != null) playerSounds.UpdateWalkingSound(false);
            return;
        }

        if (IsAttacking)
        {
            movementScript.SetMoveInput(0);
            if (playerSounds != null) playerSounds.UpdateWalkingSound(false);
            return;
        }

        HandleCrawlInput();
        movementScript.SetMoveInput(Input.GetAxisRaw("Horizontal"));
    

        // Lógica de Pouso
        bool isGroundedNow = movementScript.IsGrounded();
        bool justLanded = isGroundedNow && !wasGroundedLastFrame;

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
                if (AudioManager.Instance != null && playerSounds != null && playerSounds.landSound != null)
                {
                    AudioManager.Instance.PlaySoundEffect(playerSounds.landSound, transform.position);
                }
            }
        }

        HandleInteractionInput();
        HandleSkillInput();
        HandleCombatInput();
        ProcessAttackBuffer();
        HandleFootstepAudio();
        HandleWeaponSwitching();
        UpdateAnimations();

        wasGroundedLastFrame = isGroundedNow;

        if (Input.GetKeyUp(KeyCode.Space)) movementScript.CutJump();
    }
    #endregion

    #region 4. Inputs de Controle (Helpers do Update)
    private void HandleCrawlInput()
    {
        // 1. Pressionou CTRL (Agachar)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (movementScript.IsGrounded() && !movementScript.IsCrawling() && !movementScript.IsOnCrawlTransition())
            {
                if (isInAimMode) SetAimingState(false); // Sai da mira se estiver mirando

                // Inicia a física
                movementScript.BeginCrouchTransition();

                // Força a animação
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.abaixando);
            }
        }
        // 2. Soltou CTRL (Levantar)
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (movementScript.IsCrawling())
            {
                // Inicia a física
                movementScript.BeginStandUpTransition();

                // Força a animação
                animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.levantando);
            }
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && interagivelProximo != null)
        {
            interagivelProximo.Interagir();
        }
    }

    private void HandleSkillInput()
    {
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition())
        {
            return;
        }
        TryActivateMovementSkills();
    }

    private void HandleCombatInput()
    {
        // 1. Bloqueios de Movimento e Estado
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition()) return;

        // 2. Bloqueio de Recarga
        if (weaponHandler.IsReloading) return;

        // 3. BLOQUEIO DE COMBATE (Morte ou Dano)
        // Usamos movementScript.IsTakingDamage() para saber se estamos sendo empurrados.
        if (playerStats.IsDead() || movementScript.IsTakingDamage()) return;

        // 4. Verifica se tem arma
        var activeWeapon = weaponHandler.GetActiveWeaponSlot()?.item;
        if (activeWeapon == null) return;

        // 5. Input de Tiro (Meelee vs Ranged)
        if (activeWeapon.weaponType == WeaponType.Meelee)
        {
            if (Input.GetButtonDown("Fire1")) attackBuffered = true;
        }
        else
        {
            if (Input.GetButton("Fire1")) attackBuffered = true;
        }

        // 6. Outros Inputs de Combate
        if (Input.GetKeyDown(KeyCode.R)) weaponHandler.HandleReloadInput();

        if (Input.GetKeyDown(KeyCode.F)) defenseHandler.StartBlock(blockSkill);
        if (Input.GetKeyUp(KeyCode.F)) defenseHandler.EndBlock();
    }

    private void HandleWeaponSwitching()
    {
        if (weaponHandler.IsReloading) return;
        if (isInventoryOpen || weaponHandler == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0f) weaponHandler.CycleWeapon();
        else if (scrollInput < 0f) weaponHandler.CycleWeapon();
    }
    #endregion

    #region 5. Sistema de Skills (Equipar e Ativar)
    // Função atualizada para suportar Base vs Upgraded
    public void EquipSkill(SkillSO newSkill)
    {
        if (newSkill == null)
        {
            Debug.LogWarning("Tentativa de equipar uma Skill nula.");
            return;
        }

        if (newSkill.skillClass == SkillClass.Movimento)
        {
            switch (newSkill.actionToPerform)
            {
                case MovementSkillType.SuperJump:
                    baseJumpSkill = newSkill; // Sempre vai para o base
                    break;

                case MovementSkillType.Dash:
                    baseDashSkill = newSkill; // Sempre vai para o base
                    break;

                case MovementSkillType.DashJump: dashJumpSkill = newSkill; break;
                case MovementSkillType.WallSlide: wallSlideSkill = newSkill; break;
                case MovementSkillType.WallJump: wallJumpSkill = newSkill; break;
                case MovementSkillType.WallDash: wallDashSkill = newSkill; break;
                case MovementSkillType.WallDashJump: wallDashJumpSkill = newSkill; break;

                default:
                    Debug.LogWarning($"Tipo de Movimento {newSkill.actionToPerform} não tem slot definido no EquipSkill.");
                    break;
            }
        }
        else if (newSkill.skillClass == SkillClass.Combate)
        {
            switch (newSkill.combatActionToPerform)
            {
                case CombatSkillType.Block: blockSkill = newSkill; break;
                default:
                    Debug.LogWarning($"Tipo de Combate {newSkill.combatActionToPerform} não tem slot definido no EquipSkill.");
                    break;
            }
        }

        Debug.Log($"Skill '{newSkill.skillName}' equipada com sucesso.");
    }

    private bool TryActivateMovementSkills()
    {
        if (skillRelease.TryActivateSkill(wallDashJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(dashJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(wallJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(wallDashSkill)) return true;
        if (skillRelease.TryActivateSkill(wallSlideSkill)) return true;

        // Agora verifica diretamente as skills base, sem intermediários
        if (skillRelease.TryActivateSkill(baseJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(baseDashSkill)) return true;

        return false;
    }
    #endregion

    #region 6. Sistema de Combate e Física
    private void ProcessAttackBuffer()
    {
        // BLOQUEIO DE BUFFER:
        // Se estiver rastejando, morto ou LEVANDO DANO (Físico), cancela o ataque agendado.
        // Substituímos a variável local 'isTakingDamage' pela checagem direta no script de movimento.
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition() || playerStats.IsDead() || movementScript.IsTakingDamage())
        {
            attackBuffered = false;
            return;
        }

        if (!attackBuffered) return;

        bool canAttackNow = !IsAttacking &&
                            !movementScript.IsDashing() &&
                            weaponHandler.IsWeaponObjectActive();

        if (canAttackNow)
        {
            weaponHandler.HandleAttackInput();
            attackBuffered = false;
        }
    }

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
            movementScript.EnablePhysicsControl();
            lungeCoroutine = null;
        }
    }

    private IEnumerator LungeCoroutine(float distance, float speed)
    {
        if (Mathf.Abs(speed) < 0.1f) yield break;

        Rigidbody2D rb = movementScript.GetRigidbody();
        try
        {
            movementScript.DisablePhysicsControl();
            float duration = Mathf.Abs(distance / speed);
            float lungeDirection = Mathf.Sign(distance);
            float finalDirectionX = movementScript.GetFacingDirection().x * lungeDirection;
            float horizontalVelocity = finalDirectionX * speed;

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
            movementScript.SetVelocity(0, rb.linearVelocity.y);
            movementScript.EnablePhysicsControl();
            lungeCoroutine = null;
        }
    }
    #endregion

    #region 7. Animação e Áudio
    private void UpdateAnimations()
    {
        // 1. Prioridade SUPREMA: Morte
        if (playerStats.IsDead())
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.morrendo);
            SetAimingStateVisuals(false); // Esconde a arma ao morrer
            return;
        }

        // 2. Prioridade ALTA: Dano (Sincronizado com a Física)
        // Se o corpo está sendo empurrado (Knockback), toca a animação de dano e esconde a arma.
        if (movementScript.IsTakingDamage())
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.dano);
            SetAimingStateVisuals(false); // Esconde a arma para não bugar o visual
            return;
        }

        // 3. Prioridade: Ataque
        if (IsAttacking) return;

        // 4. Transição de Agachar
        if (movementScript.IsOnCrawlTransition()) return;

        // 5. Escalada
        if (movementScript.IsClimbing())
        {
            float vInput = movementScript.GetVerticalInput();
            animatorController.PlayState(AnimatorTarget.PlayerBody, vInput > 0 ? PlayerAnimState.subindoEscada : PlayerAnimState.descendoEscada);
            animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, Mathf.Abs(vInput) > 0.1f ? 1f : 0f);
            return;
        }

        // 6. Rastejar
        if (movementScript.IsCrawling())
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.rastejando);
            animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, movementScript.IsMoving() ? 1f : 0f);
            return;
        }

        // 7. Definição de Estados de Movimento (Padrão)
        PlayerAnimState desiredState;

        if (isLanding && !isInAimMode)
        {
            desiredState = PlayerAnimState.pousando;
        }
        else if (movementScript.IsDashing() || movementScript.IsWallDashing())
        {
            desiredState = movementScript.IsGrounded() ? PlayerAnimState.dash : PlayerAnimState.dashAereo;
        }
        else if (!movementScript.IsGrounded() || movementScript.IsIgnoringPlatforms())
        {
            if (movementScript.IsWallSliding()) desiredState = PlayerAnimState.derrapagem;
            else if (movementScript.GetVerticalVelocity() > 0.1f) desiredState = PlayerAnimState.pulando;
            else desiredState = PlayerAnimState.falling;
        }
        else // No chão
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f)
            {
                desiredState = PlayerAnimState.andando;
            }
            else
            {
                // Lógica de Pouca Vida
                if (playerStats.IsHealthLow())
                {
                    desiredState = PlayerAnimState.poucaVidaParado;
                }
                else
                {
                    desiredState = PlayerAnimState.parado;
                }
            }
        }

        // 8. Verifica interrupção da mira (Dash, Pouso, etc.)
        bool isDesiredStateAnAction = IsActionState(desiredState);

        if (isDesiredStateAnAction)
        {
            isActionInterruptingAim = true;
            SetAimingState(false);
        }
        else
        {
            // Se acabou a ação que interrompia, restaura a mira se estiver equipada
            if (isActionInterruptingAim)
            {
                isActionInterruptingAim = false;
                if (weaponHandler.IsAimWeaponEquipped()) SetAimingState(true);
            }
        }

        // 9. Aplica a animação Final
        if (isInAimMode)
        {
            // --- CORREÇÃO IMPORTANTE ---
            // Se voltamos para o modo de mira (depois de um dano ou morte cancelada),
            // precisamos garantir que o visual da arma seja religado.
            SetAimingStateVisuals(true);

            // Seleciona o clipe correto da camada de mira (Cotoco)
            if (!movementScript.IsGrounded() || movementScript.IsIgnoringPlatforms())
            {
                animatorController.PlayState(AnimatorTarget.PlayerBody, movementScript.GetVerticalVelocity() > 0.1f ? PlayerAnimState.pulandoCotoco : PlayerAnimState.fallingCotoco, 1);
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
            // Modo normal (sem arma ou ação que cancela mira)
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

    private void HandleFootstepAudio()
    {
        if (playerSounds == null) return;

        bool deveTocarSom = movementScript.IsGrounded() &&
                            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f &&
                            !movementScript.IsCrawling();

        playerSounds.UpdateWalkingSound(deveTocarSom);
    }

    public void OnActionAnimationComplete() { if (weaponHandler.IsAimWeaponEquipped()) SetAimingState(true); }
    public void OnCrouchDownAnimationComplete() { movementScript.CompleteCrouch(); }
    public void OnStandUpAnimationComplete() { movementScript.CompleteStandUp(); }
    public void OnLandingAnimationEnd() { isLanding = false; movementScript.OnLandingComplete(); }
    public void PlayFootstepSound() { }
    #endregion

    #region 8. Sistema de Interação e UI
    public void RegistrarInteragivel(ObjetoInterativo interagivel)
    {
        interagivelProximo = interagivel;
    }

    public void RemoverInteragivel(ObjetoInterativo interagivel)
    {
        if (interagivelProximo == interagivel)
        {
            interagivelProximo = null;
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
    #endregion
}