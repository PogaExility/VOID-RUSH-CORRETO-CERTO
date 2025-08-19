using UnityEngine;

[RequireComponent(typeof(AdvancedPlayerMovement2D))] //...etc
public class PlayerController : MonoBehaviour
{
    [Header("Refer�ncias de Movimento")] public SkillRelease skillRelease; public AdvancedPlayerMovement2D movementScript; public PlayerAnimatorController animatorController; public EnergyBarController energyBar; public GameObject powerModeIndicator;
    [Header("Refer�ncias de Combate")] public CombatController combatController; public PlayerAttack playerAttack; public DefenseHandler defenseHandler;
    [Header("Skills B�sicas")] public SkillSO baseJumpSkill; public SkillSO baseDashSkill;
    [Header("Skills com Upgrades")] public SkillSO upgradedJumpSkill; public SkillSO upgradedDashSkill; public SkillSO skillSlot1; public SkillSO skillSlot2;

    // ===== IN�CIO DA ALTERA��O 1: ADICIONANDO SLOTS PARA SKILLS DE PAREDE =====
    [Header("Skills de Parede")]
    public SkillSO wallJumpSkill;
    public SkillSO wallDashSkill;
    // ===== FIM DA ALTERA��O 1 =====

    private SkillSO activeJumpSkill; private SkillSO activeDashSkill; private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;

    void Awake() { movementScript = GetComponent<AdvancedPlayerMovement2D>(); skillRelease = GetComponent<SkillRelease>(); combatController = GetComponent<CombatController>(); playerAttack = GetComponent<PlayerAttack>(); defenseHandler = GetComponent<DefenseHandler>(); if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>(); }
    void Start() { energyBar.SetMaxEnergy(100f); SetPowerMode(false); }
    void Update() { HandleAllInput(); UpdateAnimations(); wasGroundedLastFrame = movementScript.IsGrounded(); }

    private void HandleAllInput()
    {
        if (isLanding) return;

        bool jumpInputDown = Input.GetKeyDown(KeyCode.Space);
        // A checagem do input de dash agora deve considerar o wallDashSkill tamb�m
        bool dashInputDown = (activeDashSkill != null && Input.GetKeyDown(activeDashSkill.activationKey)) ||
                             (wallDashSkill != null && Input.GetKeyDown(wallDashSkill.activationKey));

        if (!playerAttack.IsAttacking() && !playerAttack.IsReloading())
        {
            // ===== IN�CIO DA ALTERA��O 2: USANDO AS SKILLS DE PAREDE DEDICADAS =====
            if (movementScript.IsWallSliding())
            {
                // A combina��o agora usa as skills de parede dedicadas
                if ((jumpInputDown && Input.GetKey(wallDashSkill.activationKey)) || (dashInputDown && Input.GetKey(KeyCode.Space)))
                {
                    TryActivateCombinedSkill();
                    return;
                }

                // Chamadas para as skills isoladas de parede
                if (jumpInputDown) TryActivateSkill(wallJumpSkill);
                if (dashInputDown) TryActivateSkill(wallDashSkill);
            }
            else // Se N�O estiver em WallSlide, usa as skills normais
            {
                if (jumpInputDown) TryActivateSkill(activeJumpSkill);
                if (dashInputDown) TryActivateSkill(activeDashSkill);
            }
            // ===== FIM DA ALTERA��O 2 =====

            if (Input.GetKeyUp(KeyCode.Space)) movementScript.CutJump();
        }

        if (!movementScript.IsDashing()) combatController.ProcessCombatInput();
        if (!playerAttack.IsAttacking() && !playerAttack.IsReloading() && !defenseHandler.IsBlocking()) { if (isPowerModeActive) { if (skillSlot1 != null && Input.GetKeyDown(skillSlot1.activationKey)) TryActivateSkill(skillSlot1); if (skillSlot2 != null && Input.GetKeyDown(skillSlot2.activationKey)) TryActivateSkill(skillSlot2); } }
        HandlePowerModeToggle();
    }

    private void TryActivateCombinedSkill()
    {
        // Garante que as skills de parede est�o atribu�das
        if (wallJumpSkill == null || wallDashSkill == null) return;

        float combinedCost = wallJumpSkill.energyCost + wallDashSkill.energyCost;
        if (energyBar.HasEnoughEnergy(combinedCost))
        {
            if (skillRelease.ActivateWallDashJump(wallJumpSkill, wallDashSkill, movementScript))
            {
                energyBar.ConsumeEnergy(combinedCost);
            }
        }
    }

    private void UpdateAnimations() { if (isLanding) { return; } if (!wasGroundedLastFrame && movementScript.IsGrounded()) { isLanding = true; movementScript.OnLandingStart(); animatorController.PlayState(PlayerAnimState.pousando); return; } if (defenseHandler.IsBlocking()) { if (defenseHandler.IsInParryWindow()) animatorController.PlayState(PlayerAnimState.parry); else animatorController.PlayState(PlayerAnimState.block); } else if (!movementScript.IsGrounded()) { if (movementScript.IsWallSliding()) animatorController.PlayState(PlayerAnimState.derrapagem); else if (movementScript.IsDashing()) animatorController.PlayState(PlayerAnimState.dashAereo); else if (movementScript.GetVerticalVelocity() > 0.1f) animatorController.PlayState(PlayerAnimState.pulando); else animatorController.PlayState(PlayerAnimState.falling); } else { if (movementScript.IsDashing()) animatorController.PlayState(PlayerAnimState.dash); else if (movementScript.IsMoving()) animatorController.PlayState(PlayerAnimState.andando); else animatorController.PlayState(PlayerAnimState.parado); } }
    public void OnLandingComplete() { isLanding = false; movementScript.OnLandingComplete(); }
    private void TryActivateSkill(SkillSO skillToUse) { if (skillToUse == null) return; if (energyBar.HasEnoughEnergy(skillToUse.energyCost)) { if (skillRelease.ActivateSkill(skillToUse, movementScript, animatorController)) { energyBar.ConsumeEnergy(skillToUse.energyCost); } } }
    private void HandlePowerModeToggle() { if (Input.GetKeyDown(KeyCode.G)) SetPowerMode(!isPowerModeActive); if (isPowerModeActive && energyBar.GetCurrentEnergy() <= 0) SetPowerMode(false); }
    private void SetPowerMode(bool isActive) { if (isActive && energyBar.GetCurrentEnergy() <= 0) isActive = false; isPowerModeActive = isActive; activeJumpSkill = isActive ? upgradedJumpSkill : baseJumpSkill; activeDashSkill = isActive ? upgradedDashSkill : baseDashSkill; if (powerModeIndicator != null) powerModeIndicator.SetActive(isActive); }
}