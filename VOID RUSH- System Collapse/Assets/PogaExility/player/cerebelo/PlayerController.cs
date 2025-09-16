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

    public bool IsAttacking { get; set; }
    private bool isInventoryOpen = false;
    private SkillSO activeJumpSkill;
    private SkillSO activeDashSkill;
    private bool isPowerModeActive = false;
    private bool wasGroundedLastFrame = true;
    private bool isLanding = false;
    private bool isInAimMode = false;
    private PlayerStats playerStats;
    public bool inventoryLocked = false;
    private PlayerAnimState previousBodyState;
    private bool isActionInterruptingAim = false;
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
        if (inventoryPanel != null) { inventoryPanel.SetActive(false); isInventoryOpen = false; }
        if (cursorManager != null) cursorManager.SetDefaultCursor();
        if (animatorController != null) animatorController.SetAimLayerWeight(0);
    }

    public void SetAimingState(bool isNowAiming)
    {
        // Se j� estamos no estado desejado, n�o faz nada para evitar loops.
        if (isInAimMode == isNowAiming) return;

        isInAimMode = isNowAiming;

        // Comanda os outros componentes
        movementScript.allowMovementFlip = !isNowAiming;
        animatorController.SetAimLayerWeight(isNowAiming ? 1f : 0f);
        weaponHandler.UpdateAimVisuals(isNowAiming);
    }
    public void SetAimingStateVisuals(bool isNowAiming)
    {
        movementScript.allowMovementFlip = !isNowAiming;
        animatorController.SetAimLayerWeight(isNowAiming ? 1f : 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { ToggleInventory(); }
        if (isInventoryOpen) { movementScript.SetMoveInput(0); return; }

        movementScript.SetMoveInput(Input.GetAxisRaw("Horizontal"));
        HandlePowerModeToggle();
        HandleSkillInput();
        HandleCombatInput();
        HandleWeaponSwitching();
        UpdateAnimations();
        wasGroundedLastFrame = movementScript.IsGrounded();
        if (Input.GetKeyUp(KeyCode.Space)) movementScript.CutJump();
    }


    private Coroutine lungeCoroutine;

    public void PerformLunge(float distance, float duration)
    {
        // Garante que qualquer lunge anterior seja parado antes de iniciar um novo.
        if (lungeCoroutine != null)
        {
            StopCoroutine(lungeCoroutine);
        }
        lungeCoroutine = StartCoroutine(LungeCoroutine(distance, duration));
    }

    public void CancelLunge()
    {
        if (lungeCoroutine != null)
        {
            StopCoroutine(lungeCoroutine);
            movementScript.EnablePhysicsControl(); // Libera o controle da f�sica imediatamente.
            lungeCoroutine = null;
        }
    }

    private IEnumerator LungeCoroutine(float distance, float duration)
    {
        // 1. Remove o controle do jogador e prepara o lunge.
        movementScript.DisablePhysicsControl();

        // 2. Calcula a velocidade necess�ria para cobrir a dist�ncia no tempo desejado.
        float speed = distance / duration;
        Vector2 direction = movementScript.GetFacingDirection();

        // 3. Aplica a velocidade. A velocidade vertical do Rigidbody � preservada.
        movementScript.SetVelocity(direction.x * speed, movementScript.GetRigidbody().linearVelocity.y);

        // 4. Aguarda a dura��o do lunge.
        yield return new WaitForSeconds(duration);

        // 5. Zera a velocidade horizontal e devolve o controle ao jogador.
        movementScript.SetVelocity(0, movementScript.GetRigidbody().linearVelocity.y);
        movementScript.EnablePhysicsControl();
        lungeCoroutine = null;
    }

    private void HandleWeaponSwitching()
    {
        // ADICIONE ESTA LINHA NO TOPO DA FUN��O
        // Impede a troca de armas durante a recarga.
        if (weaponHandler.IsReloading) return;

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
        if (weaponHandler.IsReloading) return;

        // ADICIONE a verifica��o "!IsAttacking" aqui.
        if (Input.GetButton("Fire1") && !IsAttacking)
        {
            weaponHandler.HandleAttackInput();
        }

        // ... o resto da fun��o permanece igual ...
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

    // Em PlayerController.cs

    private void UpdateAnimations()
    {
        // --- ETAPA 1: DETERMINAR O ESTADO DESEJADO DA BASE LAYER ---
        PlayerAnimState desiredState;
        if (IsAttacking) return;

        if (playerStats.IsDead()) { desiredState = PlayerAnimState.morrendo; }
        else if (isLanding) { desiredState = PlayerAnimState.pousando; }
        else if (animatorController.GetCurrentAnimatorStateInfo(AnimatorTarget.PlayerBody, 0).IsName("dano")) { desiredState = PlayerAnimState.dano; }
        else if (!movementScript.IsGrounded())
        {
            if (movementScript.IsWallSliding()) desiredState = PlayerAnimState.derrapagem;
            else if (movementScript.IsDashing()) desiredState = PlayerAnimState.dashAereo;
            else if (movementScript.GetVerticalVelocity() > 0.1f) desiredState = PlayerAnimState.pulando;
            else desiredState = PlayerAnimState.falling;
        }
        else // No ch�o
        {
            if (movementScript.IsDashing()) desiredState = PlayerAnimState.dash;
            else if (movementScript.IsMoving()) desiredState = PlayerAnimState.andando;
            else
            {
                if (playerStats.IsHealthLow()) desiredState = PlayerAnimState.poucaVidaParado;
                else desiredState = PlayerAnimState.parado;
            }
        }

        // --- ETAPA 2: L�GICA DE MEM�RIA E INTERRUP��O (O C�REBRO) ---
        bool isDesiredStateAnAction = IsActionState(desiredState);

        if (isDesiredStateAnAction)
        {
            isActionInterruptingAim = true;
            SetAimingState(false);
        }
        else
        {
            if (isActionInterruptingAim)
            {
                isActionInterruptingAim = false;
                if (weaponHandler.IsAimWeaponEquipped())
                {
                    SetAimingState(true);
                }
            }
        }

        // --- ETAPA 3: TOCAR A ANIMA��O CORRETA ---
        if (isInAimMode)
        {
            // A CotocoLayer agora tem a l�gica completa
            if (!movementScript.IsGrounded())
            {
                if (movementScript.GetVerticalVelocity() > 0.1f)
                    animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.pulandoCotoco, 1);
                else
                    animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.fallingCotoco, 1);
            }
            else if (movementScript.IsMoving())
            {
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.andarCotoco, 1);
            }
            else
            {
                animatorController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.paradoCotoco, 1);
            }
        }
        else
        {
            animatorController.PlayState(AnimatorTarget.PlayerBody, desiredState, 0);
        }
    }

    private bool IsActionState(PlayerAnimState state)
    {
        switch (state)
        {
            case PlayerAnimState.dash:
            case PlayerAnimState.dashAereo:
            case PlayerAnimState.pousando:
            case PlayerAnimState.derrapagem:
            case PlayerAnimState.dano:
            case PlayerAnimState.flip:
            case PlayerAnimState.block:
            case PlayerAnimState.parry:
            case PlayerAnimState.morrendo:
                return true;
            default:
                return false;
        }
    }

    public void OnActionAnimationComplete()
    {
        // Pergunta para o WeaponHandler se a arma atual AINDA � uma arma de mira.
        if (weaponHandler.IsAimWeaponEquipped())
        {
            // Se for, REATIVA o modo de mira.
            SetAimingState(true);
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