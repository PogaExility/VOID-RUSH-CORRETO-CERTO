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
        // 1. Inicia o estado de IMPULSO do dash.
        movement.OnDashStart();
        float verticalSpeed = movement.GetVerticalVelocity();
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.01f;

        // 2. Aplica a velocidade inicial.
        if (skill.ignoresGravity)
        {
            movement.SetGravityScale(0f);
            movement.SetVelocity(direction.x * skill.dashSpeed, movement.jumpForce * 0.75f);
        }
        else
        {
            movement.SetGravityScale(movement.baseGravity);
            movement.SetVelocity(direction.x * skill.dashSpeed, verticalSpeed);
        }

        // 3. Espera a duração do impulso.
        yield return new WaitForSeconds(dashDuration);

        // 4. Finaliza o estado de IMPULSO.
        movement.OnDashEnd();

        // 5. Se, ao final do impulso, o jogador estiver no ar...
        if (!movement.IsGrounded())
        {
            // ...inicia o estado de "carregar momento" no script de movimento.
            movement.StartMomentumCarry();
        }
    }
}