using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(AdvancedPlayerMovement2D))]
public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;
    private Coroutine currentActionCoroutine;
    private AdvancedPlayerMovement2D movement;
    private PlayerController playerController;

    void Awake()
    {
        movement = GetComponent<AdvancedPlayerMovement2D>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        SkillSO activeJumpSkill = playerController.GetActiveJumpSkill();
        if (activeJumpSkill != null && movement.IsGrounded())
        {
            currentAirJumps = activeJumpSkill.airJumps;
        }
    }

    // Dentro do SkillRelease.cs

    public bool TryActivateSkill(SkillSO skill)
    {
        // Se a skill não existe, ou as teclas não foram apertadas,
        // ou as condições de estado não foram atendidas, ou outra skill está ativa, falha.
        if (skill == null || !CheckKeyPress(skill) || !CheckStateConditions(skill) || currentActionCoroutine != null)
        {
            return false;
        }

        // Se passou por tudo, executa a ação.
        return ExecuteAction(skill);
    }

    private bool CheckKeyPress(SkillSO skill)
    {
        bool triggerPressed = skill.triggerKeys.Any(key => Input.GetKeyDown(key));
        if (!triggerPressed) return false;
        return skill.requiredKeys.All(key => Input.GetKey(key));
    }

    // Dentro do seu SkillRelease.cs

    // Adicione esta função inteira ao seu SkillRelease.cs

    private bool HandleSuperJump(SkillSO skill)
    {
        // --- AQUI ESTÁ A CORREÇÃO PRINCIPAL ---
        // Adicionamos uma verificação no início: se o jogador está tocando a parede,
        // esta função é abortada, dando prioridade às skills de parede (como o Wall Slide).
        if (movement.IsTouchingWall())
        {
            return false;
        }

        // A sua lógica original de pulo, que já estava correta.
        if (movement.CanJumpFromGround() || currentAirJumps > 0)
        {
            // Se não estava no chão, gasta um pulo aéreo.
            if (!movement.IsGrounded())
            {
                currentAirJumps--;
            }
            // Chama a função de física para executar o pulo.
            movement.DoJump(skill.jumpForce);
            return true;
        }

        // Se nenhuma das condições acima for atendida, o pulo falha.
        return false;
    }


    private bool CheckStateConditions(SkillSO skill)
    {
        foreach (var group in skill.conditionGroups)
        {
            if (group.logicType == ConditionLogic.AllOf)
            {
                if (!group.states.All(state => movement.CheckState(state))) return false;
            }
            else if (group.logicType == ConditionLogic.AnyOf)
            {
                if (!group.states.Any(state => movement.CheckState(state))) return false;
            }
        }
        foreach (var state in skill.forbiddenStates)
        {
            if (movement.CheckState(state)) return false;
        }
        return true;
    }

    // Dentro do SkillRelease.cs

    // Dentro do seu SkillRelease.cs

    private bool ExecuteAction(SkillSO skill)
    {
        switch (skill.actionToPerform)
        {
            case MovementSkillType.SuperJump:
                // --- AQUI ESTÁ A CORREÇÃO PRINCIPAL ---
                // Adicionamos uma condição: se o jogador está tocando a parede,
                // o pulo normal/aéreo é PROIBIDO de ser ativado.
                if (movement.IsTouchingWall())
                {
                    return false; // Falha e permite que o PlayerController teste a próxima skill (Wall Slide).
                }

                // Se não está tocando a parede, a lógica de pulo normal funciona.
                if (movement.CanJumpFromGround() || currentAirJumps > 0)
                {
                    if (!movement.IsGrounded())
                    {
                        currentAirJumps--;
                    }
                    movement.DoJump(skill.jumpForce);
                    return true;
                }
                break; // Se não puder pular, falha.

            // --- O RESTO DAS SUAS AÇÕES, INTACTAS ---
            case MovementSkillType.Dash:
            case MovementSkillType.WallDash:
                currentActionCoroutine = StartCoroutine(ExecuteDashCoroutine(skill.dashSpeed, skill.dashDuration));
                return true;

            case MovementSkillType.DashJump:
                currentActionCoroutine = StartCoroutine(ExecuteDashJumpCoroutine(skill));
                return true;

            case MovementSkillType.WallJump:
                movement.DoWallJump(skill.wallJumpForce);
                return true;

            case MovementSkillType.WallSlide:
                if (!movement.IsWallSliding())
                {
                    movement.StartWallSlide(skill.wallSlideSpeed);
                    return true;
                }
                return false;


            case MovementSkillType.WallDashJump:
                movement.DoWallLaunch(skill.wallDashJump_LaunchForceX, skill.wallDashJump_LaunchForceY, skill.wallDashJump_ParabolaDamping);
                currentActionCoroutine = StartCoroutine(WaitForParabolaEnd());
                return true;
        }
        return false;
    }
    // --- Corotinas que Executam as Ações Físicas ---

    private IEnumerator ExecuteDashCoroutine(float speed, float duration)
    {
        movement.OnDashStart();
        movement.SetGravityScale(0f);
        Vector2 direction = movement.GetFacingDirection();
        float timer = 0f;
        while (timer < duration)
        {
            if (movement.IsTouchingWall()) break;
            movement.SetVelocity(direction.x * speed, 0);
            timer += Time.deltaTime;
            yield return null;
        }
        movement.SetVelocity(0, 0);
        movement.SetGravityScale(movement.baseGravity);
        movement.OnDashEnd();
        currentActionCoroutine = null;
    }

    private IEnumerator ExecuteDashJumpCoroutine(SkillSO skill)
    {
        movement.OnDashStart();
        movement.SetGravityScale(0f);
        Vector2 direction = movement.GetFacingDirection();
        float timer = 0f;
        bool jumped = false;

        while (timer < skill.dashJump_DashDuration)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                movement.DoLaunch(direction.x * movement.moveSpeed, skill.dashJump_JumpForce, 0.1f);
                jumped = true;
                break;
            }
            movement.SetVelocity(direction.x * skill.dashJump_DashSpeed, 0);
            timer += Time.deltaTime;
            yield return null;
        }
        if (!jumped) movement.SetVelocity(0, 0);

        movement.SetGravityScale(movement.baseGravity);
        movement.OnDashEnd();
        currentActionCoroutine = null;
    }
    private IEnumerator WaitForParabolaEnd()
    {
        // Espera até que a propriedade IsInParabolaArc no script de movimento se torne falsa.
        yield return new WaitUntil(() => !movement.IsInParabolaArc());

        // Chama a função OnParabolaEnd para restaurar o estado normal (ex: linear damping).
        movement.OnParabolaEnd();

        // Libera o sistema para a próxima skill.
        currentActionCoroutine = null;
    }
}