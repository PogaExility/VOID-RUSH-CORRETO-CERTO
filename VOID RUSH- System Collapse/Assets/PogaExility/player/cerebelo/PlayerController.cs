using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private bool isInAimMode = false;
    public SkillSO GetActiveDashSkill()
    {
        return activeDashSkill;
    }


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

    // Adicione esta corotina em qualquer lugar dentro da classe PlayerController
   // Em PlayerController.cs


    // Em PlayerController.cs
    void Update()
    {
        // Lógica de inventário e interação que não depende do estado do jogador
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }
        if (Input.GetKeyDown(KeyCode.E) && canInteract && !isInventoryOpen) { Interact(); }

     
        float horizontalInput = 0;
        if (!isInventoryOpen)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }
        movementScript.SetMoveInput(horizontalInput);
        if (isInventoryOpen) return;
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
            inventoryManager.ReturnHeldItem(); // A função correta para devolver ao inv.
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
            // A função correta para pegar um item do mundo é TryAddItem
            if (inventoryManager.TryAddItem(itemToPickup.itemData))
            {
                // Só destrói o objeto se ele foi pego com sucesso
                nearbyInteractables.Remove(itemToPickup.gameObject);
                Destroy(itemToPickup.gameObject);
            }
            else
            {
                // Opcional: Feedback se o inventário estiver cheio
                Debug.Log("Inventário cheio!");
            }
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

    // Em PlayerController.cs
    private void HandleSkillInput()
    {
        // 1. O JOGADOR APERTOU A TECLA DO DASH?
        // Lemos a tecla do SO da skill de dash ativa.
        if (activeDashSkill.triggerKeys.Any(key => Input.GetKeyDown(key)))
        {
            // Se sim, inicia o buffer e NÃO FAZ MAIS NADA NESTE FRAME.
            // Isso impede que o Dash normal seja ativado se o Pulo for pressionado junto.
            skillRelease.SetDashBuffer(dashJumpSkill.dashJump_InputBuffer);
        }

        // 2. TENTA ATIVAR AS SKILLS EM ORDEM DE PRIORIDADE
        if (skillRelease.TryActivateSkill(wallDashJumpSkill)) return;
        if (skillRelease.TryActivateSkill(dashJumpSkill)) return;
        if (skillRelease.TryActivateSkill(wallJumpSkill)) return;
        if (skillRelease.TryActivateSkill(wallDashSkill)) return;
        if (skillRelease.TryActivateSkill(wallSlideSkill)) return;
        if (skillRelease.TryActivateSkill(activeJumpSkill)) return;
        if (skillRelease.TryActivateSkill(activeDashSkill)) return;
    }
    private void HandleCombatInput()
    {
        // --- CORREÇÃO CS1955: Usa o método com '()' ---
        if (!movementScript.IsDashing()) combatController.ProcessCombatInput();
    }

    private void UpdateAimModeState()
    {
        // Entra no modo de mira se a postura for Firearm ou Buster
        // E se o jogador não estiver fazendo outra ação prioritária (como dashing ou pousando)
        bool shouldAim = (combatController.activeStance == WeaponType.Firearm || combatController.activeStance == WeaponType.Buster)
                         && !movementScript.IsDashing() && !isLanding;

        if (isInAimMode != shouldAim)
        {
            isInAimMode = shouldAim;
            // Avisa o PlayerAttack para ativar/desativar o IK dos braços/cabeça
            playerAttack.SetAiming(isInAimMode);
        }

        // Se estiver no modo de mira, o flip é controlado pelo mouse
        if (isInAimMode)
        {
            float mouseDirectionX = combatController.aimDirection.x;
            movementScript.FaceDirection(mouseDirectionX > 0 ? 1 : -1);
        }
    }
    private void UpdateAnimations()
    {
        if (isLanding) return;

        if (!wasGroundedLastFrame && movementScript.CheckState(PlayerState.IsGrounded))
        {
            isLanding = true;
            movementScript.OnLandingStart();
            animatorController.PlayState(PlayerAnimState.pousando);
            return;
        }

        if (movementScript.CheckState(PlayerState.IsWallSliding)) animatorController.PlayState(PlayerAnimState.derrapagem);
        else if (movementScript.CheckState(PlayerState.IsInParabola)) animatorController.PlayState(PlayerAnimState.dashAereo);
        else if (movementScript.CheckState(PlayerState.IsDashing)) animatorController.PlayState(movementScript.CheckState(PlayerState.IsGrounded) ? PlayerAnimState.dash : PlayerAnimState.dashAereo);
        else if (!movementScript.CheckState(PlayerState.IsGrounded))
        {
            if (movementScript.GetVerticalVelocity() > 0.1f) animatorController.PlayState(PlayerAnimState.pulando);
            else animatorController.PlayState(PlayerAnimState.falling);
        }
        else
        {
            if (movementScript.IsMoving()) animatorController.PlayState(PlayerAnimState.andando);
            else animatorController.PlayState(PlayerAnimState.parado);
        }
        if (isInAimMode)
        {
            if (movementScript.IsMoving())
                animatorController.PlayState(PlayerAnimState.andarCotoco);
            else
                animatorController.PlayState(PlayerAnimState.paradoCotoco);
            return; // Sai da função para não tocar as animações normais
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