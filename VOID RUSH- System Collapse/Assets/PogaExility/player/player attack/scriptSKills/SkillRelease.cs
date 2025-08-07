using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    // Variável para controlar os pulos aéreos disponíveis
    private int currentAirJumps;

    /// <summary>
    /// Tenta ativar uma skill. Retorna TRUE se a ativação foi bem-sucedida, FALSE caso contrário.
    /// </summary>
    public bool ActivateSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        if (skill == null) return false;

        bool wasSuccessful = false;

        switch (skill.skillClass)
        {
            case SkillClass.Movimento:
                wasSuccessful = HandleMovementSkill(skill, movement, animator);
                break;

            case SkillClass.Buff:
                // Lógica de Buff viria aqui
                break;

            case SkillClass.Dano:
                // Lógica de Dano viria aqui
                break;
        }
        return wasSuccessful;
    }

    private bool HandleMovementSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        // Se a skill de pulo for usada, reseta os pulos aéreos quando o jogador está no chão ou na parede
        if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsGrounded() || movement.IsWallSliding())
            {
                // Busca a quantidade de pulos aéreos do próprio ScriptableObject
                currentAirJumps = skill.airJumps;
            }
        }

        switch (skill.movementSkillType)
        {
            case MovementSkillType.SuperJump:
                return ExecuteJump(skill, movement, animator);

            case MovementSkillType.Dash:
                StartCoroutine(ExecuteDashCoroutine(skill, movement, animator));
                return true; // Assume que o dash sempre pode ser ativado (a checagem de energia já foi feita)
        }
        return false;
    }

    private bool ExecuteJump(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        // Tenta pular do chão, parede ou usando coyote time
        if (movement.CanJump())
        {
            movement.DoJump(skill.jumpHeightMultiplier);
            animator.TriggerJump();
            return true;
        }
        // Se não puder, tenta usar um pulo aéreo
        else if (currentAirJumps > 0)
        {
            currentAirJumps--; // Gasta um pulo aéreo
            movement.DoJump(skill.jumpHeightMultiplier);
            animator.TriggerJump();
            return true;
        }

        // Se chegou até aqui, não foi possível pular
        return false;
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        // Validação para evitar divisão por zero
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.1f;
        float originalGravity = movement.GetGravityScale();

        movement.SetGravityScale(0f);
        movement.SetVelocity(movement.GetFacingDirection().x * skill.dashSpeed, 0f);
        animator.TriggerDash();

        yield return new WaitForSeconds(dashDuration);

        movement.SetGravityScale(originalGravity);
        movement.SetVelocity(0f, movement.GetVerticalVelocity()); // Para o movimento horizontal, mas mantém a queda
    }
}