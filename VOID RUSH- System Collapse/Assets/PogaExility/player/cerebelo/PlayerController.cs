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
    public GameObject combatHUDPanel;
    public EnergyBarController energyBar;
    public GameObject powerModeIndicator;

    [Header("Referências de Movimento e Combate")]
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
    private bool attackBuffered = false;
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
        // Se já estamos no estado desejado, não faz nada para evitar loops.
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

        // --- LÓGICA DE DETECÇÃO DE POUSO ---
        // Checa se o jogador pousou NESTE frame.
        bool isGroundedNow = movementScript.IsGrounded();
        if (isGroundedNow && !wasGroundedLastFrame)
        {
            // Se não estávamos no chão no frame passado, mas estamos agora, então acabamos de pousar.
            isLanding = true;
        }
        // --- FIM DA LÓGICA DE POUSO ---

        HandlePowerModeToggle();
        HandleSkillInput();
        HandleCombatInput();
        ProcessAttackBuffer();
        HandleWeaponSwitching();
        UpdateAnimations();

        // Atualiza o estado do frame anterior DEPOIS de toda a lógica ter rodado.
        wasGroundedLastFrame = isGroundedNow;

        if (Input.GetKeyUp(KeyCode.Space)) movementScript.CutJump();
    }




    private Coroutine lungeCoroutine;

        public void PerformLunge(float distance, float duration)
        {
            if (lungeCoroutine != null) StopCoroutine(lungeCoroutine);
            lungeCoroutine = StartCoroutine(LungeCoroutine(distance, duration));

        }

    // DENTRO DE PlayerController.cs

    // DENTRO DE PlayerController.cs

    private IEnumerator LungeCoroutine(float distance, float duration)
    {
        // Guarda de segurança
        if (distance <= 0f || duration <= 0.01f)
        {
            yield break;
        }

        Rigidbody2D rb = movementScript.GetRigidbody();

        try
        {
            // --- FASE DE EXECUÇÃO ---

            movementScript.DisablePhysicsControl();

            // Calcula a velocidade horizontal necessária para cobrir a distância na duração exata.
            // Fórmula: v = d / t
            float requiredSpeed = distance / duration;

            // Pega a velocidade vertical atual (causada pela gravidade).
            float currentVerticalSpeed = rb.linearVelocity.y;

            // Define a velocidade inicial do lunge, combinando o impulso horizontal com a velocidade vertical existente.
            rb.linearVelocity = new Vector2(movementScript.GetFacingDirection().x * requiredSpeed, currentVerticalSpeed);

            // Espera a duração do lunge. Durante este tempo, a gravidade e o atrito do Rigidbody
            // (se houver algum configurado em 'Linear Drag') agirão naturalmente.
            yield return new WaitForSeconds(duration);
        }
        finally
        {
            // --- FASE DE LIMPEZA (GARANTIDA) ---

            // Ao final, zera a velocidade HORIZONTAL para uma parada precisa, mas mantém a velocidade vertical.
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // Devolve o controle ao jogador.
            movementScript.EnablePhysicsControl();

            lungeCoroutine = null;
        }
    }

    public void CancelLunge()
        {
            if (lungeCoroutine != null)
            {
                StopCoroutine(lungeCoroutine);
                movementScript.EnablePhysicsControl(); // Libera o controle da física imediatamente.
                lungeCoroutine = null;
            }
        }

    private void HandleWeaponSwitching()
    {
        // ADICIONE ESTA LINHA NO TOPO DA FUNÇÃO
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
        // A função agora apenas chama a lógica de skills, sem se preocupar com bloqueio de combate.
        TryActivateMovementSkills();
    }

    // Crie esta nova função auxiliar
    private bool TryActivateMovementSkills()
    {
        if (skillRelease.TryActivateSkill(wallDashJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(dashJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(wallJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(wallDashSkill)) return true;
        if (skillRelease.TryActivateSkill(wallSlideSkill)) return true;
        if (skillRelease.TryActivateSkill(activeJumpSkill)) return true;
        if (skillRelease.TryActivateSkill(activeDashSkill)) return true;
        return false;
    }



    // DENTRO DE PlayerController.cs

    private void HandleCombatInput()
    {
        if (weaponHandler.IsReloading) return;

        // Pega a arma ativa para saber o tipo dela
        var activeWeapon = weaponHandler.GetActiveWeaponSlot()?.item;
        if (activeWeapon == null) return;


        // LÓGICA HÍBRIDA DE INPUT
        if (activeWeapon.weaponType == WeaponType.Meelee)
        {
            // Para MEELEE, usamos GetButtonDown para registrar um único clique.
            if (Input.GetButtonDown("Fire1"))
            {
                attackBuffered = true;
            }
        }
        else // Para Ranger, Buster, etc.
        {
            // Para RANGER, usamos GetButton para permitir segurar o botão.
            if (Input.GetButton("Fire1"))
            {
                attackBuffered = true;
            }
        }

        // A lógica de recarga e defesa continua a mesma.
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

    private void ProcessAttackBuffer()
    {
        if (!attackBuffered)
        {
            return;
        }

        // Condições para poder atacar (são as mesmas para ambos os tipos)
        bool canAttackNow = !IsAttacking &&
                            !movementScript.IsDashing() &&
                            weaponHandler.IsWeaponObjectActive();

        if (canAttackNow)
        {
            weaponHandler.HandleAttackInput();
            attackBuffered = false; // Consome o buffer
        }
    }

    private void UpdateAnimations()
    {
        // Trava de ataque meelee (continua igual)
        if (IsAttacking) return;

        // --- ETAPA 1: DETERMINAR O ESTADO DESEJADO DA BASE LAYER ---
        PlayerAnimState desiredState;

        if (playerStats.IsDead()) { desiredState = PlayerAnimState.morrendo; }
        else if (isLanding) { desiredState = PlayerAnimState.pousando; }
        else if (animatorController.GetCurrentAnimatorStateInfo(AnimatorTarget.PlayerBody, 0).IsName("dano")) { desiredState = PlayerAnimState.dano; }
        else if (!movementScript.IsGrounded())
        {
            if (movementScript.IsWallSliding()) desiredState = PlayerAnimState.derrapagem;
            else if (movementScript.IsDashing() || movementScript.IsWallDashing()) desiredState = PlayerAnimState.dashAereo;
            else if (movementScript.GetVerticalVelocity() > 0.1f) desiredState = PlayerAnimState.pulando;
            else desiredState = PlayerAnimState.falling;
        }
        else // No chão
        {
            if (movementScript.IsDashing()) desiredState = PlayerAnimState.dash;
            else if (movementScript.IsMoving()) desiredState = PlayerAnimState.andando;
            else
            {
                if (playerStats.IsHealthLow()) desiredState = PlayerAnimState.poucaVidaParado;
                else desiredState = PlayerAnimState.parado;
            }
        }

    
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

        // --- ETAPA 3: TOCAR A ANIMAÇÃO CORRETA ---
        if (isInAimMode)
        {
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
        // Pergunta para o WeaponHandler se a arma atual AINDA é uma arma de mira.
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
        Debug.Log("Animação de pouso TERMINOU. Liberando o jogador.");
        isLanding = false;
        movementScript.OnLandingComplete();
    }
}