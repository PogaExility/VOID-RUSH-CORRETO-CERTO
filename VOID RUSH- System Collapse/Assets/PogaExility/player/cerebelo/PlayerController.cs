using UnityEngine;

[RequireComponent(typeof(AdvancedPlayerMovement2D))]
[RequireComponent(typeof(SkillRelease))]
[RequireComponent(typeof(CombatController))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(DefenseHandler))]
public class PlayerController : MonoBehaviour
{
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

    [Header("Skills Ativas")]
    public SkillSO activeJumpSkill;
    public SkillSO activeDashSkill;

    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>();
        skillRelease = GetComponent<SkillRelease>();
        combatController = GetComponent<CombatController>();
        playerAttack = GetComponent<PlayerAttack>();
        defenseHandler = GetComponent<DefenseHandler>();
    }

    void Update()
    {
        // Verifica se o jogador está ocupado com uma ação de combate.
        bool isCombatLocked = playerAttack.IsAttacking() || playerAttack.IsReloading() || defenseHandler.IsBlocking();
        // Verifica se o jogador está ocupado com uma ação de movimento.
        bool isMovementLocked = movementScript.IsDashing();

        // Só permite inputs de movimento se não estiver ocupado com nada.
        if (!isCombatLocked && !isMovementLocked)
        {
            HandleMovementInput();
        }

        // Só permite inputs de combate se não estiver ocupado com movimento.
        if (!isMovementLocked)
        {
            combatController.ProcessCombatInput();
        }

        UpdateAnimations();
    }

    private void HandleMovementInput()
    {
        if (activeJumpSkill != null && Input.GetKeyDown(KeyCode.Space))
        {
            TryActivateSkill(activeJumpSkill);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            movementScript.CutJump();
        }
        if (activeDashSkill != null && Input.GetKeyDown(activeDashSkill.activationKey))
        {
            TryActivateSkill(activeDashSkill);
        }
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

    private void UpdateAnimations()
    {
        bool isGrounded = movementScript.IsGrounded();
        bool isWallSliding = movementScript.IsWallSliding();
        bool isDashing = movementScript.IsDashing();
        bool isJumping = movementScript.IsJumping();
        bool isWallJumping = movementScript.IsWallJumping();
        bool isFalling = movementScript.GetVerticalVelocity() < -0.1f && !isGrounded && !isWallSliding;
        bool isRunning = movementScript.IsMoving() && isGrounded;
        bool isIdle = !isRunning && isGrounded;

        animatorController.UpdateAnimationState(isIdle, isRunning, isFalling, isWallSliding, isJumping, isDashing || isWallJumping);
    }
}