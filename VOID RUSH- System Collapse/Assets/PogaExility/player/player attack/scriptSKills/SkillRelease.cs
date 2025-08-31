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

    // Em SkillRelease.cs
    // Substitua sua função TryActivateSkill por esta:
    public bool TryActivateSkill(SkillSO skill)
    {
        if (skill == null) return false;

        // CORREÇÃO CRÍTICA: Checa o cancelamento ANTES de tudo e para TODAS as skills.
        if (skill.cancelIfKeysHeld.Any(key => Input.GetKey(key)))
        {
            return false;
        }

        // Se outra ação já está rodando, falha.
        if (currentActionCoroutine != null) return false;

        // Checagens normais de tecla e estado.
        if (!CheckKeyPress(skill) || !CheckStateConditions(skill))
        {
            return false;
        }

        return ExecuteAction(skill);
    }

 
    private bool CheckKeyPress(SkillSO skill)
    {
        // Lógica para skills com GATILHO e REQUERIDAS (DashJump, WallDashJump)
        if (skill.triggerKeys.Count > 0 && skill.requiredKeys.Count > 0)
        {
            // O gatilho foi pressionado E a requerida está sendo segurada?
            bool triggerPressed = skill.triggerKeys.Any(key => Input.GetKeyDown(key));
            bool requiredHeld = skill.requiredKeys.All(key => Input.GetKey(key));

            return triggerPressed && requiredHeld;
        }

        // Lógica para skills SÓ com GATILHO (Pulo, Dash normal)
        if (skill.triggerKeys.Count > 0)
        {
            return skill.triggerKeys.Any(key => Input.GetKeyDown(key));
        }

        // Se a skill não tem gatilho, a checagem de tecla não se aplica
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


    private bool ExecuteAction(SkillSO skill)
    {
        switch (skill.actionToPerform)
        {
            case MovementSkillType.SuperJump:
                // --- VERSÃO SIMPLIFICADA E CORRETA ---
                // A lógica de "pode pular?" é a única coisa que importa aqui.
                // O PlayerController já garantiu que não estamos na parede.
                if (movement.CanJumpFromGround() || currentAirJumps > 0)
                {
                    if (!movement.IsGrounded())
                    {
                        currentAirJumps--;
                    }
                    movement.DoJump(skill.jumpForce);
                    return true;
                }
                return false; // Falha se não puder pular

            // --- O RESTO DAS SUAS AÇÕES, INTACTAS ---

            case MovementSkillType.Dash:
                // CORREÇÃO: Envie o 'skill' inteiro, não seus parâmetros separados.
                currentActionCoroutine = StartCoroutine(ExecuteDashCoroutine(skill));
                return true;



            case MovementSkillType.WallDash: // <-- FAÇA O WALLDASH "CAIR" PARA O DASH
                                             // Ambos agora usam a MESMA corotina, que já está corrigida.
                currentActionCoroutine = StartCoroutine(ExecuteDashCoroutine(skill));
                return true;

            // Em SkillRelease.cs -> ExecuteAction
            case MovementSkillType.DashJump:
                float horizontalInput = Input.GetAxisRaw("Horizontal");
                Vector2 launchDirection;
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    launchDirection = new Vector2(Mathf.Sign(horizontalInput), 0);
                }
                else
                {
                    launchDirection = movement.GetFacingDirection();
                }
                // 4. CHAMA O NOVO DoLaunch COM A DIREÇÃO CORRETA
                movement.DoLaunch(skill.dashJump_DashSpeed, skill.dashJump_JumpForce, launchDirection, skill.dashJump_GravityScaleOnFall, skill.dashJump_ParabolaDamping);
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
                movement.DoWallLaunch(skill.wallDashJump_LaunchForceX, skill.wallDashJump_LaunchForceY, skill.wallDashJump_GravityScaleOnFall, skill.wallDashJump_ParabolaDamping);
                return true; // SÓ ISSO

        }
        return false;
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill)
    {
        movement.OnDashStart();

        Vector2 direction;
        float inputX = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(inputX) > 0.1f)
        {
            direction = new Vector2(Mathf.Sign(inputX), 0);
        }
        else
        {
            direction = movement.GetFacingDirection();
        }
        if (skill.actionToPerform == MovementSkillType.WallDash)
        {
            // --- A ORDEM QUE FALTAVA ---
            movement.StopWallSlide();

            direction = movement.GetWallEjectDirection();
        }

        // --- A CORREÇÃO DO ERRO DE DIGITAÇÃO ---
        // A segunda condição agora é 'movement.IsFacingRight()', como deveria ser.
        if ((direction.x > 0 && !movement.IsFacingRight()) || (direction.x < 0 && movement.IsFacingRight()))
        {
            movement.Flip();
        }


        float originalGravity = movement.GetRigidbody().gravityScale;
        movement.SetGravityScale(0f);
        float timer = 0f;
        while (timer < skill.dashDuration)
        {
            if (movement.IsTouchingWall() && timer > 0.1f) break;
            movement.SetVelocity(direction.x * skill.dashSpeed, movement.GetRigidbody().linearVelocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        movement.SetVelocity(0, movement.GetRigidbody().linearVelocity.y);
        movement.SetGravityScale(originalGravity);
        movement.OnDashEnd();
        currentActionCoroutine = null;
    }
}