using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referências")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

    [Header("Skills")]
    public SkillSO baseJumpSkill, baseDashSkill;
    public SkillSO upgradedJumpSkill, upgradedDashSkill, skillSlot1, skillSlot2;

    private SkillSO activeJumpSkill, activeDashSkill;
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
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetPowerMode(!isPowerModeActive);
        }
        if (isPowerModeActive && energyBar.GetCurrentEnergy() <= 0)
        {
            SetPowerMode(false);
        }
    }

    private void HandleSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.Space)) TryActivateSkill(activeJumpSkill);
        if (Input.GetKeyDown(activeDashSkill.activationKey)) TryActivateSkill(activeDashSkill);
        if (isPowerModeActive)
        {
            if (Input.GetKeyDown(skillSlot1.activationKey)) TryActivateSkill(skillSlot1);
            if (Input.GetKeyDown(skillSlot2.activationKey)) TryActivateSkill(skillSlot2);
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

    // ====================================================================
    // A CORREÇÃO ESTÁ AQUI
    // ====================================================================
    private void UpdateAnimations()
    {
        bool isGrounded = movementScript.IsGrounded();
        bool isWallSliding = movementScript.IsWallSliding();
        // Usa o novo método IsInAction() para saber se o dash (ou wall jump) está acontecendo
        bool isInAction = movementScript.IsInAction();
        bool isJumping = movementScript.IsJumping();
        bool isFalling = movementScript.GetVerticalVelocity() < -0.1f && !isGrounded && !isWallSliding && !isJumping;
        bool isRunning = movementScript.IsMoving() && isGrounded;
        bool isIdle = !isRunning && isGrounded;

        // O bool de 'dashing' para o animator agora é o mesmo do 'isInAction'
        animatorController.UpdateAnimationState(isIdle, isRunning, isFalling, isWallSliding, isJumping, isInAction);
    }
    // ====================================================================
}