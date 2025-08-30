using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(SkillRelease))]
public class PlayerController : MonoBehaviour
{
    // --- SUAS REFER�NCIAS ORIGINAIS ---
    [Header("Refer�ncias de Gerenciamento")]
    public CursorManager cursorManager;
    public InventoryManager inventoryManager;

    [Header("Refer�ncias de UI")]
    public GameObject inventoryPanel;


    [Header("Refer�ncias de Movimento")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

    [Header("Refer�ncias de Combate")]
    public CombatController combatController;
    public PlayerAttack playerAttack;
    public DefenseHandler defenseHandler;

    // --- A NOVA ESTRUTURA DE SKILLS ---
    [Header("Skills B�sicas")]
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




    // --- SUAS VARI�VEIS DE ESTADO ORIGINAIS ---
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

    // Em PlayerController.cs
    void Update()
    {
        // L�gica de invent�rio e intera��o que n�o depende do estado do jogador
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }
        if (Input.GetKeyDown(KeyCode.E) && canInteract && !isInventoryOpen) { Interact(); }

        // --- O NOVO FLUXO DE COMANDO ---

        // 1. L� O INPUT UMA �NICA VEZ
        float horizontalInput = 0;
        if (!isInventoryOpen)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        // 2. COMANDA O "CORPO" A SE ATUALIZAR
        // Isso for�a o HandleFlipLogic a rodar ANTES de qualquer skill ser testada.
        movementScript.SetMoveInput(horizontalInput);

        // --- FIM DO NOVO FLUXO ---

        // Se o invent�rio estiver aberto, para aqui.
        if (isInventoryOpen) return;

        // Agora, o resto da l�gica roda com o estado do personagem j� atualizado
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
    private void HandleSkillInput()
    {
        if (isLanding) return;

        // LER AS TECLAS DE FORMA EXPL�CITA
        bool jumpKeyDown = Input.GetKeyDown(KeyCode.Space);
        bool dashKeyDown = Input.GetKeyDown(KeyCode.Q);
        bool dashKeyHeld = Input.GetKey(KeyCode.Q);
        bool jumpKeyHeld = Input.GetKey(KeyCode.Space);

        // --- L�GICA DE INPUT CONTEXTUAL ---

        // CONTEXTO 1: J� EST� DESLIZANDO
        if (movementScript.IsWallSliding())
        {
            if ((jumpKeyDown && dashKeyHeld) || (dashKeyDown && jumpKeyHeld)) { if (skillRelease.TryActivateSkill(wallDashJumpSkill)) return; }
            if (jumpKeyDown) { if (skillRelease.TryActivateSkill(wallJumpSkill)) return; }
            if (dashKeyDown) { if (skillRelease.TryActivateSkill(wallDashSkill)) return; }
        }
        // CONTEXTO 2: TOCANDO A PAREDE, MAS N�O DESLIZANDO (PODE COME�AR A DESLIZAR)
        else if (movementScript.IsTouchingWall() && !movementScript.IsGrounded())
        {
            // Se apertar Espa�o aqui, a �NICA a��o poss�vel � iniciar o WallSlide.
            if (jumpKeyDown)
            {
                if (skillRelease.TryActivateSkill(wallSlideSkill)) return;
            }
        }
        // CONTEXTO 3: GERAL (CH�O/AR LIVRE)
        else
        {
            if ((jumpKeyDown && dashKeyHeld) || (dashKeyDown && jumpKeyHeld)) { if (skillRelease.TryActivateSkill(dashJumpSkill)) return; }
            if (jumpKeyDown) { if (skillRelease.TryActivateSkill(activeJumpSkill)) return; }
            if (dashKeyDown) { if (skillRelease.TryActivateSkill(activeDashSkill)) return; }
        }
    }
    private void HandleCombatInput()
    {
        // --- CORRE��O CS1955: Usa o m�todo com '()' ---
        if (!movementScript.IsDashing()) combatController.ProcessCombatInput();
    }

    // Dentro do seu PlayerController.cs

    // Em PlayerController.cs
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

        if (movementScript.IsWallSliding())
        {
            animatorController.PlayState(PlayerAnimState.derrapagem);
        }
        else if (!movementScript.IsGrounded())
        {
            // --- AQUI EST� A CORRE��O DA ANIMA��O ---
            // Se estiver em uma par�bola (DashJump ou WallDashJump), a anima��o � de dash a�reo.
            if (movementScript.IsInParabolaArc())
            {
                animatorController.PlayState(PlayerAnimState.dashAereo);
            }
            // Se n�o, continua com a l�gica normal.
            else if (movementScript.IsDashing())
            {
                animatorController.PlayState(PlayerAnimState.dashAereo);
            }
            else if (movementScript.GetVerticalVelocity() > 0.1f)
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
            if (movementScript.IsDashing())
            {
                animatorController.PlayState(PlayerAnimState.dash);
            }
            else if (movementScript.IsMoving())
            {
                animatorController.PlayState(PlayerAnimState.andando);
            }
            else
            {
                animatorController.PlayState(PlayerAnimState.parado);
            }
        }
    }

    public void OnLandingComplete()
    {
        isLanding = false;
        movementScript.OnLandingComplete();
    }

    // Adicione estas duas fun��es inteiras em qualquer lugar dentro da classe PlayerController

    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetPowerMode(!isPowerModeActive);
        }
        // Se voc� usa uma barra de energia, esta linha desativa o modo automaticamente
        // if (isPowerModeActive && energyBar != null && energyBar.GetCurrentEnergy() <= 0) SetPowerMode(false);
    }

    private void SetPowerMode(bool isActive)
    {
        // if (isActive && energyBar != null && energyBar.GetCurrentEnergy() <= 0) isActive = false;

        isPowerModeActive = isActive;

        // Atualiza as skills que est�o "equipadas"
        activeJumpSkill = isPowerModeActive ? upgradedJumpSkill : baseJumpSkill;
        activeDashSkill = isPowerModeActive ? upgradedDashSkill : baseDashSkill;

        // Ativa/desativa o feedback visual na UI
        if (powerModeIndicator != null)
        {
            powerModeIndicator.SetActive(isPowerModeActive);
        }
        Debug.Log("Power Mode Ativo: " + isPowerModeActive);
    }

    // --- A FUN��O QUE FALTAVA ---
    public SkillSO GetActiveJumpSkill()
    {
        return activeJumpSkill;
    }
}