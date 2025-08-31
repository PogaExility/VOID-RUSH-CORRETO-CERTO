using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(AdvancedPlayerMovement2D))]
public class SkillRelease : MonoBehaviour
{
    private int currentAirJumps;
    private Coroutine currentActionCoroutine;
    private AdvancedPlayerMovement2D movement;
    private PlayerController playerController;
    private float dashInputBufferTimer = 0f;
    private Dictionary<SkillSO, float> skillCooldowns = new Dictionary<SkillSO, float>();
    public void SetDashBuffer(float bufferTime)
    {
        dashInputBufferTimer = bufferTime;
    }



    void Awake()
    {
        movement = GetComponent<AdvancedPlayerMovement2D>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // 1. Atualiza o timer do buffer de input
        if (dashInputBufferTimer > 0)
        {
            dashInputBufferTimer -= Time.deltaTime;
        }

        // 2. ATUALIZA TODOS OS COOLDOWNS ATIVOS
        // Se não houver skills em cooldown, esta parte é pulada (super eficiente).
        if (skillCooldowns.Count > 0)
        {
            // Cria uma lista das skills em cooldown para poder modificar o dicionário com segurança.
            List<SkillSO> skillsOnCooldown = new List<SkillSO>(skillCooldowns.Keys);

            foreach (SkillSO skill in skillsOnCooldown)
            {
                // Diminui o tempo do cooldown
                skillCooldowns[skill] -= Time.deltaTime;
                // Se o tempo acabou, remove a skill do dicionário.
                if (skillCooldowns[skill] <= 0)
                {
                    skillCooldowns.Remove(skill);
                }
            }
        }

        // 3. Lógica original de resetar pulos aéreos
        SkillSO activeJumpSkill = playerController.GetActiveJumpSkill();
        if (activeJumpSkill != null && movement.IsGrounded())
        {
            currentAirJumps = activeJumpSkill.airJumps;
        }
    }

    public bool TryActivateSkill(SkillSO skill)
    {
        if (skill == null) return false;

        // --- A NOVA CHECAGEM DE COOLDOWN ---
        // Se a skill que estamos tentando usar está no dicionário, significa que ela está em cooldown.
        if (skillCooldowns.ContainsKey(skill))
        {
            return false; // Falha imediatamente.
        }

        if (currentActionCoroutine != null) return false;
        if (skill.cancelIfKeysHeld.Any(key => Input.GetKey(key))) return false;
        if (!CheckKeyPress(skill) || !CheckStateConditions(skill)) return false;

        // Se a skill foi executada com sucesso...
        if (ExecuteAction(skill))
        {
            // ...e se ela TEM um cooldown...
            if (skill.cooldownDuration > 0)
            {
                // ...adiciona ela ao dicionário com sua duração total.
                skillCooldowns[skill] = skill.cooldownDuration;
            }
            return true; // Retorna sucesso.
        }

        return false;
    }


    private bool CheckKeyPress(SkillSO skill)
    {
        // LÓGICA PARA SKILLS COMBINADAS (DashJump, WallDashJump)
        if (skill.actionToPerform == MovementSkillType.DashJump || skill.actionToPerform == MovementSkillType.WallDashJump)
        {
            bool triggerPressed = skill.triggerKeys.Any(key => Input.GetKeyDown(key)); // Pulo foi pressionado?

            // CONDIÇÃO DE SUCESSO:
            // O jogador está segurando a tecla requerida (Q)? OU o buffer de dash está ativo?
            bool requiredHeld = skill.requiredKeys.All(key => Input.GetKey(key));
            bool dashBufferActive = dashInputBufferTimer > 0;

            if (triggerPressed && (requiredHeld || dashBufferActive))
            {
                dashInputBufferTimer = 0f; // Consome o buffer
                return true;
            }
            return false;
        }

        // LÓGICA PADRÃO PARA TODAS AS OUTRAS SKILLS (Dash, Pulo Normal, etc.)
        // Esta lógica restaura o funcionamento correto que eu apaguei.
        if (skill.triggerKeys.Count > 0)
        {
            bool triggerPressed = skill.triggerKeys.Any(key => Input.GetKeyDown(key));
            bool requiredHeld = skill.requiredKeys.Count == 0 || skill.requiredKeys.All(key => Input.GetKey(key));
            return triggerPressed && requiredHeld;
        }

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