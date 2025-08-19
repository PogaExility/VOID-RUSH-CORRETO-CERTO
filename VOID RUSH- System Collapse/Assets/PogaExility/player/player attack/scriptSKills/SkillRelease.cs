using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    private Coroutine currentDashCoroutine;
    [Header("Combinação")]
    public float comboHMultiplier = 1.0f;
    public float comboVMultiplier = 1.0f;

    public bool ActivateWallDashJump(SkillSO jumpSkill, SkillSO dashSkill, AdvancedPlayerMovement2D movement)
    {
        if (!movement.IsWallSliding()) return false;

        float horizontalForce = dashSkill.dashSpeed * comboHMultiplier;
        float verticalForce = movement.wallJumpForce.y * jumpSkill.jumpHeightMultiplier * comboVMultiplier;

        movement.DoWallDashJump(horizontalForce, verticalForce);
        return true;
    }

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

            if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
            currentDashCoroutine = StartCoroutine(ExecuteDashCoroutine(skill, movement));
            return true;
        }
        else if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsDashing() && !movement.IsGrounded())
            {
                if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
                movement.OnDashEnd();
                return movement.TryJump(skill.jumpHeightMultiplier);
            }

            if (movement.IsWallJumping()) return false;

            if (movement.IsGrounded()) { movement.ResetAirJumps(skill.airJumps); }

            if (movement.IsWallSliding())
            {
                StartCoroutine(ExecuteWallJumpCoroutine(skill, movement));
                return true;
            }
            // Bloco removido para corrigir o bug do "freio de mão aéreo"
            // else if (!movement.IsGrounded() && movement.IsTouchingWall()) { ... }
            else
            {
                return movement.TryJump(skill.jumpHeightMultiplier);
            }
        }
        return false;
    }

    private IEnumerator ExecuteWallJumpCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.DoWallJump(skill.jumpHeightMultiplier);
        yield return new WaitForSeconds(0.2f);
        movement.OnWallJumpEnd();
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.OnDashStart();
        float initialY = movement.transform.position.y;

        if (movement.IsWallSliding())
        {
            movement.StopWallSlide();
            float dashDuration = 0.2f;
            float timer = 0;
            movement.SetGravityScale(0f);
            Vector2 direction = movement.GetWallEjectDirection();
            if ((direction.x > 0 && !movement.IsFacingRight()) || (direction.x < 0 && movement.IsFacingRight())) { movement.Flip(); }

            while (timer < dashDuration)
            {
                movement.SetVelocity(direction.x * skill.dashSpeed, 0);
                float currentX = movement.transform.position.x;
                movement.transform.position = new Vector3(currentX, initialY, movement.transform.position.z);
                timer += Time.deltaTime;
                yield return null;
            }
        }
        else if (skill.dashType == DashType.Aereo)
        {
            float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.01f;
            float timer = 0;
            movement.SetGravityScale(0f);
            while (timer < dashDuration)
            {
                if (movement.IsTouchingWall()) break;
                Vector2 direction = movement.GetFacingDirection();
                movement.SetVelocity(direction.x * skill.dashSpeed, 0);
                timer += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            float dashStartTime = Time.time;
            movement.SetGravityScale(movement.baseGravity);
            float minDashTime = 0.1f;
            while (true)
            {
                movement.SetVelocity(movement.GetFacingDirection().x * skill.dashSpeed, movement.GetRigidbody().linearVelocity.y);
                if (movement.IsTouchingWall()) break;
                if (Time.time > dashStartTime + minDashTime) { if (movement.IsGrounded()) { yield return new WaitForFixedUpdate(); if (movement.IsGrounded()) break; } }
                yield return null;
            }
        }

        movement.SetGravityScale(movement.baseGravity);
        movement.OnDashEnd();
        currentDashCoroutine = null;
    }
}