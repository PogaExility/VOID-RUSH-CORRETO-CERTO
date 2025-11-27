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
    private float footstepTimer = 0f;

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
        // SetPowerMode(false); <- REMOVIDO
        if (inventoryPanel != null) { inventoryPanel.SetActive(false); isInventoryOpen = false; }
        if (cursorManager != null) cursorManager.SetDefaultCursor();
        if (animatorController != null) animatorController.SetAimLayerWeight(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }

        if (isInventoryOpen)
        {
            movementScript.SetMoveInput(0);

            // --- CORREÇÃO ---
            // Se o inventário estiver aberto, forçamos o som a parar IMEDIATAMENTE
            // antes de sair da função com o 'return'.
            if (playerSounds != null) playerSounds.UpdateWalkingSound(false);

            return;
        }

        if (IsAttacking)
        {
            movementScript.SetMoveInput(0);
            // Também paramos o som se estiver atacando parado
            if (playerSounds != null) playerSounds.UpdateWalkingSound(false);
            return;
        }

        // Prioridade 1: Rastejar
        HandleCrawlInput();

        // Movimento Horizontal
        movementScript.SetMoveInput(Input.GetAxisRaw("Horizontal"));

        // Lógica de Pouso (Landing)
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
        // Pressionou para começar a rastejar
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (movementScript.IsGrounded() && !movementScript.IsCrawling() && !movementScript.IsOnCrawlTransition())
            {
                if (isInAimMode)
                {
                    SetAimingState(false);
                }
                movementScript.BeginCrouchTransition();
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.abaixando);
            }
        }
        // Soltou a tecla para levantar
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (movementScript.IsCrawling())
            {
                animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);
                movementScript.BeginStandUpTransition();
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
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition()) return;
        if (weaponHandler.IsReloading) return;

        var activeWeapon = weaponHandler.GetActiveWeaponSlot()?.item;
        if (activeWeapon == null) return;

        if (activeWeapon.weaponType == WeaponType.Meelee)
        {
            if (Input.GetButtonDown("Fire1")) attackBuffered = true;
        }
        else
        {
            if (Input.GetButton("Fire1")) attackBuffered = true;
        }

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
        if (movementScript.IsCrawling() || movementScript.IsOnCrawlTransition())
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

    #region 7. Sistema de Animação e Áudio
    private void UpdateAnimations()
    {
        if (IsAttacking) return;

        // --- Lógica de Animação (Mantenha a sua lógica visual aqui) ---
        // (Resumo para não apagar seu código visual)
        if (movementScript.IsClimbing())
        {
            float vInput = movementScript.GetVerticalInput();
            animatorController.PlayState(AnimatorTarget.PlayerBody, vInput > 0 ? PlayerAnimState.subindoEscada : PlayerAnimState.descendoEscada);
            animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, Mathf.Abs(vInput) > 0.1f ? 1f : 0f);
            return;
        }

        if (movementScript.IsCrawling())
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.rastejando);
            animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, movementScript.IsMoving() ? 1f : 0f);
            return;
        }

        PlayerAnimState desiredState;
        if (playerStats.IsDead()) desiredState = PlayerAnimState.morrendo;
        else if (isLanding && !isInAimMode) desiredState = PlayerAnimState.pousando;
        else if (!movementScript.IsGrounded()) desiredState = PlayerAnimState.pulando;
        else if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f) desiredState = PlayerAnimState.andando;
        else desiredState = PlayerAnimState.parado;

        if (!isInAimMode) animatorController.PlayState(AnimatorTarget.PlayerBody, desiredState);
    }

    // --- FUNÇÃO DE ÁUDIO MODIFICADA (LOOP) ---
    private void HandleFootstepAudio()
    {
        if (playerSounds == null) return;

        // Define se o player DEVE estar fazendo barulho de passos
        // Condições: Estar no chão + Input apertado + NÃO estar rastejando
        bool deveTocarSom = movementScript.IsGrounded() &&
                            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f &&
                            !movementScript.IsCrawling();

        // Manda o estado (Ligar ou Desligar) para o PlayerSounds
        playerSounds.UpdateWalkingSound(deveTocarSom);
    }

    // As outras funções de som (PlayFootstepSound antiga) podem ser removidas ou ignoradas
    // pois o controle agora é direto no PlayerSounds.updateWalkingSound

    public void OnActionAnimationComplete() { if (weaponHandler.IsAimWeaponEquipped()) SetAimingState(true); }
    public void OnCrouchDownAnimationComplete() { movementScript.CompleteCrouch(); }
    public void OnStandUpAnimationComplete() { movementScript.CompleteStandUp(); }
    public void OnLandingAnimationEnd() { isLanding = false; movementScript.OnLandingComplete(); }

    // Mantemos apenas para compatibilidade se algum outro script chamar, mas não usamos no Update
    public void PlayFootstepSound()
    {
        // Função legada, a lógica agora está no HandleFootstepAudio -> PlayerSounds
    }
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