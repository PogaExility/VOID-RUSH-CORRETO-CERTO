using UnityEngine;
using System.Collections;

public class SkillRelease : MonoBehaviour
{
    // Vari�vel para controlar os pulos a�reos dispon�veis
    private int currentAirJumps;

    /// <summary>
    /// Tenta ativar uma skill. Retorna TRUE se a ativa��o foi bem-sucedida, FALSE caso contr�rio.
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
                // L�gica de Buff viria aqui
                break;

            case SkillClass.Dano:
                // L�gica de Dano viria aqui
                break;
        }
        return wasSuccessful;
    }

    private bool HandleMovementSkill(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        // Se a skill de pulo for usada, reseta os pulos a�reos quando o jogador est� no ch�o ou na parede
        if (skill.movementSkillType == MovementSkillType.SuperJump)
        {
            if (movement.IsGrounded() || movement.IsWallSliding())
            {
                // Busca a quantidade de pulos a�reos do pr�prio ScriptableObject
                currentAirJumps = skill.airJumps;
            }
        }

        switch (skill.movementSkillType)
        {
            case MovementSkillType.SuperJump:
                return ExecuteJump(skill, movement, animator);

            case MovementSkillType.Dash:
                StartCoroutine(ExecuteDashCoroutine(skill, movement, animator));
                return true; // Assume que o dash sempre pode ser ativado (a checagem de energia j� foi feita)
        }
        return false;
    }

    private bool ExecuteJump(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        // Tenta pular do ch�o, parede ou usando coyote time
        if (movement.CanJump())
        {
            movement.DoJump(skill.jumpHeightMultiplier);
            animator.TriggerJump();
            return true;
        }
        // Se n�o puder, tenta usar um pulo a�reo
        else if (currentAirJumps > 0)
        {
            currentAirJumps--; // Gasta um pulo a�reo
            movement.DoJump(skill.jumpHeightMultiplier);
            animator.TriggerJump();
            return true;
        }

        // Se chegou at� aqui, n�o foi poss�vel pular
        return false;
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill, AdvancedPlayerMovement2D movement, PlayerAnimatorController animator)
    {
        // Valida��o para evitar divis�o por zero
        float dashDuration = (skill.dashSpeed > 0) ? skill.dashDistance / skill.dashSpeed : 0.1f;
        float originalGravity = movement.GetGravityScale();

        movement.SetGravityScale(0f);
        movement.SetVelocity(movement.GetFacingDirection().x * skill.dashSpeed, 0f);
        animator.TriggerDash();

        yield return new WaitForSeconds(dashDuration);

        movement.SetGravityScale(originalGravity);
        movement.SetVelocity(0f, movement.GetVerticalVelocity()); // Para o movimento horizontal, mas mant�m a queda
    }
}