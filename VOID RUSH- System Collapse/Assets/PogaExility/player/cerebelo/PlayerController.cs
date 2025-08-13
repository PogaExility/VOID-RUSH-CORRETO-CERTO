using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referências")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

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

    void Start()
    {
        if (skillRelease == null || movementScript == null || animatorController == null || energyBar == null)
        {
            Debug.LogError("ERRO CRÍTICO: Referências faltando no PlayerController!", this.gameObject);
            this.enabled = false;
            return;
        }
        energyBar.SetMaxEnergy(100f);
        SetPowerMode(false);
    }

    void Update()
    {
        HandlePowerModeToggle();
        HandleSkillInput();
        UpdateAnimations();
    }

    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G)) SetPowerMode(!isPowerModeActive);
        if (isPowerModeActive && energyBar.GetCurrentEnergy() <= 0) SetPowerMode(false);
    }

    // --- MÉTODO MODIFICADO ---
    private void HandleSkillInput()
    {
        // Lógica para INICIAR o pulo quando a tecla é pressionada.
        if (activeJumpSkill != null && Input.GetKeyDown(KeyCode.Space))
        {
            TryActivateSkill(activeJumpSkill);
        }

        // Lógica para CORTAR o pulo quando a tecla é solta.
        if (Input.GetKeyUp(KeyCode.Space))
        {
            movementScript.CutJump();
        }

        // Outras skills continuam funcionando normalmente.
        if (activeDashSkill != null && Input.GetKeyDown(activeDashSkill.activationKey))
        {
            TryActivateSkill(activeDashSkill);
        }

        if (isPowerModeActive)
        {
            if (skillSlot1 != null && Input.GetKeyDown(skillSlot1.activationKey)) TryActivateSkill(skillSlot1);
            if (skillSlot2 != null && Input.GetKeyDown(skillSlot2.activationKey)) TryActivateSkill(skillSlot2);
        }
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