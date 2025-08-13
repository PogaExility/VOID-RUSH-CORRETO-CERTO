using UnityEngine;

[RequireComponent(typeof(AdvancedPlayerMovement2D))]
[RequireComponent(typeof(SkillRelease))]
[RequireComponent(typeof(CombatController))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(DefenseHandler))]
public class PlayerController : MonoBehaviour
{
    // ... (Todas as suas referências e variáveis permanecem iguais)
    [Header("Referências de Movimento")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

    [Header("Referências de Combate")]
    public CombatController combatController;
    public PlayerAttack playerAttack;
    public DefenseHandler defenseHandler;

    [Header("Skills Básicas")]
    public SkillSO baseJumpSkill;
    public SkillSO baseDashSkill;

    [Header("Skills com Upgrades")]
    public SkillSO upgradedJumpSkill;
    public SkillSO upgradedDashSkill;
    public SkillSO skillSlot1;
    public SkillSO skillSlot2;

    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;

    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>();
        skillRelease = GetComponent<SkillRelease>();
        combatController = GetComponent<CombatController>();
        playerAttack = GetComponent<PlayerAttack>();
        defenseHandler = GetComponent<DefenseHandler>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
    }

    void Start()
    {
        energyBar.SetMaxEnergy(100f);
        SetPowerMode(false);
    }

    void Update()
    {
        bool isCombatLocked = playerAttack.IsAttacking() || playerAttack.IsReloading() || defenseHandler.IsBlocking();
        bool isMovementLocked = movementScript.IsDashing();

        if (!isCombatLocked && !isMovementLocked)
        {
            HandleMovementInput();
        }
        if (!isMovementLocked)
        {
            combatController.ProcessCombatInput();
        }
        HandlePowerModeToggle();
        UpdateAnimations();
    }

    private void HandleMovementInput()
    {
        if (activeJumpSkill != null && Input.GetKeyDown(KeyCode.Space)) { TryActivateSkill(activeJumpSkill); }
        if (Input.GetKeyUp(KeyCode.Space)) { movementScript.CutJump(); }
        if (activeDashSkill != null && Input.GetKeyDown(activeDashSkill.activationKey)) { TryActivateSkill(activeDashSkill); }
        if (isPowerModeActive)
        {
            if (skillSlot1 != null && Input.GetKeyDown(skillSlot1.activationKey)) TryActivateSkill(skillSlot1);
            if (skillSlot2 != null && Input.GetKeyDown(skillSlot2.activationKey)) TryActivateSkill(skillSlot2);
        }
    }

    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G)) SetPowerMode(!isPowerModeActive);
        if (isPowerModeActive && energyBar.GetCurrentEnergy() <= 0) SetPowerMode(false);
    }

    private void SetPowerMode(bool isActive)
    {
        if (isActive && energyBar.GetCurrentEnergy() <= 0) isActive = false;
        isPowerModeActive = isActive;
        activeJumpSkill = isActive ? upgradedJumpSkill : baseJumpSkill;
        activeDashSkill = isActive ? upgradedDashSkill : baseDashSkill;
        if (powerModeIndicator != null) powerModeIndicator.SetActive(isActive);
    }

    private void TryActivateSkill(SkillSO skillToUse)
    {
        if (skillToUse == null) return;
        if (energyBar.HasEnoughEnergy(skillToUse.energyCost))
        {
            if (skillRelease.ActivateSkill(skillToUse, movementScript, animatorController))
            {
                energyBar.ConsumeEnergy(skillToUse.energyCost);
            }
        }
    }

    // ====================================================================
    // FUNÇÃO DE ANIMAÇÃO REESCRITA COM HIERARQUIA
    // ====================================================================
    private void UpdateAnimations()
    {
        // Pega todos os estados de uma vez.
        bool isGrounded = movementScript.IsGrounded();
        bool isWallSliding = movementScript.IsWallSliding();
        bool isDashing = movementScript.IsDashing();
        bool isJumping = movementScript.IsJumping();
        bool isFalling = movementScript.GetVerticalVelocity() < -0.1f && !isGrounded && !isWallSliding;
        bool isRunning = movementScript.IsMoving() && isGrounded;
        bool isBlocking = defenseHandler.IsBlocking();
        bool isParrying = defenseHandler.IsInParryWindow();

        // Variáveis que serão enviadas para o Animator
        bool animIdle = false;
        bool animRunning = false;
        bool animJumping = false;
        bool animFalling = false;
        bool animWallSliding = false;
        bool animDashing = false;
        bool animBlocking = false;
        bool animParrying = false;

        // HIERARQUIA DE PRIORIDADE:
        // Ações de combate e estados aéreos têm prioridade sobre o movimento no chão.

        if (isBlocking)
        {
            animBlocking = true;
            // O parry só pode acontecer se estiver bloqueando.
            if (isParrying)
            {
                animParrying = true;
            }
        }
        else if (isDashing)
        {
            animDashing = true;
        }
        else if (!isGrounded) // Se estiver no ar...
        {
            if (isWallSliding)
            {
                animWallSliding = true;
            }
            else if (isJumping)
            {
                animJumping = true;
            }
            else if (isFalling)
            {
                animFalling = true;
            }
        }
        else // Se estiver no chão...
        {
            if (isRunning)
            {
                animRunning = true;
            }
            else
            {
                animIdle = true;
            }
        }

        // Envia os valores finais e limpos para o Animator Controller.
        animatorController.UpdateAnimationState(
            animIdle, animRunning, animFalling, animWallSliding,
            animJumping, animDashing,
            animBlocking, animParrying, false // 'flipping' ainda não implementado
        );
    }
}