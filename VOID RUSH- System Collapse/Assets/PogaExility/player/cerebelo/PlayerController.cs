using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(SkillRelease))]
public class PlayerController : MonoBehaviour
{

    [Header("Refer�ncias de Gerenciamento")]
    public CursorManager cursorManager;

    [Header("Refer�ncias de UI")]
    public GameObject inventoryPanel;
    public GameObject combatHUDPanel;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

    [Header("Refer�ncias de Movimento e Combate")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public DefenseHandler defenseHandler;
    public WeaponHandler weaponHandler;

    [Header("Skills de Movimento")]
    public SkillSO baseJumpSkill;
    public SkillSO baseDashSkill;
    public SkillSO dashJumpSkill;
    public SkillSO upgradedJumpSkill;
    public SkillSO upgradedDashSkill;
    public SkillSO wallSlideSkill;
    public SkillSO wallJumpSkill;
    public SkillSO wallDashSkill;
    public SkillSO wallDashJumpSkill;

    [Header("Skills de Combate")]
    public SkillSO blockSkill;

    // Vari�veis de Estado
    private bool isInventoryOpen = false;
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;
    private bool isInAimMode = false;
    private PlayerStats playerStats;
    public bool inventoryLocked = false;

    void Awake()
    {

        movementScript = GetComponent<AdvancedPlayerMovement2D>();
        skillRelease = GetComponent<SkillRelease>();
        defenseHandler = GetComponent<DefenseHandler>();
        playerStats = GetComponent<PlayerStats>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
        if (cursorManager == null) cursorManager = FindAnyObjectByType<CursorManager>();
        if (weaponHandler == null) weaponHandler = GetComponent<WeaponHandler>();

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

    public void SetAimingState(bool isNowAiming)
    {
        isInAimMode = isNowAiming;
        movementScript.allowMovementFlip = !isNowAiming;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }

        float horizontalInput = 0;
        if (!isInventoryOpen)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }
        movementScript.SetMoveInput(horizontalInput);

        if (isInventoryOpen) return;

        HandlePowerModeToggle();
        HandleSkillInput();
        HandleCombatInput();
        HandleWeaponSwitching();

        UpdateAnimations();
        wasGroundedLastFrame = movementScript.IsGrounded();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            movementScript.CutJump();
        }
    }
    private void HandleWeaponSwitching()
    {
        if (isInventoryOpen || weaponHandler == null) return;
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0f)
        {
            weaponHandler.CycleWeapon();
        }
        else if (scrollInput < 0f)
        {
            weaponHandler.CycleWeapon();
        }
    }
    private void ToggleInventory()
    {
        if (inventoryLocked) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        if (combatHUDPanel != null)
            combatHUDPanel.SetActive(!isInventoryOpen);

        Time.timeScale = isInventoryOpen ? 0f : 1f;

        if (cursorManager != null)
        {
            if (isInventoryOpen)
                cursorManager.SetInventoryCursor();
            else
                cursorManager.SetDefaultCursor();
        }
    }

    private void HandleSkillInput()
    {
        if (activeDashSkill.triggerKeys.Any(key => Input.GetKeyDown(key)))
        {
            skillRelease.SetDashBuffer(dashJumpSkill.dashJump_InputBuffer);
        }

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
        if (Input.GetButton("Fire1"))
        {
            weaponHandler.HandleAttackInput();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponHandler.HandleReloadInput();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            defenseHandler.StartBlock(blockSkill);
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            defenseHandler.EndBlock();
        }
    }



    private void UpdateAnimations()
    {
        // PRIORIDADE M�XIMA: Morte
        if (playerStats.IsDead())
        {
            animatorController.PlayState(PlayerAnimState.morrendo);
            return;
        }

        // PRIORIDADE 2: Eventos (Pouso, Dano)
        if (isLanding) return;
        if (!wasGroundedLastFrame && movementScript.IsGrounded())
        {
            isLanding = true;
            movementScript.OnLandingStart();
            animatorController.PlayState(PlayerAnimState.pousando);
            return;
        }
        if (animatorController.GetCurrentAnimatorStateInfo(0).IsName("dano")) return;

        // A L�GICA DE RECARGA FOI COMPLETAMENTE REMOVIDA DAQUI.

        // PRIORIDADE 3: Modo de Mira
        if (isInAimMode)
        {
            if (movementScript.IsGrounded())
            {
                if (movementScript.IsMoving())
                    animatorController.PlayState(PlayerAnimState.andarCotoco);
                else
                    animatorController.PlayState(PlayerAnimState.paradoCotoco);
            }
            else // No ar
            {
                if (movementScript.GetVerticalVelocity() > 0.1f)
                    animatorController.PlayState(PlayerAnimState.pulandoCotoco);
                else
                    animatorController.PlayState(PlayerAnimState.falling);
            }
            return;
        }

        // PRIORIDADE 4: Movimento Normal
        if (!movementScript.IsGrounded())
        {
            if (movementScript.IsWallSliding())
                animatorController.PlayState(PlayerAnimState.derrapagem);
            else if (movementScript.IsInParabolaArc() || movementScript.IsDashing())
                animatorController.PlayState(PlayerAnimState.dashAereo);
            else if (movementScript.GetVerticalVelocity() > 0.1f)
                animatorController.PlayState(PlayerAnimState.pulando);
            else
                animatorController.PlayState(PlayerAnimState.falling);
            return;
        }

        // PRIORIDADE 5: Movimento no Ch�o
        if (movementScript.IsDashing())
            animatorController.PlayState(PlayerAnimState.dash);
        else if (movementScript.IsMoving())
            animatorController.PlayState(PlayerAnimState.andando);
        else // Parado
        {
            if (playerStats.IsHealthLow())
                animatorController.PlayState(PlayerAnimState.poucaVidaParado);
            else
                animatorController.PlayState(PlayerAnimState.parado);
        }
    }

    private void HandlePowerModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetPowerMode(!isPowerModeActive);
        }
    }

    private void SetPowerMode(bool isActive)
    {
        isPowerModeActive = isActive;
        activeJumpSkill = isPowerModeActive ? upgradedJumpSkill : baseJumpSkill;
        activeDashSkill = isPowerModeActive ? upgradedDashSkill : baseDashSkill;
        if (powerModeIndicator != null)
        {
            powerModeIndicator.SetActive(isActive);
        }
        Debug.Log("Power Mode Ativo: " + isPowerModeActive);
    }

    public SkillSO GetActiveJumpSkill()
    {
        return activeJumpSkill;
    }
    public void OnLandingAnimationEnd()
    {
        Debug.Log("Anima��o de pouso TERMINOU. Liberando o jogador.");
        isLanding = false;
        movementScript.OnLandingComplete();
    }
}