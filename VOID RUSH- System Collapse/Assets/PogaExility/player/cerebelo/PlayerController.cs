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
<<<<<<< HEAD
    private List<GameObject> nearbyInteractables = new List<GameObject>();
    private bool canInteract => nearbyInteractables.Count > 0;
    private bool wasGroundedLastFrame = true;

    // --- LÓGICA DO GLOW MODE ---
    private bool isPowerModeActive = false;
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;

=======
    private List<ItemPickup> nearbyItems = new List<ItemPickup>();
    private bool canInteract => nearbyItems.Count > 0;

    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;
    private List<GameObject> nearbyInteractables = new List<GameObject>();
>>>>>>> parent of 880d514 (coisa pra krl)
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

<<<<<<< HEAD
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
=======
        GameObject objectToInteract = nearbyInteractables[0];
        if (objectToInteract == null) return;

        // Tenta interagir com um ItemPickup
        if (objectToInteract.TryGetComponent<ItemPickup>(out var itemToPickup))
        {
            // --- MUDANÇA IMPORTANTE: Usamos a nova função do InventoryManager ---
            if (inventoryManager.PickupItem(itemToPickup.itemData))
            {
                nearbyInteractables.Remove(objectToInteract);
                Destroy(objectToInteract);
            }
            // Não abre mais o inventário automaticamente, pois PickupItem não "segura" o item.
        }
        // Tenta interagir com um QuestGiver
        else if (objectToInteract.TryGetComponent<QuestGiver>(out var questGiver))
        {
            questGiver.Interact();
            nearbyInteractables.Remove(objectToInteract);
>>>>>>> parent of 880d514 (coisa pra krl)
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
<<<<<<< HEAD
        if (other.GetComponent<ItemPickup>() != null || other.GetComponent<QuestGiver>() != null || other.GetComponent<Checkpoint>() != null)
=======
        // Adiciona qualquer objeto com ItemPickup OU QuestGiver à lista
        if (other.GetComponent<ItemPickup>() != null || other.GetComponent<QuestGiver>() != null)
>>>>>>> parent of 880d514 (coisa pra krl)
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
<<<<<<< HEAD
=======

    // --- Funções de movimento e combate (sem alterações) ---
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
>>>>>>> parent of 880d514 (coisa pra krl)
}