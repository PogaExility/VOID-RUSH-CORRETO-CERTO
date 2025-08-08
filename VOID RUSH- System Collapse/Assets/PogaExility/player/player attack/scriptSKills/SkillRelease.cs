using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;

    public bool ActivateSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill == null || movement.IsDashing()) return false;

        bool success = false;
        if (skill.skillClass == SkillClass.Movimento)
        {
            success = HandleMovementSkill(skill, movement, animator);
        }
        return success;
    }

    private bool HandleMovementSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            // Reseta os pulos aéreos se estiver no chão ou parede
            if (movement.IsGrounded() || movement.IsWallSliding())
            {
                currentAirJumps = skill.airJumps;
            }

            // TENTA UM WALL JUMP PRIMEIRO (MAIOR PRIORIDADE)
            if (movement.IsWallSliding())
            {
                movement.DoWallJump(skill.jumpHeightMultiplier);
                animator.SetJumping(true);
                return true;
            }
            // TENTA UM PULO DO CHÃO/COYOTE TIME
            else if (movement.CanJumpFromGround())
            {
                movement.DoJump(skill.jumpHeightMultiplier);
                animator.SetJumping(true);
                return true;
            }
            // TENTA UM PULO AÉREO
            else if (currentAirJumps > 0)
            {
                currentAirJumps--;
                movement.DoJump(skill.jumpHeightMultiplier);
                animator.SetJumping(true);
                return true;
            }
        }
        else if (skill.movementSkillType == MovementSkillType.Dash)
        {
            if (movement.IsWallSliding())
            {
                StartCoroutine(ExecuteWallDashCoroutine(skill, movement, animator));
                return true;
            }
            else
            {
                if (!movement.IsGrounded() && !skill.canDashInAir) return false;
                StartCoroutine(ExecuteDashCoroutine(skill, movement, animator));
                return true;
            }
        }

        // Se nenhuma condição de pulo/dash for satisfeita
        return false;
    }

    // Coroutines de Dash (sem alterações)
    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        animator.SetDashing(true);
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.1f;
        movement.OnDashStart();
        movement.SetVelocity(movement.GetDashDirection().x * skill.dashSpeed, 0f);
        yield return new WaitForSeconds(dashDuration);
        movement.OnDashEnd();
        animator.SetDashing(false);
    }

    private IEnumerator ExecuteWallDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        animator.SetDashing(true);
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.1f;
        movement.OnDashStart(true);
        movement.SetVelocity(movement.GetWallEjectDirection().x * skill.dashSpeed, 0f);
        yield return new WaitForSeconds(dashDuration);
        movement.OnDashEnd();
        animator.SetDashing(false);
    }
}