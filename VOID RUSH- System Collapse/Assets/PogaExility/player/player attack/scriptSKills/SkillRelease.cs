using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;

    public bool ActivateSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill == null || movement.IsDashing()) return false;

        bool success = false;
        switch (skill.skillClass)
        {
            case SkillClass.Movimento:
                success = HandleMovementSkill(skill, movement, animator);
                break;
        }
        return success;
    }

    private bool HandleMovementSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsGrounded() || movement.IsWallSliding())
            {
                currentAirJumps = skill.airJumps;
            }
        }

        switch (skill.movementSkillType)
        {
            case MovementSkillType.SuperJump:
                return ExecuteJump(skill, movement, animator);

            case MovementSkillType.Dash:
                if (!movement.IsGrounded() && !skill.canDashInAir) return false;
                StartCoroutine(ExecuteDashCoroutine(skill, movement, animator));
                return true;
        }
        return false;
    }

    private bool ExecuteJump(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (movement.CanJump())
        {
            movement.DoJump(skill.jumpHeightMultiplier);
            animator.TriggerJump();
            return true;
        }
        else if (currentAirJumps > 0)
        {
            currentAirJumps--;
            movement.DoJump(skill.jumpHeightMultiplier);
            animator.TriggerJump();
            return true;
        }
        return false;
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.1f;

        movement.OnDashStart();
        animator.TriggerDash();

        // USA A NOVA LÓGICA DE DIREÇÃO
        movement.SetVelocity(movement.GetDashDirection().x * skill.dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        movement.OnDashEnd();
    }
}