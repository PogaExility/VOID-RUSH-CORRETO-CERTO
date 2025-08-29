using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AdvancedPlayerMovement2D))] //...etc
public class PlayerController : MonoBehaviour
{
    // ===== NOVAS REFERÊNCIAS =====
    [Header("Referências de Gerenciamento")]
    [Tooltip("Arraste o objeto que contém o CursorManager.")]
    public CursorManager cursorManager;
    [Tooltip("Arraste o objeto que contém o InventoryManager (geralmente, este mesmo objeto Player).")]
    public InventoryManager inventoryManager;

    [Header("Referências de UI")]
    [Tooltip("Arraste o objeto do Canvas do seu inventário aqui.")]
    public GameObject inventoryPanel;

    // ===== O RESTO DAS SUAS REFERÊNCIAS =====
    [Header("Referências de Movimento")] public SkillRelease skillRelease; public AdvancedPlayerMovement2D movementScript; public PlayerAnimatorController animatorController; public EnergyBarController energyBar; public GameObject powerModeIndicator;
    [Header("Referências de Combate")] public CombatController combatController; public PlayerAttack playerAttack; public DefenseHandler defenseHandler;
    [Header("Skills Básicas")] public SkillSO baseJumpSkill; public SkillSO baseDashSkill;
    [Header("Skills com Upgrades")] public SkillSO upgradedJumpSkill; public SkillSO upgradedDashSkill; public SkillSO skillSlot1; public SkillSO skillSlot2;
    [Header("Skills de Parede")] public SkillSO wallJumpSkill; public SkillSO wallDashSkill;

    private bool isInventoryOpen = false;
    // ***** ESTA É A FUNÇÃO QUE DEVE PERMANECER *****
    // Dentro do seu PlayerController.cs

    // Dentro do PlayerController.cs
    // Dentro do seu PlayerController.cs

    private void Interact()
    {
        if (nearbyInteractables.Count == 0) return;
        GameObject objectToInteract = nearbyInteractables[0];
        if (objectToInteract == null) { nearbyInteractables.RemoveAt(0); return; }

        // Sua lógica para QuestGiver, Checkpoint, etc., permanece intacta e na ordem correta
        if (objectToInteract.TryGetComponent<QuestGiver>(out var questGiver))
        {
            questGiver.Interact();
            nearbyInteractables.Remove(objectToInteract);
        }
        else if (objectToInteract.TryGetComponent<Checkpoint>(out var checkpoint))
        {
            checkpoint.Interact();
            nearbyInteractables.Remove(objectToInteract);
        }
        // ... sua lógica para MissionBoard e TravelPoint ...

        // --- A LÓGICA DE COLETA DE ITEM, AGORA NA ORDEM CORRETA ---
        else if (objectToInteract.TryGetComponent<ItemPickup>(out var itemToPickup))
        {
            // PASSO 1: GARANTE QUE O INVENTÁRIO ESTEJA ABERTO PRIMEIRO.
            // Isso "acorda" o InventoryGridView e garante que ele está pronto para receber ordens.
            if (!isInventoryOpen)
            {
                ToggleInventory();
            }

            // PASSO 2: AGORA QUE A UI ESTÁ PRONTA, DAMOS A ORDEM PARA PEGAR O ITEM.
            // O InventoryGridView, agora ativo, vai ouvir o evento e criar a imagem da arma.
            inventoryManager.StartHoldingItem(itemToPickup.itemData);

            // PASSO 3: Limpa o objeto do mundo.
            nearbyInteractables.Remove(itemToPickup.gameObject);
            Destroy(itemToPickup.gameObject);
        }
    }


    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;
    private List<GameObject> nearbyInteractables = new List<GameObject>();
    private bool canInteract => nearbyInteractables.Count > 0;
    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>();
        skillRelease = GetComponent<SkillRelease>();
        combatController = GetComponent<CombatController>();
        playerAttack = GetComponent<PlayerAttack>();
        defenseHandler = GetComponent<DefenseHandler>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
        if (cursorManager == null) cursorManager = FindFirstObjectByType<CursorManager>(); // Procura automaticamente se não for arrastado
    }

    void Start()
    {
        energyBar.SetMaxEnergy(100f);
        SetPowerMode(false);
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
        if (cursorManager != null)
        {
            cursorManager.SetDefaultCursor(); // Garante que o cursor comece normal
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

        if (!isInventoryOpen)
        {
            inventoryManager.DropHeldItem();
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // A lista agora detecta os novos objetos
        if (other.GetComponent<ItemPickup>() != null ||
            other.GetComponent<QuestGiver>() != null ||
            other.GetComponent<Checkpoint>() != null ||
            other.GetComponent<MissionBoard>() != null || // <-- ADICIONADO
            other.GetComponent<TravelPoint>() != null)    // <-- ADICIONADO
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
}