using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AdvancedPlayerMovement2D))]
public class PlayerController : MonoBehaviour
{
    // --- SUAS REFERÊNCIAS ORIGINAIS ---
    [Header("Referências de Gerenciamento")]
    public CursorManager cursorManager;
    public InventoryManager inventoryManager;

    [Header("Referências de UI")]
    public GameObject inventoryPanel;

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

    // --- A NOVA ESTRUTURA DE SKILLS ---
    [Header("Skills Básicas")]
    public SkillSO baseJumpSkill;
    public SkillSO baseDashSkill;

    [Header("Skills com Upgrades (Glow Mode)")]
    public SkillSO upgradedJumpSkill;
    public SkillSO upgradedDashSkill;

    [Header("Skills de Parede (Sempre Ativas)")]
    public List<SkillSO> wallSkills = new List<SkillSO>();


    // --- SUAS VARIÁVEIS DE ESTADO ORIGINAIS ---
    private bool isInventoryOpen = false;
    private List<GameObject> nearbyInteractables = new List<GameObject>();
    private bool canInteract => nearbyInteractables.Count > 0;
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;

    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>();
        skillRelease = GetComponent<SkillRelease>();
        combatController = GetComponent<CombatController>();
        playerAttack = GetComponent<PlayerAttack>();
        defenseHandler = GetComponent<DefenseHandler>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
        if (cursorManager == null) cursorManager = FindAnyObjectByType<CursorManager>();
    }

    void Start()
    {
        if (energyBar != null) energyBar.SetMaxEnergy(100f);
        SetPowerMode(false);
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
        if (cursorManager != null)
        {
            cursorManager.SetDefaultCursor();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.E) && canInteract && !isInventoryOpen)
        {
            Interact();
        }

        if (isInventoryOpen) return;

        // A chamada para a nova função de input de skills
        HandleSkillInput();

        // Suas lógicas de combate e power mode
        HandleCombatAndPowerMode();

        UpdateAnimations();
        wasGroundedLastFrame = movementScript.IsGrounded();
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null) return;
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        Time.timeScale = isInventoryOpen ? 0f : 1f;

        if (cursorManager != null)
        {
            if (isInventoryOpen)
            {
                cursorManager.SetInventoryCursor();
            }
            else
            {
                cursorManager.SetDefaultCursor();
            }
        }

        if (!isInventoryOpen && inventoryManager.heldItem != null)
        {
            inventoryManager.DropHeldItem();
        }
    }

    private void Interact()
    {
        if (nearbyInteractables.Count == 0) return;
        GameObject objectToInteract = nearbyInteractables[0];
        if (objectToInteract == null) { nearbyInteractables.RemoveAt(0); return; }

        if (objectToInteract.TryGetComponent<QuestGiver>(out var questGiver)) { questGiver.Interact(); nearbyInteractables.Remove(objectToInteract); }
        else if (objectToInteract.TryGetComponent<Checkpoint>(out var checkpoint)) { checkpoint.Interact(); nearbyInteractables.Remove(objectToInteract); }
        else if (objectToInteract.TryGetComponent<ItemPickup>(out var itemToPickup))
        {
            if (!isInventoryOpen)
            {
                ToggleInventory();
            }
            inventoryManager.StartHoldingItem(itemToPickup.itemData);
            nearbyInteractables.Remove(itemToPickup.gameObject);
            Destroy(itemToPickup.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<ItemPickup>() != null || other.GetComponent<QuestGiver>() != null || other.GetComponent<Checkpoint>() != null || other.GetComponent<MissionBoard>() != null || other.GetComponent<TravelPoint>() != null)
        {
            if (!nearbyInteractables.Contains(other.gameObject))
            {
                nearbyInteractables.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (nearbyInteractables.Contains(other.gameObject))
        {
            nearbyInteractables.Remove(other.gameObject);
        }
    }

    // --- O NOVO SISTEMA DE INPUTS ---

    private void HandleSkillInput()
    {
        if (isLanding || playerAttack.IsAttacking() || playerAttack.IsReloading()) return;

        // Tenta ativar as skills que mudam com o Power Mode
        skillRelease.TryActivateSkill(activeJumpSkill);
        skillRelease.TryActivateSkill(activeDashSkill);

        // Tenta ativar todas as skills de parede
        foreach (var skill in wallSkills)
        {
            skillRelease.TryActivateSkill(skill);
        }
    }

    private void HandleCombatAndPowerMode()
    {
        if (!movementScript.IsDashing()) combatController.ProcessCombatInput();
        HandlePowerModeToggle();
    }

    private void UpdateAnimations()
    {
        if (isLanding) return;
        if (!wasGroundedLastFrame && movementScript.IsGrounded()) { isLanding = true; movementScript.OnLandingStart(); animatorController.PlayState(PlayerAnimState.pousando); return; }
        if (defenseHandler.IsBlocking()) { if (defenseHandler.IsInParryWindow()) animatorController.PlayState(PlayerAnimState.parry); else animatorController.PlayState(PlayerAnimState.block); }
        else if (!movementScript.IsGrounded()) { if (movementScript.IsWallSliding()) animatorController.PlayState(PlayerAnimState.derrapagem); else if (movementScript.IsDashing()) animatorController.PlayState(PlayerAnimState.dashAereo); else if (movementScript.GetVerticalVelocity() > 0.1f) animatorController.PlayState(PlayerAnimState.pulando); else animatorController.PlayState(PlayerAnimState.falling); }
        else { if (movementScript.IsDashing()) animatorController.PlayState(PlayerAnimState.dash); else if (movementScript.IsMoving()) animatorController.PlayState(PlayerAnimState.andando); else animatorController.PlayState(PlayerAnimState.parado); }
    }

    public void OnLandingComplete()
    {
        isLanding = false;
        movementScript.OnLandingComplete();
    }

    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G)) SetPowerMode(!isPowerModeActive);
        if (isPowerModeActive && energyBar != null && energyBar.GetCurrentEnergy() <= 0) SetPowerMode(false);
    }

    private void SetPowerMode(bool isActive)
    {
        if (isActive && energyBar != null && energyBar.GetCurrentEnergy() <= 0) isActive = false;
        isPowerModeActive = isActive;
        activeJumpSkill = isActive ? upgradedJumpSkill : baseJumpSkill;
        activeDashSkill = isActive ? upgradedDashSkill : baseDashSkill;
        if (powerModeIndicator != null) powerModeIndicator.SetActive(isActive);
    }

    // Função para o SkillRelease saber qual skill de pulo está ativa
    public SkillSO GetActiveJumpSkill()
    {
        return activeJumpSkill;
    }
}