using UnityEngine;

[RequireComponent(typeof(AdvancedPlayerMovement2D))]
[RequireComponent(typeof(SkillRelease))]
[RequireComponent(typeof(CombatController))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(DefenseHandler))]
public class PlayerController : MonoBehaviour
{
    [Header("Referências de Movimento")] public SkillRelease skillRelease; public AdvancedPlayerMovement2D movementScript; public PlayerAnimatorController animatorController; public EnergyBarController energyBar; public GameObject powerModeIndicator;
    [Header("Referências de Combate")] public CombatController combatController; public PlayerAttack playerAttack; public DefenseHandler defenseHandler;
    [Header("Skills Básicas")] public SkillSO baseJumpSkill; public SkillSO baseDashSkill;
    [Header("Skills com Upgrades")] public SkillSO upgradedJumpSkill; public SkillSO upgradedDashSkill; public SkillSO skillSlot1; public SkillSO skillSlot2;
    private SkillSO activeJumpSkill; private SkillSO activeDashSkill; private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;

    // A trava de estado agora mora aqui, no controlador.
    private bool isLanding = false;

    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>(); skillRelease = GetComponent<SkillRelease>(); combatController = GetComponent<CombatController>(); playerAttack = GetComponent<PlayerAttack>(); defenseHandler = GetComponent<DefenseHandler>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
    }
    void Start() { energyBar.SetMaxEnergy(100f); SetPowerMode(false); }

    void Update()
    {
        HandleAllInput();
        UpdateAnimations();
        wasGroundedLastFrame = movementScript.IsGrounded();
    }

    private void HandleAllInput()
    {
        if (!playerAttack.IsAttacking() && !playerAttack.IsReloading())
        {
            if (activeJumpSkill != null && Input.GetKeyDown(KeyCode.Space)) TryActivateSkill(activeJumpSkill);
            if (Input.GetKeyUp(KeyCode.Space)) movementScript.CutJump();
        }

        if (!movementScript.IsDashing()) combatController.ProcessCombatInput();

        if (!playerAttack.IsAttacking() && !playerAttack.IsReloading() && !defenseHandler.IsBlocking())
        {
            if (!movementScript.IsDashing() && activeDashSkill != null && Input.GetKeyDown(activeDashSkill.activationKey)) TryActivateSkill(activeDashSkill);
            if (isPowerModeActive)
            {
                if (skillSlot1 != null && Input.GetKeyDown(skillSlot1.activationKey)) TryActivateSkill(skillSlot1);
                if (skillSlot2 != null && Input.GetKeyDown(skillSlot2.activationKey)) TryActivateSkill(skillSlot2);
            }
        }
        HandlePowerModeToggle();
    }

    private void UpdateAnimations()
    {
        // Se a animação de pouso está ativa, pare toda a lógica de animação aqui.
        if (isLanding)
        {
            return;
        }

        if (!wasGroundedLastFrame && movementScript.IsGrounded())
        {
            isLanding = true; // Ativa a trava
            animatorController.PlayState(PlayerAnimState.pousando);
            return; // Sai da função neste frame para garantir que 'pousando' seja o único comando enviado.
        }

        // O resto da lógica de animação...
        if (defenseHandler.IsBlocking())
        {
            if (defenseHandler.IsInParryWindow()) animatorController.PlayState(PlayerAnimState.parry);
            else animatorController.PlayState(PlayerAnimState.block);
        }
        else if (!movementScript.IsGrounded())
        {
            if (movementScript.IsWallSliding()) animatorController.PlayState(PlayerAnimState.derrapagem);
            else if (movementScript.IsDashing() && activeDashSkill != null && activeDashSkill.dashType == DashType.Aereo) animatorController.PlayState(PlayerAnimState.dashAereo);
            else if (movementScript.GetVerticalVelocity() > 0.1f) animatorController.PlayState(PlayerAnimState.pulando);
            else animatorController.PlayState(PlayerAnimState.falling);
        }
        else
        {
            if (movementScript.IsDashing()) animatorController.PlayState(PlayerAnimState.dash);
            else if (movementScript.IsMoving()) animatorController.PlayState(PlayerAnimState.andando);
            else animatorController.PlayState(PlayerAnimState.parado);
        }
    }

    /// <summary>
    /// ESTA É A NOVA FUNÇÃO PÚBLICA a ser chamada pelo Animation Event.
    /// </summary>
    public void OnLandingComplete()
    {
        isLanding = false; // Desativa a trava.
    }

    private void TryActivateSkill(SkillSO skillToUse) { if (skillToUse == null) return; if (energyBar.HasEnoughEnergy(skillToUse.energyCost)) { if (skillRelease.ActivateSkill(skillToUse, movementScript, animatorController)) { energyBar.ConsumeEnergy(skillToUse.energyCost); } } }
    private void HandlePowerModeToggle() { if (Input.GetKeyDown(KeyCode.G)) SetPowerMode(!isPowerModeActive); if (isPowerModeActive && energyBar.GetCurrentEnergy() <= 0) SetPowerMode(false); }
    private void SetPowerMode(bool isActive) { if (isActive && energyBar.GetCurrentEnergy() <= 0) isActive = false; isPowerModeActive = isActive; activeJumpSkill = isActive ? upgradedJumpSkill : baseJumpSkill; activeDashSkill = isActive ? upgradedDashSkill : baseDashSkill; if (powerModeIndicator != null) powerModeIndicator.SetActive(isActive); }
}