using UnityEngine;

/// <summary>
/// O cérebro do jogador. Gerencia inputs, troca de skills (Modo de Poder)
/// e comanda os outros componentes (Movimento, Animação, Skills).
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Referências Essenciais")]
    [Tooltip("O script que executa a lógica das skills.")]
    public SkillRelease skillRelease;
    [Tooltip("O script que controla a física do movimento.")]
    public AdvancedPlayerMovement2D movementScript;
    [Tooltip("O script que controla o Animator.")]
    public PlayerAnimatorController animatorController;
    [Tooltip("O script que controla a barra de energia.")]
    public EnergyBarController energyBar;
    [Tooltip("Opcional. Efeito visual para o Modo de Poder.")]
    public GameObject powerModeIndicator;

    [Header("Skills Básicas (Forma Inicial)")]
    public SkillSO baseJumpSkill;
    public SkillSO baseDashSkill;

    [Header("Skills com Upgrades")]
    public SkillSO upgradedJumpSkill;
    public SkillSO upgradedDashSkill;
    public SkillSO skillSlot1;
    public SkillSO skillSlot2;

    // --- Controle Interno ---
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;

    void Start()
    {
        // Validação para garantir que tudo foi configurado no Inspector
        if (skillRelease == null || movementScript == null || animatorController == null || energyBar == null)
        {
            Debug.LogError("ERRO CRÍTICO: Uma ou mais referências não foram atribuídas no PlayerController!", this.gameObject);
            this.enabled = false;
            return;
        }

        energyBar.SetMaxEnergy(100f);
        // Garante que o jogo comece no modo básico
        SetPowerMode(false);
    }

    void Update()
    {
        HandlePowerModeToggle();
        HandleSkillInput();
        UpdateAnimations();
    }

    /// <summary>
    /// Lida com a ativação/desativação do Modo de Poder com a tecla G.
    /// </summary>
    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetPowerMode(!isPowerModeActive);
        }

        // Desativa automaticamente se a energia acabar
        if (isPowerModeActive && energyBar.GetCurrentEnergy() <= 0)
        {
            SetPowerMode(false);
        }
    }

    /// <summary>
    /// Lê os inputs de pulo, dash e skills de slot e os envia para serem processados.
    /// </summary>
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

        // Skills de slot só funcionam se o Modo de Poder estiver ativo
        if (isPowerModeActive)
        {
            if (skillSlot1 != null && Input.GetKeyDown(skillSlot1.activationKey)) TryActivateSkill(skillSlot1);
            if (skillSlot2 != null && Input.GetKeyDown(skillSlot2.activationKey)) TryActivateSkill(skillSlot2);
        }
    }

    /// <summary>
    /// Troca entre as skills básicas e com upgrades.
    /// </summary>
    private void SetPowerMode(bool isActive)
    {
        // Não pode ativar o modo se não tiver energia
        if (isActive && energyBar.GetCurrentEnergy() <= 0)
        {
            isActive = false;
        }

        isPowerModeActive = isActive;

        activeJumpSkill = isActive ? upgradedJumpSkill : baseJumpSkill;
        activeDashSkill = isActive ? upgradedDashSkill : baseDashSkill;

        if (powerModeIndicator != null) powerModeIndicator.SetActive(isActive);
    }

    /// <summary>
    /// Tenta ativar uma skill, checando a energia e chamando o SkillRelease.
    /// </summary>
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

    /// <summary>
    /// O cérebro da animação. Define qual estado deve ser ativado com base em uma ordem de prioridade.
    /// </summary>
    private void UpdateAnimations()
    {
        // 1. Coleta todos os estados de baixo nível do script de movimento
        bool isGrounded = movementScript.IsGrounded();
        bool isWallSliding = movementScript.IsWallSliding();
        bool isDashing = movementScript.IsDashing();
        bool isJumping = movementScript.IsJumping();
        bool isWallJumping = movementScript.IsWallJumping();
        bool isFalling = movementScript.GetVerticalVelocity() < -0.1f && !isGrounded;
        bool isRunning = movementScript.IsMoving() && isGrounded;
        bool isIdle = !isRunning && isGrounded;

        // Bools que serão enviados para o Animator
        bool animIdle = false, animRunning = false, animFalling = false,
             animWallSliding = false, animJumping = false, animDashing = false;

        // 2. Lógica de Prioridade
        if (isDashing) animDashing = true;
        else if (isWallJumping) animJumping = true;
        else if (isWallSliding) animWallSliding = true;
        else if (isJumping) animJumping = true;
        else if (isFalling) animFalling = true;
        else if (isRunning) animRunning = true;
        else animIdle = true;

        // 3. Envia a ordem final para o PlayerAnimatorController
        animatorController.UpdateAnimationState(animIdle, animRunning, animFalling, animWallSliding, animJumping, animDashing);
    }
}