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

    private IEnumerator ExecuteWallDashCoroutine(float speed, float duration)
    {
        movement.OnDashStart();
        movement.StopWallSlide(); // Garante que o estado de slide pare
        movement.SetGravityScale(0f);

        // --- A LÓGICA CORRETA ---
        Vector2 direction = movement.GetWallEjectDirection(); // Pega a direção para LONGE da parede

        float timer = 0f;
        while (timer < duration)
        {
            movement.SetVelocity(direction.x * speed, 0);
            timer += Time.deltaTime;
            yield return null;
        }
        movement.SetVelocity(0, 0);
        movement.SetGravityScale(movement.baseGravity);
        movement.OnDashEnd();
        currentActionCoroutine = null;
    }

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
    public bool TryActivateSkill(SkillSO skill)
    {
        if (skill == null) return false;

        // Log apenas para a skill que nos interessa
        if (skill.skillName == "wallSliding")
        {
            Debug.Log($"<color=yellow>INTERROGATÓRIO:</color> Verificando a skill '{skill.skillName}'.");

            bool keyCheck = CheckKeyPress(skill);
            if (!keyCheck)
            {
                Debug.Log($"<color=red>FALHA NO INTERROGATÓRIO:</color> A checagem de TECLAS para '{skill.skillName}' falhou.");
                return false;
            }

            bool stateCheck = CheckStateConditions(skill);
            if (!stateCheck)
            {
                // VAMOS DESCOBRIR QUAL ESTADO ESPECÍFICO FALHOU
                Debug.Log($"<color=red>FALHA NO INTERROGATÓRIO:</color> A checagem de ESTADOS para '{skill.skillName}' falhou. Detalhes:");
                foreach (var group in skill.conditionGroups)
                {
                    foreach (var state in group.states)
                    {
                        Debug.Log($"- Condição '{state}' está: {movement.CheckState(state)}");
                    }
                }
                return false;
            }

            if (currentActionCoroutine != null)
            {
                Debug.Log($"<color=red>FALHA NO INTERROGATÓRIO:</color> Outra skill já está ativa.");
                return false;
            }

            Debug.Log($"<color=green>SUCESSO NO INTERROGATÓRIO:</color> '{skill.skillName}' passou em todos os testes. Executando.");
            return ExecuteAction(skill);
        }

        // Para as outras skills, roda a lógica normal sem poluir o console
        if (!CheckKeyPress(skill) || !CheckStateConditions(skill) || currentActionCoroutine != null)
        {
            return false;
        }
        return ExecuteAction(skill);
    }

    // Em SkillRelease.cs
    // Em SkillRelease.cs
    // Em SkillRelease.cs
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
                // Chama a corotina de Dash correta e funcional.
                currentActionCoroutine = StartCoroutine(ExecuteDashCoroutine(skill.dashSpeed, skill.dashDuration));
                return true;


            case MovementSkillType.WallDash:
                // Também chama a corotina de WallDash, que lida com a ejeção.
                currentActionCoroutine = StartCoroutine(ExecuteWallDashCoroutine(skill.dashSpeed, skill.dashDuration));
                return true;

            // Em SkillRelease.cs -> ExecuteAction
            case MovementSkillType.DashJump:
                // 1. LÊ O INPUT DO JOGADOR AGORA
                float horizontalInput = Input.GetAxisRaw("Horizontal");
                Vector2 launchDirection;

                // 2. SE O JOGADOR ESTIVER PRESSIONANDO UMA DIREÇÃO, USA ELA.
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    launchDirection = new Vector2(Mathf.Sign(horizontalInput), 0);
                }
                // 3. SE NÃO, USA A DIREÇÃO QUE O PERSONAGEM JÁ TINHA (NEUTRO).
                else
                {
                    launchDirection = movement.GetFacingDirection();
                }

                // 4. CHAMA O NOVO DoLaunch COM A DIREÇÃO CORRETA
                movement.DoLaunch(skill.dashJump_DashSpeed, skill.dashJump_JumpForce, skill.wallDashJump_ParabolaDamping, launchDirection);
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
                return true; // SÓ ISSO

        }
        return false;
    }
    // --- Corotinas que Executam as Ações Físicas ---

    // Corotina para Dash Normal / Aéreo
    // Substitua a corotina ExecuteDashCoroutine em SkillRelease.cs por esta
    // Em SkillRelease.cs
    // Em SkillRelease.cs
    private IEnumerator ExecuteDashCoroutine(float speed, float duration)
    {
        movement.OnDashStart();

        // A DIREÇÃO JÁ ESTÁ CORRETA. SEMPRE.
        // A "race condition" foi eliminada na arquitetura.
        Vector2 dashDirection = movement.GetFacingDirection();

        float originalGravity = movement.GetRigidbody().gravityScale;
        movement.SetGravityScale(0f);

        float timer = 0f;
        while (timer < duration)
        {
            if (movement.IsTouchingWall()) break;

            // Preserva o arco do pulo
            float currentYVelocity = movement.GetRigidbody().linearVelocity.y;
            movement.SetVelocity(dashDirection.x * speed, currentYVelocity);

            timer += Time.deltaTime;
            yield return null;
        }

        movement.SetVelocity(0, movement.GetRigidbody().linearVelocity.y);
        movement.SetGravityScale(originalGravity);
        movement.OnDashEnd();
        currentActionCoroutine = null;
    }

    // Garanta que esta corotina exista em SkillRelease.cs
    private IEnumerator WaitForParabolaEnd()
    {
        // Espera até a parábola terminar (tocando o chão) ou ser interrompida (tocando uma parede)
        yield return new WaitUntil(() => !movement.IsInParabolaArc() || movement.IsTouchingWall());

        if (movement.IsTouchingWall())
        {
            movement.SetVelocity(0, 0); // Zera a velocidade se bateu na parede
        }

        movement.OnParabolaEnd(); // Avisa o "corpo" para limpar o estado de parábola
        currentActionCoroutine = null; // Libera o sistema para a próxima skill
    }
    // Adicione esta corotina inteira ao SkillRelease.cs
    private IEnumerator ExecuteDashJumpExplosionCoroutine(SkillSO skill)
    {
        // 1. TOMA O CONTROLE
        movement.DisablePhysicsControl();
        movement.OnDashStart(); // Reutiliza o estado de dash

        // 2. DETERMINA A DIREÇÃO
        Vector2 launchDirection = movement.GetFacingDirection();

        // 3. EXECUTA A EXPLOSÃO
        float originalGravity = movement.GetRigidbody().gravityScale;
        movement.SetGravityScale(0f);
        movement.SetVelocity(launchDirection.x * skill.dashJump_DashSpeed, skill.dashJump_JumpForce);

        // Duração do "salto" antes que a gravidade volte. Pequeno, só para dar o impulso.
        yield return new WaitForSeconds(0.2f);

        // 4. DEVOLVE O CONTROLE
        movement.SetGravityScale(originalGravity);
        movement.OnDashEnd();
        movement.EnablePhysicsControl();
        currentActionCoroutine = null;
    }
}