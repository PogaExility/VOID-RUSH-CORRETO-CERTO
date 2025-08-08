using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;

    public bool ActivateSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill == null) return false;
        if (skill.skillClass == SkillClass.Movimento) return HandleMovementSkill(skill, movement);
        return false;
    }

    private bool HandleMovementSkill(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsDashing() || movement.IsWallJumping()) return false;
            if (movement.IsGrounded() || movement.IsWallSliding()) currentAirJumps = skill.airJumps;

            if (movement.IsWallSliding())
            {
                StartCoroutine(ExecuteWallJumpCoroutine(skill, movement));
                return true;
            }
            else if (movement.CanJumpFromGround() || currentAirJumps > 0)
            {
                if (!movement.CanJumpFromGround()) currentAirJumps--;
                movement.DoJump(skill.jumpHeightMultiplier);
                return true;
            }
        }
        else if (skill.movementSkillType == MovementSkillType.Dash)
        {
            if (movement.IsDashing()) return false;
            if (!movement.IsGrounded() && !skill.canDashInAir) return false;
            StartCoroutine(ExecuteDashCoroutine(skill, movement));
            return true;
        }
        return false;
    }

    private IEnumerator ExecuteWallJumpCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.DoWallJump(skill.jumpHeightMultiplier);
        yield return new WaitForSeconds(0.3f);
        movement.OnWallJumpEnd();
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.OnDashStart();
        float verticalSpeed = movement.GetVerticalVelocity();
        float dashDuration = skill.dashDistance / skill.dashSpeed;

        if (skill.ignoresGravity)
        {
            movement.SetGravityScale(0f);
            movement.SetVelocity(movement.GetDashDirection().x * skill.dashSpeed, verticalSpeed);
        }
        else
        {
            movement.SetGravityScale(movement.baseGravity);
            movement.SetVelocity(movement.GetDashDirection().x * skill.dashSpeed, 0);
        }

        yield return new WaitForSeconds(dashDuration);

        movement.OnDashEnd();
        movement.SetGravityScale(movement.baseGravity);
        movement.SetVelocity(0, movement.GetVerticalVelocity());
    }
}