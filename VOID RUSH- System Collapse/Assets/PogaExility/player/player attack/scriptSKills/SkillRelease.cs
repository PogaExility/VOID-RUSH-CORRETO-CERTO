using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;

    public bool ActivateSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill == null || movement.IsInAction()) return false;

        if (skill.skillClass == SkillClass.Movimento)
        {
            return HandleMovementSkill(skill, movement, animator);
        }
        return false;
    }

    private bool HandleMovementSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsGrounded() || movement.IsWallSliding()) currentAirJumps = skill.airJumps;

            if (movement.IsWallSliding())
            {
                StartCoroutine(ExecuteWallJumpCoroutine(skill, movement, animator));
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
            if (!movement.IsGrounded() && !skill.canDashInAir) return false;
            StartCoroutine(ExecuteDashCoroutine(skill, movement, animator));
            return true;
        }
        return false;
    }

    private IEnumerator ExecuteWallJumpCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        movement.StartAction(); // BLOQUEIA CONTROLE
        movement.DoWallJump(skill.jumpHeightMultiplier);
        yield return new WaitForSeconds(0.3f);
        movement.EndAction(); // LIBERA CONTROLE
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        movement.StartAction(); // BLOQUEIA CONTROLE

        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.01f;
        Vector2 dashDirection = movement.GetDashDirection();
        RaycastHit2D hit = Physics2D.Raycast(movement.transform.position, dashDirection, skill.dashDistance, movement.collisionLayer);
        if (hit.collider != null) { dashDuration = (hit.distance / skill.dashSpeed) * 0.9f; }

        float verticalSpeed = movement.GetVerticalVelocity();

        if (skill.ignoresGravity)
        {
            movement.SetGravityScale(0f);
            movement.SetVelocity(dashDirection.x * skill.dashSpeed, verticalSpeed);
        }
        else
        {
            movement.SetGravityScale(movement.baseGravity);
            movement.SetVelocity(dashDirection.x * skill.dashSpeed, 0);
        }

        yield return new WaitForSeconds(dashDuration);

        movement.SetGravityScale(movement.baseGravity);
        movement.SetVelocity(0, movement.GetVerticalVelocity());
        movement.EndAction(); // LIBERA CONTROLE
    }

    private IEnumerator ExecuteWallDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        movement.StartAction(); // BLOQUEIA CONTROLE

        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.1f;
        Vector2 ejectDirection = movement.GetWallEjectDirection();
        if ((ejectDirection.x > 0 && !movement.IsFacingRight()) || (ejectDirection.x < 0 && movement.IsFacingRight())) movement.Flip();
        movement.SetVelocity(ejectDirection.x * skill.dashSpeed, 0f);
        yield return new WaitForSeconds(dashDuration);
        movement.SetGravityScale(movement.baseGravity);
        movement.SetVelocity(0, 0);

        movement.EndAction(); // LIBERA CONTROLE
    }
}