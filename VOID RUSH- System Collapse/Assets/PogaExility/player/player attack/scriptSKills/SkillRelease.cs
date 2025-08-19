using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;
    private Coroutine currentDashCoroutine;

    public bool ActivateWallDashJump(SkillSO jumpSkill, SkillSO dashSkill, AdvancedPlayerMovement2D movement)
    {
        if (!movement.IsWallSliding()) return false;
        float horizontalForce = dashSkill.dashSpeed * 1.5f;
        float verticalForce = movement.wallJumpForce.y * jumpSkill.jumpHeightMultiplier;
        movement.DoWallDashJump(horizontalForce, verticalForce);
        return true;
    }

    private IEnumerator WallJumpEndDelay(AdvancedPlayerMovement2D movement) { yield return new WaitForSeconds(0.2f); movement.OnWallJumpEnd(); }

    public bool ActivateSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill == null) return false;
        if (skill.skillClass == SkillClass.Movimento) return HandleMovementSkill(skill, movement);
        return false;
    }

    // ===== INÍCIO DA ALTERAÇÃO: SEPARANDO A LÓGICA DE CADA SKILL =====
    private bool HandleMovementSkill(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        switch (skill.movementSkillType)
        {
            // Lógica para Dash de Chão/Aéreo
            case MovementSkillType.Dash:
                if (skill.dashType == DashType.Normal && !movement.IsGrounded()) return false; // Dash Normal só no chão
                if (movement.IsDashing()) return false;
                if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
                currentDashCoroutine = StartCoroutine(ExecuteDashCoroutine(skill, movement));
                return true;

            // Lógica para Pulo Normal e Pulo Aéreo
            case MovementSkillType.SuperJump:
                if (movement.IsWallJumping() || movement.IsWallSliding()) return false; // SuperJump não funciona na parede
                if (movement.IsGrounded()) { currentAirJumps = skill.airJumps; }
                if (movement.CanJumpFromGround() || currentAirJumps > 0)
                {
                    if (!movement.CanJumpFromGround()) currentAirJumps--;
                    movement.DoJump(skill.jumpHeightMultiplier);
                    return true;
                }
                break;

            // Lógica EXCLUSIVA para Wall Jump
            case MovementSkillType.WallJump:
                if (!movement.IsWallSliding()) return false;
                StartCoroutine(ExecuteWallJumpCoroutine(skill, movement));
                return true;

            // Lógica EXCLUSIVA para Wall Dash
            case MovementSkillType.WallDash:
                if (!movement.IsWallSliding()) return false;
                if (movement.IsDashing()) return false;
                if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
                currentDashCoroutine = StartCoroutine(ExecuteDashCoroutine(skill, movement));
                return true;
        }

        // Lógica para iniciar o Wall Slide (ainda parte do pulo geral)
        if (!movement.IsGrounded() && movement.IsTouchingWall() && !movement.IsWallSliding())
        {
            movement.StartWallSlide();
            return true;
        }

        return false;
    }
    // ===== FIM DA ALTERAÇÃO =====

    private IEnumerator ExecuteWallJumpCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement) { movement.DoWallJump(skill.jumpHeightMultiplier); yield return new WaitForSeconds(0.2f); movement.OnWallJumpEnd(); }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.OnDashStart();
        if (movement.IsWallSliding())
        {
            movement.StopWallSlide();
            float dashDuration = 0.2f;
            float timer = 0;
            movement.SetGravityScale(0f);
            Vector2 direction = movement.GetWallEjectDirection();
            if ((direction.x > 0 && !movement.IsFacingRight()) || (direction.x < 0 && movement.IsFacingRight())) { movement.Flip(); }
            while (timer < dashDuration) { movement.SetVelocity(direction.x * skill.dashSpeed, 0); timer += Time.deltaTime; yield return null; }
            movement.SetGravityScale(movement.baseGravity);
        }
        else // Dash normal (chão ou aéreo)
        {
            float dashStartTime = Time.time;
            if (skill.dashType == DashType.Aereo) { float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.01f; float timer = 0; movement.SetGravityScale(0f); while (timer < dashDuration) { if (movement.IsTouchingWall()) break; Vector2 direction = movement.GetFacingDirection(); movement.SetVelocity(direction.x * skill.dashSpeed, 0); timer += Time.deltaTime; yield return null; } movement.SetGravityScale(movement.baseGravity); }
            else { movement.SetGravityScale(movement.baseGravity); float minDashTime = 0.1f; while (true) { movement.SetVelocity(movement.GetFacingDirection().x * skill.dashSpeed, movement.GetRigidbody().linearVelocity.y); if (movement.IsTouchingWall()) break; if (Time.time > dashStartTime + minDashTime) { if (movement.IsGrounded()) { yield return new WaitForFixedUpdate(); if (movement.IsGrounded()) break; } } yield return null; } }
        }
        movement.OnDashEnd();
        currentDashCoroutine = null;
    }
}