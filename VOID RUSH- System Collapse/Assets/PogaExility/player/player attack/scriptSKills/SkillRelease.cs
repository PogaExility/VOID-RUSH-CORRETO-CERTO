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
        // --- LÓGICA DE DASH ---
        if (skill.movementSkillType == MovementSkillType.Dash)
        {
            if (movement.IsDashing()) return false;
            if (!movement.IsGrounded() && !movement.IsWallSliding() && !skill.canDashInAir) return false;

            Vector2 dashDirection;
            if (movement.IsWallSliding())
            {
                dashDirection = movement.GetWallEjectDirection();
                if ((dashDirection.x > 0 && !movement.IsFacingRight()) || (dashDirection.x < 0 && movement.IsFacingRight()))
                {
                    movement.Flip();
                }
            }
            else
            {
                dashDirection = movement.GetDashDirection();
            }

            if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
            currentDashCoroutine = StartCoroutine(ExecuteDashCoroutine(skill, movement, dashDirection));
            return true;
        }
        // --- LÓGICA DE PULO ---
        else if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsWallJumping()) return false;
            if (movement.IsGrounded()) { currentAirJumps = skill.airJumps; }

            // Lógica para pular durante um dash terrestre
            if (movement.IsDashing() && movement.IsGrounded())
            {
                if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
                movement.OnDashEnd();
                movement.InhibitAirDeceleration = true; // Ativa a inibição
                movement.DoJump(skill.jumpHeightMultiplier); // Pula
                return true;
            }

            // Lógica Padrão de Pulo e Parede
            if (!movement.IsGrounded() && movement.IsTouchingWall() && !movement.IsWallSliding())
            {
                movement.StartWallSlide();
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
                movement.InhibitAirDeceleration = false; // Garante que um pulo normal tenha atrito
                movement.DoJump(skill.jumpHeightMultiplier);
                return true;
            }
        }
        return false;
    }

    private IEnumerator ExecuteWallJumpCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.InhibitAirDeceleration = false;
        movement.DoWallJump(skill.jumpHeightMultiplier);
        yield return new WaitForSeconds(0.3f);
        movement.OnWallJumpEnd();
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, Vector2 direction)
    {
        movement.OnDashStart();
        float verticalSpeed = movement.GetVerticalVelocity();
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.01f;

        // Se for um dash terrestre, a inibição está desligada no início.
        // Se for um dash aéreo, a inibição também deve estar desligada.
        movement.InhibitAirDeceleration = false;

        if (skill.ignoresGravity)
        {
            movement.SetGravityScale(0f);
            movement.SetVelocity(direction.x * skill.dashSpeed, 0f);
        }
        else
        {
            movement.SetGravityScale(movement.baseGravity);
            movement.SetVelocity(direction.x * skill.dashSpeed, verticalSpeed);
        }

        yield return new WaitForSeconds(dashDuration);

        movement.OnDashEnd();

        // Se era um dash terrestre que terminou no ar, ATIVA a inibição.
        if (!skill.canDashInAir && !movement.IsGrounded())
        {
            movement.InhibitAirDeceleration = true;
        }
    }
}