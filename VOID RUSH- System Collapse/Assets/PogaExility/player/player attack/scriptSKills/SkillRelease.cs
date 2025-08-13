using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;
    private Coroutine currentDashCoroutine;

    public bool ActivateSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill == null) return false;
        if (skill.skillClass == SkillClass.Movimento) return HandleMovementSkill(skill, movement);
        return false;
    }

    private bool HandleMovementSkill(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        if (skill.movementSkillType == MovementSkillType.Dash)
        {
            if (movement.IsDashing()) return false;
            if (skill.dashType == DashType.Normal && !movement.IsGrounded() && !movement.IsWallSliding()) return false;

            Vector2 dashDirection = movement.GetDashDirection();
            if (movement.IsWallSliding()) { dashDirection = movement.GetWallEjectDirection(); }

            if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
            currentDashCoroutine = StartCoroutine(ExecuteDashCoroutine(skill, movement, dashDirection));
            return true;
        }
        else if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsWallJumping()) return false;
            if (movement.IsGrounded()) { currentAirJumps = skill.airJumps; }

            if (!movement.IsGrounded() && movement.IsTouchingWall() && !movement.IsWallSliding()) { movement.StartWallSlide(); return true; }
            if (movement.IsWallSliding()) { StartCoroutine(ExecuteWallJumpCoroutine(skill, movement)); return true; }
            else if (movement.CanJumpFromGround() || currentAirJumps > 0)
            {
                if (!movement.CanJumpFromGround()) currentAirJumps--;
                movement.DoJump(skill.jumpHeightMultiplier);
                return true;
            }
        }
        return false;
    }

    private IEnumerator ExecuteWallJumpCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.DoWallJump(skill.jumpHeightMultiplier);
        yield return new WaitForSeconds(0.3f);
        movement.OnWallJumpEnd();
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, Vector2 direction)
    {
        movement.OnDashStart();

        if (skill.dashType == DashType.Aereo)
        {
            float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.01f;
            float timer = 0;
            movement.SetGravityScale(0f);
            while (timer < dashDuration)
            {
                movement.SetVelocity(direction.x * skill.dashSpeed, 0);
                timer += Time.deltaTime;
                yield return null;
            }
            movement.SetGravityScale(movement.baseGravity);
        }
        else // DASH NORMAL
        {
            movement.SetGravityScale(movement.baseGravity);
            float minDashTime = 0.1f;
            float timer = 0;

            while (true)
            {
                // CONTROLE NO AR: Lê o input a cada frame.
                float horizontalInput = Input.GetAxisRaw("Horizontal");
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    direction = (horizontalInput > 0) ? Vector2.right : Vector2.left;
                }

                movement.SetVelocity(direction.x * skill.dashSpeed, movement.GetRigidbody().linearVelocity.y);

                // Condição de saída: Tocou o chão OU uma parede (após o tempo mínimo).
                if (timer > minDashTime && (movement.IsGrounded() || movement.IsTouchingWall()))
                {
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        movement.OnDashEnd();
    }
}