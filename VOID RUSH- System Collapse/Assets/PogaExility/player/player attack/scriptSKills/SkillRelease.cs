using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;
    private bool canInitiateWallSlide = false;

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
            if (movement.IsWallJumping() || movement.IsDashing()) return false;
            if (movement.IsGrounded()) { currentAirJumps = skill.airJumps; canInitiateWallSlide = false; }
            if (movement.IsTouchingWall() && !movement.IsGrounded() && canInitiateWallSlide)
            {
                movement.StartWallSlide();
                canInitiateWallSlide = false;
                return true;
            }
            if (movement.IsWallSliding())
            {
                StartCoroutine(ExecuteWallJumpCoroutine(skill, movement));
                return true;
            }
            else if (movement.CanJumpFromGround() || currentAirJumps > 0)
            {
                if (!movement.CanJumpFromGround()) currentAirJumps--;
                movement.DoJump(skill.jumpHeightMultiplier);
                canInitiateWallSlide = true;
                return true;
            }
        }
        else if (skill.movementSkillType == MovementSkillType.Dash)
        {
            if (movement.IsDashing()) return false;
            if (movement.IsWallSliding())
            {
                StartCoroutine(ExecuteWallDashCoroutine(skill, movement));
                return true;
            }
            else
            {
                if (!movement.IsGrounded() && !skill.canDashInAir) return false;
                StartCoroutine(ExecuteDashCoroutine(skill, movement));
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

    // ====================================================================
    // A LÓGICA DO DASH ASCENDENTE ESTÁ AQUI
    // ====================================================================
    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.01f;
        Vector2 dashDirection = movement.GetDashDirection();

        movement.OnDashStart(dashDirection.x * skill.dashSpeed);

        if (skill.ignoresGravity)
        {
            movement.SetGravityScale(0f);
            movement.SetVelocity(movement.GetRigidbody().linearVelocity.x, movement.jumpForce * 0.75f);
        }
        else
        {
            movement.SetGravityScale(movement.baseGravity);
        }

        yield return new WaitForSeconds(dashDuration);

        movement.OnDashEnd();
        movement.SetGravityScale(movement.baseGravity);
    }

    private IEnumerator ExecuteWallDashCoroutine(SkillSO skill, AdvancedPlayerMovement2sD movement)
    {
        Vector2 ejectDirection = movement.GetWallEjectDirection();
        float dashSpeed = ejectDirection.x * skill.dashSpeed;

        movement.OnDashStart(dashSpeed); // AGORA PASSA A VELOCIDADE

        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.1f;
        if ((ejectDirection.x > 0 && !movement.IsFacingRight()) || (ejectDirection.x < 0 && movement.IsFacingRight())) movement.Flip();

        // A velocidade já foi definida no OnDashStart, aqui só precisamos manter
        movement.SetVelocity(dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        movement.OnDashEnd();
        movement.SetGravityScale(movement.baseGravity);
    }
}