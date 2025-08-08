using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refer�ncias")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public EnergyBarController energyBar;
    [Tooltip("Opcional. Um objeto (ex: uma Luz 2D) que liga/desliga para indicar o Modo de Poder.")]
    public GameObject powerModeIndicator;

    [Header("Skills B�sicas (Forma Inicial)")]
    [Tooltip("Vers�o do pulo sem upgrades. Deve ter Custo de Energia 0.")]
    public SkillSO baseJumpSkill;
    [Tooltip("Vers�o do dash sem a�reo. Deve ter Custo de Energia 0.")]
    public SkillSO baseDashSkill;

    [Header("Skills com Upgrades")]
    [Tooltip("Vers�o do pulo com upgrades (ex: pulo duplo).")]
    public SkillSO upgradedJumpSkill;
    [Tooltip("Vers�o do dash com upgrades (ex: a�reo).")]
    public SkillSO upgradedDashSkill;
    public SkillSO skillSlot1;
    public SkillSO skillSlot2;

    // --- Controle Interno ---
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;

    void Start()
    {
        if (skillRelease == null || movementScript == null || animatorController == null || energyBar == null)
        {
            Debug.LogError("ERRO CR�TICO: Refer�ncias faltando no PlayerController! Arraste os componentes no Inspector.", this.gameObject);
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
            if (!isPowerModeActive && energyBar.GetCurrentEnergy() > 0)
            {
                SetPowerMode(true);
            }
            else
            {
                SetPowerMode(false);
            }
        }

        if (isPowerModeActive && energyBar.GetCurrentEnergy() <= 0)
        {
            SetPowerMode(false);
            Debug.Log("<color=red>ENERGIA ESGOTADA! Modo de Poder desativado automaticamente.</color>");
        }
    }

    private void HandleSkillInput()
    {
        if (activeJumpSkill != null && Input.GetKeyDown(KeyCode.Space))
        {
            TryActivateSkill(activeJumpSkill);
        }

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
        isPowerModeActive = isActive;

        if (isActive)
        {
            Debug.Log("<color=cyan>MODO DE PODER ATIVADO</color>");
            activeJumpSkill = upgradedJumpSkill;
            activeDashSkill = upgradedDashSkill;
            if (powerModeIndicator != null) powerModeIndicator.SetActive(true);
        }
        else
        {
            Debug.Log("<color=grey>Modo de Poder Desativado</color>");
            activeJumpSkill = baseJumpSkill;
            activeDashSkill = baseDashSkill;
            if (powerModeIndicator != null) powerModeIndicator.SetActive(false);
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
        else
        {
            Debug.Log("SEM ENERGIA para usar " + skillToUse.skillName);
        }
    }

    // ====================================================================
    // A �NICA MUDAN�A EST� AQUI
    // ====================================================================
    private void UpdateAnimations()
    {
        bool isGrounded = movementScript.IsGrounded();
        bool isWallSliding = movementScript.IsWallSliding();

        bool isFalling = movementScript.GetVerticalVelocity() < -0.1f && !isGrounded && !isWallSliding;
        bool isRunning = movementScript.GetHorizontalInput() != 0 && isGrounded;
        bool isIdle = !isRunning && !isFalling && !isWallSliding && isGrounded;

        // Se o jogador est� no ch�o (parado ou correndo), ele definitivamente n�o est� pulando.
        // Isso desliga a anima��o de pulo assim que ele aterrissa.
        if (isGrounded)
        {
            animatorController.SetJumping(false);
        }

        // Envia os estados cont�nuos para o Animator
        animatorController.UpdateAnimator(isIdle, isRunning, isFalling, isWallSliding);
    }
    // ====================================================================
}