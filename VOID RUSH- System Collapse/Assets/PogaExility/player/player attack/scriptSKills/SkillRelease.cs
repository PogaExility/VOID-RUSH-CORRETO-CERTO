using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(AdvancedPlayerMovement2D))]
public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;
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
        SkillSO activeJumpSkill = playerController.GetActiveJumpSkill();
        if (activeJumpSkill != null && movement.IsGrounded())
        {
            currentAirJumps = activeJumpSkill.airJumps;
        }
    }

    public bool TryActivateSkill(SkillSO skill)
    {
        if (skill == null || !CheckKeyPress(skill) || currentSkillCoroutine != null) return false;

        bool success = false;
        switch (skill.movementSkillType)
        {
            case MovementSkillType.SuperJump:
                if (!movement.IsWallSliding() && (movement.CanJumpFromGround() || currentAirJumps > 0))
                {
                    if (!movement.CanJumpFromGround()) currentAirJumps--;
                    movement.DoJump(skill.jumpForce / movement.jumpForce);
                    success = true;
                }
                break;
            case MovementSkillType.Dash:
            case MovementSkillType.WallDash:
                if (movement.IsDashing()) break;
                if (!movement.IsWallSliding() && skill.movementSkillType == MovementSkillType.WallDash) break;
                if (movement.IsWallSliding() && skill.movementSkillType == MovementSkillType.Dash) break;
                currentSkillCoroutine = StartCoroutine(ExecuteDashCoroutine(skill));
                success = true;
                break;
            case MovementSkillType.WallJump:
                if (!movement.IsWallSliding()) break;
                movement.DoWallJump(skill.wallJumpForce.y / movement.wallJumpForce.y);
                success = true;
                break;
            case MovementSkillType.WallSlide:
                if (movement.IsGrounded() || !movement.IsTouchingWall() || movement.IsWallSliding()) break;
                movement.StartWallSlide();
                success = true;
                break;
            case MovementSkillType.WallDashJump:
                if (!movement.IsWallSliding()) break;
                movement.DoWallDashJump(skill.wallDashJump_LaunchForceX, skill.wallDashJump_LaunchForceY);
                currentSkillCoroutine = StartCoroutine(WaitForParabolaEnd());
                success = true;
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

    private IEnumerator ExecuteDashCoroutine(SkillSO skill)
    {
        movement.OnDashStart();
        movement.SetGravityScale(0f);
        Vector2 direction = movement.GetFacingDirection();
        if ((direction.x > 0 && !movement.IsFacingRight()) || (direction.x < 0 && movement.IsFacingRight()))
        {
            movement.Flip();
        }
        float timer = 0f;
        while (timer < skill.dashDuration)
        {
            if (movement.IsTouchingWall()) break;
            movement.SetVelocity(direction.x * skill.dashSpeed, 0);
            timer += Time.deltaTime;
            yield return null;
        }
        movement.SetVelocity(0, 0);
        movement.SetGravityScale(movement.baseGravity);
        movement.OnDashEnd();
        currentSkillCoroutine = null;
    }

    private IEnumerator WaitForParabolaEnd()
    {
        yield return new WaitUntil(() => !movement.IsInParabolaArc());
        movement.OnWallJumpEnd();
        currentSkillCoroutine = null;
    }
}