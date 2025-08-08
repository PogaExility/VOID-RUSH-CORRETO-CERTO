using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referências")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public EnergyBarController energyBar;
    [Tooltip("Opcional. Um objeto (ex: uma Luz 2D) que liga/desliga para indicar o Modo de Poder.")]
    public GameObject powerModeIndicator;

    [Header("Skills Básicas (Forma Inicial)")]
    [Tooltip("Versão do pulo sem upgrades. Deve ter Custo de Energia 0.")]
    public SkillSO baseJumpSkill;
    [Tooltip("Versão do dash sem aéreo. Deve ter Custo de Energia 0.")]
    public SkillSO baseDashSkill;

    [Header("Skills com Upgrades")]
    [Tooltip("Versão do pulo com upgrades (ex: pulo duplo).")]
    public SkillSO upgradedJumpSkill;
    [Tooltip("Versão do dash com upgrades (ex: aéreo).")]
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
            Debug.LogError("ERRO CRÍTICO: Referências faltando no PlayerController! Arraste os componentes no Inspector.", this.gameObject);
            this.enabled = false;
            return;
        }

        energyBar.SetMaxEnergy(100f);
        // Garante que o jogo comece no modo básico e com os visuais corretos
        SetPowerMode(false);
    }

    void Update()
    {
        // --- LÓGICA DE TROCA DE MODO ---
        HandlePowerModeToggle();

        // --- LEITURA DE INPUTS DE HABILIDADE ---
        HandleSkillInput();

        // --- ATUALIZAÇÃO DE ANIMAÇÕES ---
        UpdateAnimations();
    }

    private void HandlePowerModeToggle()
    {
        // Se o jogador apertar G, tenta trocar o modo
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Se o modo estiver desligado, tenta ligar (só se tiver energia)
            if (!isPowerModeActive && energyBar.GetCurrentEnergy() > 0)
            {
                SetPowerMode(true);
            }
            // Se o modo já estiver ligado, desliga
            else
            {
                SetPowerMode(false);
            }
        }

        // Desativa o modo automaticamente se a energia acabar
        if (isPowerModeActive && energyBar.GetCurrentEnergy() <= 0)
        {
            SetPowerMode(false);
            Debug.Log("<color=red>ENERGIA ESGOTADA! Modo de Poder desativado automaticamente.</color>");
        }
    }

    private void HandleSkillInput()
    {
        // Pulo e Dash sempre funcionam, mas usam a skill que estiver ativa (básica ou com upgrade)
        if (activeJumpSkill != null && Input.GetKeyDown(KeyCode.Space))
        {
            TryActivateSkill(activeJumpSkill);
        }

        if (activeDashSkill != null && Input.GetKeyDown(activeDashSkill.activationKey))
        {
            TryActivateSkill(activeDashSkill);
        }

        // Skills de Slot só funcionam se o Modo de Poder estiver ativo
        if (isPowerModeActive)
        {
            if (skillSlot1 != null && Input.GetKeyDown(skillSlot1.activationKey)) TryActivateSkill(skillSlot1);
            if (skillSlot2 != null && Input.GetKeyDown(skillSlot2.activationKey)) TryActivateSkill(skillSlot2);
        }
    }

    // Centraliza a lógica de troca de modo
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

    private void UpdateAnimations()
    {
        bool isWallSliding = movementScript.IsWallSliding();
        bool isFalling = movementScript.GetVerticalVelocity() < -0.1f && !movementScript.IsGrounded() && !isWallSliding;
        bool isRunning = movementScript.GetHorizontalInput() != 0 && movementScript.IsGrounded();
        bool isIdle = !isRunning && !isFalling && !isWallSliding && movementScript.IsGrounded();

        animatorController.UpdateAnimator(isIdle, isRunning, isFalling, isWallSliding);
    }
}