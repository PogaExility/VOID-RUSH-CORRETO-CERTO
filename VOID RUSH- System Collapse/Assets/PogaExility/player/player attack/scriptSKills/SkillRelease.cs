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
            if (skill.dashType == DashType.Normal && !movement.IsGrounded()) return false;
            if (movement.IsDashing() && skill.dashType == DashType.Aereo) return false;

            if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
            currentDashCoroutine = StartCoroutine(ExecuteDashCoroutine(skill, movement));
            return true;
        }
        else if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsWallJumping()) return false;
            if (movement.IsGrounded()) { currentAirJumps = skill.airJumps; }

            // CORREÇÃO: Lógica de Wall Jump e Wall Slide simplificada e mais robusta.
            // Se estiver deslizando, pula da parede.
            if (movement.IsWallSliding())
            {
                StartCoroutine(ExecuteWallJumpCoroutine(skill, movement));
                return true;
            }
            // Se estiver no ar, tocando a parede mas ainda não deslizando, começa a deslizar.
            else if (!movement.IsGrounded() && movement.IsTouchingWall())
            {
                movement.StartWallSlide();
                return true;
            }
            // Senão, é um pulo normal ou pulo no ar.
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
        // Pequeno delay para o jogador não poder "grudar" na parede de novo instantaneamente.
        yield return new WaitForSeconds(0.2f);
        movement.OnWallJumpEnd();
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement)
    {
        movement.OnDashStart();
        float dashStartTime = Time.time;

        if (skill.dashType == DashType.Aereo)
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
            movement.SetGravityScale(movement.baseGravity);
        }
        else // DASH NORMAL
        {
            movement.SetGravityScale(movement.baseGravity);
            float minDashTime = 0.1f;
            while (true)
            {
                movement.SetVelocity(movement.GetFacingDirection().x * skill.dashSpeed, movement.GetRigidbody().linearVelocity.y);
                if (movement.IsTouchingWall()) break;
                if (Time.time > dashStartTime + minDashTime)
                {
                    if (movement.IsGrounded())
                    {
                        yield return new WaitForFixedUpdate();
                        if (movement.IsGrounded()) break;
                    }
                }
                yield return null;
            }
        }

        movement.OnDashEnd();
        currentDashCoroutine = null;
    }
}