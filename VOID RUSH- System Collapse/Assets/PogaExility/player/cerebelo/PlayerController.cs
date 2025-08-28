using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(SkillRelease))]
public class PlayerController : MonoBehaviour
{
    // --- REFERÊNCIAS ESSENCIAIS ---
    [Header("Referências de Componentes")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public InventoryManager inventoryManager;
    public CursorManager cursorManager;

    [Header("Referências de UI e Efeitos")]
    public GameObject inventoryPanel;
    public GameObject powerModeIndicator; // Indicador visual do Glow Mode

    // --- ESTRUTURA DE SKILLS ---
    [Header("Skills Base")]
    public SkillSO baseJumpSkill;
    public SkillSO baseDashSkill;

    [Header("Skills de Power-Up (Glow)")]
    public SkillSO upgradedJumpSkill;
    public SkillSO upgradedDashSkill;

    [Header("Skills de Parede (Sempre Ativas)")]
    public SkillSO wallSlideSkill;
    public SkillSO wallJumpSkill;
    public SkillSO wallDashSkill;
    public SkillSO wallDashJumpSkill;

    // --- Variáveis de Estado Interno ---
    private bool isInventoryOpen = false;
    private List<GameObject> nearbyInteractables = new List<GameObject>();
    private bool canInteract => nearbyInteractables.Count > 0;
    private bool wasGroundedLastFrame = true;

    // --- LÓGICA DO GLOW MODE ---
    private bool isPowerModeActive = false;
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;

    void Awake()
    {
        // Pega as referências automaticamente se não forem arrastadas no Inspector.
        if (skillRelease == null) skillRelease = GetComponent<SkillRelease>();
        if (movementScript == null) movementScript = GetComponent<AdvancedPlayerMovement2D>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
        if (cursorManager == null) cursorManager = FindAnyObjectByType<CursorManager>();
    }

    void Start()
    {
        // Garante que o jogo comece no modo normal
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

        if (isInventoryOpen) return;

        if (Input.GetKeyDown(KeyCode.E) && canInteract)
        {
            Interact();
        }

        // A lógica do Modo Glow é checada a cada frame
        HandlePowerModeToggle();

        HandleSkillInput();
        UpdateAnimations();

        wasGroundedLastFrame = movementScript.IsGrounded;
    }

    private void HandleSkillInput()
    {
        // Tenta ativar as skills que dependem do Modo Glow (pulo e dash)
        skillRelease.TryActivateSkill(activeJumpSkill);
        skillRelease.TryActivateSkill(activeDashSkill);

        // Tenta ativar as skills de parede, que estão sempre disponíveis
        skillRelease.TryActivateSkill(wallSlideSkill);
        skillRelease.TryActivateSkill(wallJumpSkill);
        skillRelease.TryActivateSkill(wallDashSkill);
        skillRelease.TryActivateSkill(wallDashJumpSkill);
    }

    // --- LÓGICA DO MODO GLOW (RESTAURADA E COMPLETA) ---
    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetPowerMode(!isPowerModeActive);
        }
        // Desativa o Power Mode se a energia acabar (lógica futura com EnergyBar)
        // if (isPowerModeActive && energyBar != null && energyBar.GetCurrentEnergy() <= 0) SetPowerMode(false);
    }

    private void SetPowerMode(bool isActive)
    {
        // Lógica para impedir ativação sem energia
        // if (isActive && energyBar != null && energyBar.GetCurrentEnergy() <= 0) isActive = false;

        isPowerModeActive = isActive;

        // Atualiza as skills ativas com base no modo
        activeJumpSkill = isPowerModeActive ? upgradedJumpSkill : baseJumpSkill;
        activeDashSkill = isPowerModeActive ? upgradedDashSkill : baseDashSkill;

        // Atualiza o indicador visual na UI
        if (powerModeIndicator != null)
        {
            powerModeIndicator.SetActive(isPowerModeActive);
        }
        Debug.Log("Power Mode Ativo: " + isPowerModeActive);
    }

    // Função pública para que outros scripts (como SkillRelease) saibam qual a skill de pulo atual
    public SkillSO GetActiveJumpSkill()
    {
        return activeJumpSkill;
    }

    private void UpdateAnimations()
    {
        if (!wasGroundedLastFrame && movementScript.IsGrounded)
        {
            animatorController.PlayState(PlayerAnimState.pousando);
            return;
        }

        if (movementScript.IsWallSliding)
        {
            animatorController.PlayState(PlayerAnimState.derrapagem);
        }
        else if (movementScript.IsDashing)
        {
            animatorController.PlayState(movementScript.IsGrounded ? PlayerAnimState.dash : PlayerAnimState.dashAereo);
        }
        else if (!movementScript.IsGrounded)
        {
            if (movementScript.GetVerticalVelocity() > 0.1f)
            {
                animatorController.PlayState(PlayerAnimState.pulando);
            }
            else
            {
                animatorController.PlayState(PlayerAnimState.falling);
            }
        }
        else
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f)
            {
                animatorController.PlayState(PlayerAnimState.andando);
            }
            else
            {
                animatorController.PlayState(PlayerAnimState.parado);
            }
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null) return;
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        Time.timeScale = isInventoryOpen ? 0f : 1f;

        if (cursorManager != null)
        {
            if (isInventoryOpen) cursorManager.SetInventoryCursor();
            else cursorManager.SetDefaultCursor();
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
            inventoryManager.StartHoldingItem(itemToPickup.itemData);
            nearbyInteractables.Remove(itemToPickup.gameObject);
            Destroy(itemToPickup.gameObject);
            if (!isInventoryOpen) { ToggleInventory(); }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<ItemPickup>() != null || other.GetComponent<QuestGiver>() != null || other.GetComponent<Checkpoint>() != null)
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
}