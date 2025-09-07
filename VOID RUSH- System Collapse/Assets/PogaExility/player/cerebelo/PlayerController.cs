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

        // --- ADICIONE ESTA LINHA ---
        // Trava ou destrava o flip por movimento com base no estado de mira.
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

        // --- CHAMADAS DAS FUN��ES DE L�GICA ---
        HandlePowerModeToggle();
        HandleSkillInput();
        HandleCombatInput();
        HandleWeaponSwitching();// <-- CHAMADA AQUI
       
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

        // Apenas UMA fun��o � chamada, sem argumento.
        if (scrollInput > 0f)
        {
            weaponHandler.CycleWeapon(); // <<-- CORRE��O
        }
        // Opcional: Para rodar pra tr�s, precisaria de outra fun��o.
        // Por agora, qualquer scroll roda pra frente.
        else if (scrollInput < 0f)
        {
            weaponHandler.CycleWeapon(); // <<-- CORRE��O
        }
    }
    private void ToggleInventory()
    {
        if (inventoryLocked) return;

        isInventoryOpen = !isInventoryOpen;

        // Ativa/Desativa os pain�is
        inventoryPanel.SetActive(isInventoryOpen);
        if (combatHUDPanel != null) // Linha de seguran�a
            combatHUDPanel.SetActive(!isInventoryOpen); // <<-- ADICIONE ESTA LINHA (note o "!")

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
        // 1. O JOGADOR APERTOU A TECLA DO DASH?
        // Lemos a tecla do SO da skill de dash ativa.
        if (activeDashSkill.triggerKeys.Any(key => Input.GetKeyDown(key)))
        {
            // Se sim, inicia o buffer e N�O FAZ MAIS NADA NESTE FRAME.
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
        if (Input.GetButton("Fire1")) // Para clicar ou segurar
        {
            weaponHandler.HandleAttackInput();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponHandler.HandleReloadInput();
        }

        // Defesa continua igual
        if (Input.GetKeyDown(KeyCode.F))
        {
            defenseHandler.StartBlock(blockSkill);
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            defenseHandler.EndBlock();
        }
    }
    // EM PlayerController.cs

    private void UpdateAnimations()
    {
        // --- PARTE 1: INFORMAR O ANIMATOR ---
        // O script agora apenas envia os "fatos" para o Animator a cada frame.
        // O Animator, com suas transi��es, far� todo o trabalho de escolher a anima��o.
        Animator anim = animatorController.GetAnimator();
        anim.SetBool("IsAiming", isInAimMode);
        anim.SetBool("IsGrounded", movementScript.IsGrounded());
        anim.SetFloat("VerticalVelocity", movementScript.GetVerticalVelocity());
        anim.SetBool("IsMoving", movementScript.IsMoving());
        anim.SetBool("IsWallSliding", movementScript.IsWallSliding());
        anim.SetBool("IsDashing", movementScript.IsDashing());

        // --- PARTE 2: LIDAR COM ESTADOS DE ALTA PRIORIDADE (OVERRIDE) ---
        // Estas s�o exce��es que precisam FOR�AR uma anima��o, pois s�o eventos �nicos.

        // PRIORIDADE M�XIMA: Morte - para tudo e toca a anima��o de morte.
        if (playerStats.IsDead())
        {
            animatorController.PlayState(PlayerAnimState.morrendo);
            return;
        }

        // PRIORIDADE 2: Pouso - se a anima��o de pouso j� est� tocando, n�o a interrompa.
        if (isLanding)
        {
            return;
        }
        // Se o jogador ACABOU de pousar, for�a a anima��o de pouso.
        if (!wasGroundedLastFrame && movementScript.IsGrounded())
        {
            isLanding = true;
            movementScript.OnLandingStart();
            animatorController.PlayState(PlayerAnimState.pousando);
            return;
        }

        // PRIORIDADE 3: Dano - se a anima��o de dano est� tocando, n�o a interrompa.
        if (animatorController.GetCurrentAnimatorStateInfo(0).IsName("dano"))
        {
            return;
        }

        // Com o novo sistema, o resto da l�gica de `if/else` para escolher anima��es
        // de movimento (parado, andando, pulando, cotoco, etc.) foi completamente removido
        // e agora � gerenciado pelo Animator Controller, o que resolve os bugs.
    

        // PRIORIDADE 6: A��es no Ch�o (Movimento)
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
            // PRIORIDADE M�NIMA: Parado ou Parado com Pouca Vida
             if (playerStats.IsHealthLow()) // (Voc� precisar� de uma fun��o como esta)
             {
                 animatorController.PlayState(PlayerAnimState.poucaVidaParado);
             }
             else
            {
            animatorController.PlayState(PlayerAnimState.parado);
             }
        }
    }
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
    // Em PlayerController.cs
    public void OnLandingAnimationEnd()
    {
        Debug.Log("Anima��o de pouso TERMINOU. Liberando o jogador.");
        isLanding = false; // Libera a trava da anima��o
        movementScript.OnLandingComplete(); // Libera a f�sica do personagem
    }

}