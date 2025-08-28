using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(AdvancedPlayerMovement2D), typeof(PlayerController))]
public class SkillRelease : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private int currentAirJumps;
    private Coroutine currentSkillCoroutine;

    private AdvancedPlayerMovement2D movement;
    private PlayerController playerController;

    void Awake()
    {
        movement = GetComponent<AdvancedPlayerMovement2D>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        SkillSO baseJumpSkill = playerController.baseJumpSkill;
        if (baseJumpSkill == null) return;

        // --- CORREÇÃO CS1955: Usamos a propriedade IsGrounded, sem parênteses () ---
        if (movement.IsGrounded)
        {
            currentAirJumps = baseJumpSkill.airJumps;
        }

        // A lógica do Wall Slide foi movida para uma skill ativa, conforme sua definição.
    }

    public bool TryActivateSkill(SkillSO skill)
    {
        if (skill == null || !CheckKeyPress(skill) || (currentSkillCoroutine != null && !skill.canInterrupt))
        {
            return false;
        }

        bool success = false;
        switch (skill.movementSkillType)
        {
            case MovementSkillType.SuperJump:
                success = HandleSuperJump(skill);
                break;
            case MovementSkillType.Dash:
                success = HandleDash(skill);
                break;
            case MovementSkillType.WallJump:
                success = HandleWallJump(skill);
                break;
            case MovementSkillType.WallDash:
                success = HandleWallDash(skill);
                break;
            case MovementSkillType.WallDashJump:
                success = HandleWallDashJump(skill);
                break;
            case MovementSkillType.WallSlide:
                success = HandleWallSlide(skill);
                break;
        }

        return success;
    }

    private bool CheckKeyPress(SkillSO skill)
    {
        bool triggerPressed = skill.triggerKeys.Any(key => Input.GetKeyDown(key));
        if (!triggerPressed) return false;
        return skill.requiredKeys.All(key => Input.GetKey(key));
    }

    // --- Funções de Lógica Específicas (Corrigidas) ---

    private bool HandleSuperJump(SkillSO skill)
    {
        // --- CORREÇÃO CS1955: IsWallSliding ---
        if (movement.IsWallSliding) return false;

        // --- CORREÇÃO CS1061: CanJumpFromGround ---
        if (movement.CanJumpFromGround() || currentAirJumps > 0)
        {
            if (!movement.CanJumpFromGround())
            {
                currentAirJumps--;
            }
            // --- CORREÇÃO CS1061: DoJump ---
            movement.DoJump(skill.jumpForce);
            return true;
        }
        return false;
    }

    private bool HandleDash(SkillSO skill)
    {
        // --- CORREÇÃO CS1955: IsGrounded ---
        if (skill.dashType == DashType.Normal && !movement.IsGrounded) return false;

        currentSkillCoroutine = StartCoroutine(ExecuteDashCoroutine(skill));
        return true;
    }

    private bool HandleWallJump(SkillSO skill)
    {
        // --- CORREÇÃO CS1955: IsWallSliding ---
        if (!movement.IsWallSliding) return false;

        // --- CORREÇÃO CS1503 e CS1061: Passando o Vector2 correto para DoWallJump ---
        movement.DoWallJump(skill.wallJumpForce);
        return true;
    }

    private bool HandleWallDash(SkillSO skill)
    {
        // --- CORREÇÃO CS1955: IsWallSliding ---
        if (!movement.IsWallSliding) return false;

        currentSkillCoroutine = StartCoroutine(ExecuteDashCoroutine(skill));
        return true;
    }

    private bool HandleWallDashJump(SkillSO skill)
    {
        // --- CORREÇÃO CS1955: IsWallSliding ---
        if (!movement.IsWallSliding) return false;

        // --- CORREÇÃO CS7036: Passando todos os parâmetros necessários ---
        movement.DoWallDashJump(skill.launchForceX, skill.launchForceY, skill.parabolaLinearDamping);
        currentSkillCoroutine = StartCoroutine(WaitForParabolaEnd());
        return true;
    }

    private bool HandleWallSlide(SkillSO skill)
    {
        // --- CORREÇÃO CS1955: IsGrounded, IsTouchingWall, IsWallSliding ---
        if (movement.IsGrounded || !movement.IsTouchingWall || movement.IsWallSliding) return false;

        // --- CORREÇÃO CS7036: Passando o parâmetro slideSpeed ---
        movement.StartWallSlide(skill.wallSlideSpeed);
        return true;
    }

    // --- Corotinas que Executam as Ações Físicas (Corrigidas) ---

    private IEnumerator ExecuteDashCoroutine(SkillSO skill)
    {
        movement.OnDashStart();
        movement.SetGravityScale(0f);

        Vector2 direction = movement.GetFacingDirection();

        float timer = 0f;
        while (timer < skill.dashDuration)
        {
            // --- CORREÇÃO CS1955: IsTouchingWall ---
            if (movement.IsTouchingWall) break;

            movement.SetVelocity(direction.x * skill.dashSpeed, 0);
            timer += Time.deltaTime;
            yield return null;
        }

        movement.SetGravityScale(movement.baseGravity);
        movement.OnDashEnd();
        currentSkillCoroutine = null;
    }

    private IEnumerator WaitForParabolaEnd()
    {
        // --- CORREÇÃO CS1955: IsInParabola ---
        yield return new WaitUntil(() => !movement.IsInParabola);
        currentSkillCoroutine = null;
    }
}