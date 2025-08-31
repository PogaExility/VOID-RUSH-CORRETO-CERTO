using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Animator animator;


    void Awake()
    {
        animator = GetComponent<Animator>();
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

    // Adicione esta corotina em qualquer lugar dentro da classe PlayerController
   // Em PlayerController.cs


    // Em PlayerController.cs
    void Update()
    {
        // Lógica de inventário e interação que não depende do estado do jogador
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }
        if (Input.GetKeyDown(KeyCode.E) && canInteract && !isInventoryOpen) { Interact(); }

        // --- O NOVO FLUXO DE COMANDO ---

        // 1. LÊ O INPUT UMA ÚNICA VEZ
        float horizontalInput = 0;
        if (!isInventoryOpen)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        // 2. COMANDA O "CORPO" A SE ATUALIZAR
        // Isso força o HandleFlipLogic a rodar ANTES de qualquer skill ser testada.
        movementScript.SetMoveInput(horizontalInput);

        // --- FIM DO NOVO FLUXO ---

        // Se o inventário estiver aberto, para aqui.
        if (isInventoryOpen) return;

        // Agora, o resto da lógica roda com o estado do personagem já atualizado
        HandlePowerModeToggle();
        HandleSkillInput();
        UpdateAnimations();
        wasGroundedLastFrame = movementScript.IsGrounded();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            movementScript.CutJump();
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

    // Em PlayerController.cs
    // Em PlayerController.cs
    // Em PlayerController.cs
    // Em PlayerController.cs
    // Em PlayerController.cs
    // Em PlayerController.cs

    private void HandleSkillInput()
    {
        if (movementScript.IsWallSliding())
        {
            if (skillRelease.TryActivateSkill(wallDashJumpSkill)) return;
            if (skillRelease.TryActivateSkill(wallJumpSkill)) return;
            if (skillRelease.TryActivateSkill(wallDashSkill)) return;
        }
        else if (movementScript.IsTouchingWall() && !movementScript.IsGrounded())
        {
            if (skillRelease.TryActivateSkill(wallSlideSkill)) return;
        }

        if (skillRelease.TryActivateSkill(dashJumpSkill)) return;
        if (skillRelease.TryActivateSkill(activeJumpSkill)) return;
        if (skillRelease.TryActivateSkill(activeDashSkill)) return;
    }
    private void HandleCombatInput()
    {
        // --- CORREÇÃO CS1955: Usa o método com '()' ---
        if (!movementScript.IsDashing()) combatController.ProcessCombatInput();
    }

    // Em PlayerController.cs

    // Substitua a sua UpdateAnimations inteira por esta
    // Em PlayerController.cs
    // Em PlayerController.cs
    // Em PlayerController.cs
    private void UpdateAnimations()
    {
        // TRAVA PRINCIPAL: Se estamos no processo de pouso, não faça mais nada.
        if (isLanding)
        {
            return;
        }

        // Se acabamos de tocar o chão, INICIA o processo de pouso.
        if (!wasGroundedLastFrame && movementScript.IsGrounded())
        {
            isLanding = true;
            movementScript.OnLandingStart();
            animatorController.PlayState(PlayerAnimState.pousando);
            return;
        }

        // A LINHA PROBLEMÁTICA FOI REMOVIDA DAQUI

        // --- LÓGICA NORMAL DE ANIMAÇÃO (só roda se NÃO estivermos pousando) ---
        if (movementScript.IsWallSliding()) animatorController.PlayState(PlayerAnimState.derrapagem);
        else if (movementScript.IsInParabolaArc()) animatorController.PlayState(PlayerAnimState.dashAereo);
        else if (movementScript.IsDashing()) animatorController.PlayState(movementScript.IsGrounded() ? PlayerAnimState.dash : PlayerAnimState.dashAereo);
        else if (!movementScript.IsGrounded())
        {
            if (movementScript.GetVerticalVelocity() > 0.1f) animatorController.PlayState(PlayerAnimState.pulando);
            else animatorController.PlayState(PlayerAnimState.falling);
        }
        else
        {
            if (movementScript.IsMoving()) animatorController.PlayState(PlayerAnimState.andando);
            else animatorController.PlayState(PlayerAnimState.parado);
        }
    }
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
    // Em PlayerController.cs
    public void OnLandingAnimationEnd()
    {
        Debug.Log("Animação de pouso TERMINOU. Liberando o jogador.");
        isLanding = false; // Libera a trava da animação
        movementScript.OnLandingComplete(); // Libera a física do personagem
    }

}