using UnityEngine;


[RequireComponent(typeof(AdvancedPlayerMovement2D))] //...etc
public class PlayerController : MonoBehaviour
{
    [Header("Referências de UI")]
    [Tooltip("Arraste o objeto do Canvas do seu inventário aqui.")]
    public GameObject inventoryPanel;

    [Header("Referências de Movimento")] public SkillRelease skillRelease; public AdvancedPlayerMovement2D movementScript; public PlayerAnimatorController animatorController; public EnergyBarController energyBar; public GameObject powerModeIndicator;
    [Header("Referências de Combate")] public CombatController combatController; public PlayerAttack playerAttack; public DefenseHandler defenseHandler;
    [Header("Skills Básicas")] public SkillSO baseJumpSkill; public SkillSO baseDashSkill;
    [Header("Skills com Upgrades")] public SkillSO upgradedJumpSkill; public SkillSO upgradedDashSkill; public SkillSO skillSlot1; public SkillSO skillSlot2;
    [Header("Skills de Parede")] public SkillSO wallJumpSkill; public SkillSO wallDashSkill;

    [Header("Configurações de Input")]
    [Tooltip("A janela de tempo em segundos para executar a combinação.")]
    public float wallInputBufferTime = 0.15f;
    private float _lastWallJumpInputTime = -10f;
    private float _lastWallDashInputTime = -10f;

    private SkillSO activeJumpSkill; private SkillSO activeDashSkill; private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;
    private bool isInventoryOpen = false;

    void Awake() { movementScript = GetComponent<AdvancedPlayerMovement2D>(); skillRelease = GetComponent<SkillRelease>(); combatController = GetComponent<CombatController>(); playerAttack = GetComponent<PlayerAttack>(); defenseHandler = GetComponent<DefenseHandler>(); if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>(); }

    void Start()
    {
        energyBar.SetMaxEnergy(100f);
        SetPowerMode(false);
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
    }

    void Update()
    {
        // ===== INÍCIO DA ALTERAÇÃO =====
        if (Input.GetKeyDown(KeyCode.E)) // TROCADO DE 'I' PARA 'E'
        {
            ToggleInventory();
        }
        // ===== FIM DA ALTERAÇÃO =====

        if (isInventoryOpen)
        {
            return;
        }

        HandleAllInput();
        UpdateAnimations();
        wasGroundedLastFrame = movementScript.IsGrounded();
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null) return;
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        Time.timeScale = isInventoryOpen ? 0f : 1f;
    }

    private void HandleAllInput()
    {
        if (isLanding) return;

        bool jumpInputDown = Input.GetKeyDown(KeyCode.Space);
        bool dashInputDown = (activeDashSkill != null && Input.GetKeyDown(activeDashSkill.activationKey)) ||
                             (wallDashSkill != null && Input.GetKeyDown(wallDashSkill.activationKey));

        if (!playerAttack.IsAttacking() && !playerAttack.IsReloading())
        {
            if (movementScript.IsWallSliding())
            {
                if ((jumpInputDown && Input.GetKey(wallDashSkill.activationKey)) || (dashInputDown && Input.GetKey(KeyCode.Space))) { TryActivateCombinedSkill(); return; }
                if (jumpInputDown) TryActivateSkill(wallJumpSkill);
                if (dashInputDown) TryActivateSkill(wallDashSkill);
            }
            else
            {
                if (jumpInputDown) TryActivateSkill(activeJumpSkill);
                if (dashInputDown) TryActivateSkill(activeDashSkill);
            }
            if (Input.GetKeyUp(KeyCode.Space)) movementScript.CutJump();
        }

        if (!movementScript.IsDashing()) combatController.ProcessCombatInput();
        if (!playerAttack.IsAttacking() && !playerAttack.IsReloading() && !defenseHandler.IsBlocking()) { if (isPowerModeActive) { if (skillSlot1 != null && Input.GetKeyDown(skillSlot1.activationKey)) TryActivateSkill(skillSlot1); if (skillSlot2 != null && Input.GetKeyDown(skillSlot2.activationKey)) TryActivateSkill(skillSlot2); } }
        HandlePowerModeToggle();
    }

    private void TryActivateCombinedSkill()
    {
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