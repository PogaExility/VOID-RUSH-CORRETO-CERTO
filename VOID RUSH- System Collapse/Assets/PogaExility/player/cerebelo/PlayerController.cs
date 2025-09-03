using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(SkillRelease))]
public class PlayerController : MonoBehaviour
{

    [Header("Referências de Gerenciamento")]
    public CursorManager cursorManager;

    [Header("Referências de UI")]
    public GameObject inventoryPanel;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

    [Header("Referências de Movimento e Combate")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public CombatController combatController;
    public PlayerAttack playerAttack;
    public DefenseHandler defenseHandler;

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

    // Variáveis de Estado
    private bool isInventoryOpen = false;
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;
    private bool isInAimMode = false;
    private bool isAimModeLocked = false;
    private PlayerStats playerStats;


    void Awake()
    {
       
        movementScript = GetComponent<AdvancedPlayerMovement2D>();
        skillRelease = GetComponent<SkillRelease>();
        combatController = GetComponent<CombatController>();
        playerAttack = GetComponent<PlayerAttack>();
        defenseHandler = GetComponent<DefenseHandler>();
        playerStats = GetComponent<PlayerStats>();
        if (animatorController == null) animatorController = GetComponent<PlayerAnimatorController>();
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
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }
        

        // Trava do modo de mira
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            isAimModeLocked = !isAimModeLocked; // Inverte o estado da trava

            if (isAimModeLocked)
            {
                // Força a postura para Firearm (ou Buster) para ativar a mira
                combatController.activeStance = WeaponType.Ranger;
            }
            else
            {
                // --- A CORREÇÃO ESTÁ AQUI ---
                // Força a postura de volta para Melee para DESATIVAR a mira
                combatController.activeStance = WeaponType.Melee;
            }
        }

        float horizontalInput = 0;
        if (!isInventoryOpen)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }
        movementScript.SetMoveInput(horizontalInput);

        if (isInventoryOpen) return;

        // --- CHAMADAS DAS FUNÇÕES DE LÓGICA ---
        HandlePowerModeToggle();
        HandleSkillInput();
        HandleCombatInput();      // <-- CHAMADA AQUI
        //UpdateAimModeState();     // <-- CHAMADA AQUI
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
    }
   

 

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
        //combatController.ProcessCombatInput();

        if (Input.GetKeyDown(KeyCode.F))
        {
            defenseHandler.StartBlock(blockSkill);
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            defenseHandler.EndBlock();
        }
    }

   /* private void UpdateAimModeState()
    {
        bool shouldAim = isAimModeLocked ||
                        ((combatController.activeStance == WeaponType.Ranger || combatController.activeStance == WeaponType.Buster)
                         && !movementScript.IsDashing() && !isLanding);

        // A checagem de mudança de estado já existe e é perfeita para isso
        if (isInAimMode != shouldAim)
        {
            isInAimMode = shouldAim;
            playerAttack.SetAiming(isInAimMode); // Isso já ativa/desativa os prefabs

            // --- ADICIONE ESTE BLOCO ---
            // Atualiza o cursor do mouse com base no novo estado
            if (cursorManager != null)
            {
                if (isInAimMode)
                {
                    cursorManager.SetAimCursor();
                }
                else
                {
                    cursorManager.SetDefaultCursor();
                }
            }
            // --- FIM DO BLOCO ---
        }*/

    
    // Em PlayerController.cs
    private void UpdateAnimations()
    {
        // --- HIERARQUIA DE PRIORIDADE ---
        // A primeira condição que for verdadeira, define a animação e a função termina.

        // PRIORIDADE MÁXIMA: Animação de Morte
        if (playerStats.IsDead()) // (Você precisará de uma função como esta no PlayerStats)
        {
            animatorController.PlayState(PlayerAnimState.morrendo);
             return;
         }

        // PRIORIDADE 2: Animação de Pouso
        if (isLanding)
        {
            // Se já estamos no processo de pouso, a animação 'pousando' já foi chamada.
            // Não fazemos nada e deixamos ela terminar. O evento de animação vai limpar 'isLanding'.
            return;
        }
        // Lógica para INICIAR o pouso
        if (!wasGroundedLastFrame && movementScript.IsGrounded())
        {
            isLanding = true;
            movementScript.OnLandingStart();
            animatorController.PlayState(PlayerAnimState.pousando);
            return; // Animação de pouso definida. Fim.
        }

        // PRIORIDADE 3: Animação de Dano
        // (A chamada para a animação de dano já está no PlayerStats.cs, o que é bom.
        // Mas precisamos impedir que as animações de movimento a substituam imediatamente).
         if (animatorController.GetCurrentAnimatorStateInfo(0).IsName("dano"))
         {
             return; // Deixa a animação de dano terminar.
         }

        // PRIORIDADE 4: Modo de Mira ("Cotoco")
        if (isInAimMode)
        {
            if (movementScript.IsMoving())
            {
                animatorController.PlayState(PlayerAnimState.andarCotoco);
            }
            else
            {
                animatorController.PlayState(PlayerAnimState.paradoCotoco);
            }
            return; // Animação de mira definida. Fim.
        }

        // PRIORIDADE 5: Ações Aéreas e de Parede (Movimento)
        if (!movementScript.IsGrounded())
        {
            if (movementScript.IsWallSliding())
            {
                animatorController.PlayState(PlayerAnimState.derrapagem);
            }
            else if (movementScript.IsInParabolaArc())
            {
                animatorController.PlayState(PlayerAnimState.dashAereo);
            }
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
            return; // Animação aérea definida. Fim.
        }

        // PRIORIDADE 6: Ações no Chão (Movimento)
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
            // PRIORIDADE MÍNIMA: Parado ou Parado com Pouca Vida
             if (playerStats.IsHealthLow()) // (Você precisará de uma função como esta)
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