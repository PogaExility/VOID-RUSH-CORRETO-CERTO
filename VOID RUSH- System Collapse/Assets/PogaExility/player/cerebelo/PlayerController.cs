using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(SkillRelease))]
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
    public SkillSO dashJumpSkill;

    [Header("Skills com Upgrades (Glow Mode)")]
    public SkillSO upgradedJumpSkill;
    public SkillSO upgradedDashSkill;

    [Header("Skills de Parede (Sempre Ativas)")]
    public SkillSO wallSlideSkill;
    public SkillSO wallJumpSkill;
    public SkillSO wallDashSkill;
    public SkillSO wallDashJumpSkill;




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

        HandlePowerModeToggle();
        HandleSkillInput();
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

    private void HandleSkillInput()
    {
        // Se o jogador estiver em uma animação de aterrissagem, bloqueia as skills.
        if (isLanding) return;

        // Tenta ativar as skills que mudam com o Power Mode.
        skillRelease.TryActivateSkill(activeJumpSkill);
        skillRelease.TryActivateSkill(activeDashSkill);

        // Tenta ativar a skill de DashJump (que não muda com o Power Mode, por enquanto).
        skillRelease.TryActivateSkill(dashJumpSkill);

        // Tenta ativar todas as skills de parede, que estão sempre disponíveis.
        skillRelease.TryActivateSkill(wallSlideSkill);
        skillRelease.TryActivateSkill(wallJumpSkill);
        skillRelease.TryActivateSkill(wallDashSkill);
        skillRelease.TryActivateSkill(wallDashJumpSkill);
    }

    private void HandleCombatInput()
    {
        // --- CORREÇÃO CS1955: Usa o método com '()' ---
        if (!movementScript.IsDashing()) combatController.ProcessCombatInput();
    }

    // Dentro do seu PlayerController.cs

    private void UpdateAnimations()
    {
        if (isLanding) return;
        if (!wasGroundedLastFrame && movementScript.IsGrounded())
        {
            isLanding = true;
            movementScript.OnLandingStart();
            animatorController.PlayState(PlayerAnimState.pousando);
            return;
        }

        // --- AQUI ESTÁ A CORREÇÃO DA ANIMAÇÃO ---
        // Adicionamos uma checagem no topo. Se estiver deslizando na parede,
        // a animação de "derrapagem" tem prioridade sobre todas as outras.
        if (movementScript.IsWallSliding())
        {
            animatorController.PlayState(PlayerAnimState.derrapagem);
            return; // Retorna para não executar as outras checagens de animação
        }
        // --- FIM DA CORREÇÃO ---

        if (defenseHandler.IsBlocking())
        {
            if (defenseHandler.IsInParryWindow()) animatorController.PlayState(PlayerAnimState.parry);
            else animatorController.PlayState(PlayerAnimState.block);
        }
        else if (!movementScript.IsGrounded())
        {
            // A checagem de WallSliding acima garante que esta parte não seja executada incorretamente
            if (movementScript.IsDashing()) animatorController.PlayState(PlayerAnimState.dashAereo);
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

    public void OnLandingComplete()
    {
        isLanding = false;
        movementScript.OnLandingComplete();
    }

    // Adicione estas duas funções inteiras em qualquer lugar dentro da classe PlayerController

    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetPowerMode(!isPowerModeActive);
        }
        // Se você usa uma barra de energia, esta linha desativa o modo automaticamente
        // if (isPowerModeActive && energyBar != null && energyBar.GetCurrentEnergy() <= 0) SetPowerMode(false);
    }

    private void SetPowerMode(bool isActive)
    {
        // if (isActive && energyBar != null && energyBar.GetCurrentEnergy() <= 0) isActive = false;

        isPowerModeActive = isActive;

        // Atualiza as skills que estão "equipadas"
        activeJumpSkill = isPowerModeActive ? upgradedJumpSkill : baseJumpSkill;
        activeDashSkill = isPowerModeActive ? upgradedDashSkill : baseDashSkill;

        // Ativa/desativa o feedback visual na UI
        if (powerModeIndicator != null)
        {
            powerModeIndicator.SetActive(isPowerModeActive);
        }
        Debug.Log("Power Mode Ativo: " + isPowerModeActive);
    }

    // --- A FUNÇÃO QUE FALTAVA ---
    public SkillSO GetActiveJumpSkill()
    {
        return activeJumpSkill;
    }
}