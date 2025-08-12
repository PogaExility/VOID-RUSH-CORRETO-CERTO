using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;
    private Coroutine currentDashCoroutine;
    private bool canInitiateWallSlide = false;

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

            // Wall Dash tem prioridade se estiver deslizando
            if (movement.IsWallSliding())
            {
                if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
                currentDashCoroutine = StartCoroutine(ExecuteWallDashCoroutine(skill, movement));
                return true;
            }

            // O Dash pode ser usado durante um Wall Jump para cancelar e dar um novo impulso
            if (!movement.IsGrounded() && !skill.canDashInAir) return false;

            if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
            currentDashCoroutine = StartCoroutine(ExecuteDashCoroutine(skill, movement));
            return true;
        }
        // --- LÓGICA DE PULO ---
        else if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            // O pulo PODE cancelar um dash
            if (movement.IsWallJumping()) return false;
            if (movement.IsGrounded()) { currentAirJumps = skill.airJumps; canInitiateWallSlide = false; }

            // Lógica de "Agarrar"
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
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.01f;

        if (skill.ignoresGravity)
        {
            movement.SetGravityScale(0f);
            movement.SetVelocity(movement.GetDashDirection().x * skill.dashSpeed, movement.jumpForce * 0.75f);
        }
        else
        {
            movement.SetGravityScale(movement.baseGravity);
            movement.SetVelocity(movement.GetDashDirection().x * skill.dashSpeed, movement.jumpForce * 0.5f);
        }
        yield return new WaitForSeconds(dashDuration);
        movement.OnDashEnd();
    }

    private IEnumerator ExecuteWallDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.OnDashStart();
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.1f;
        Vector2 ejectDirection = movement.GetWallEjectDirection();
        if ((ejectDirection.x > 0 && !movement.IsFacingRight()) || (ejectDirection.x < 0 && movement.IsFacingRight())) movement.Flip();
        movement.SetVelocity(ejectDirection.x * skill.dashSpeed, 0f);
        yield return new WaitForSeconds(dashDuration);
        movement.OnDashEnd();
        movement.SetGravityScale(movement.baseGravity);
        movement.SetVelocity(0, 0);
    }
}