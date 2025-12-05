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



    private PlayerSounds playerSounds;

    void Awake()
    {
        movement = GetComponent<AdvancedPlayerMovement2D>();
        playerController = GetComponent<PlayerController>();

        // Busca o componente de som
        playerSounds = GetComponent<PlayerSounds>();
        if (playerSounds == null) playerSounds = GetComponentInChildren<PlayerSounds>();
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

    // Em SkillRelease.cs

    public bool TryActivateSkill(SkillSO skill)
    {
        if (skill == null) return false;

        // 1. Checagem de Input
        if (!CheckKeyPress(skill)) return false;

        // Debug de Input
        Debug.Log($"--- TENTANDO ATIVAR: {skill.skillName} (Input Recebido) ---");

        // 2. Checagem de Cooldown
        if (skillCooldowns.ContainsKey(skill))
        {
            Debug.Log("<color=red>FALHA:</color> Skill está em cooldown.");
            return false;
        }

        // 3. Checagem de Ação em Andamento (Evita spam ou sobreposição)
        if (currentActionCoroutine != null)
        {
            Debug.Log($"<color=red>FALHA:</color> Outra ação já está em execução.");
            return false;
        }

        // 4. Checagem de Teclas de Cancelamento
        if (skill.cancelIfKeysHeld.Any(key => Input.GetKey(key)))
        {
            Debug.Log("<color=red>FALHA:</color> Uma tecla de cancelamento está sendo segurada.");
            return false;
        }

        // 5. NOVA CHECAGEM: Energia
        // Acessamos a EnergyBar através do PlayerController.
        if (playerController != null && playerController.energyBar != null)
        {
            // Se a skill tem custo e não temos energia suficiente, falha.
            if (skill.energyCost > 0 && !playerController.energyBar.HasEnoughEnergy(skill.energyCost))
            {
                Debug.Log($"<color=red>FALHA:</color> Energia insuficiente. Necessário: {skill.energyCost}");
                return false;
            }
        }

        // 6. Checagem de Condições de Estado (Chão, Ar, Parede, etc.)
        if (!CheckStateConditions(skill, true))
        {
            return false;
        }

        Debug.Log("<color=green>SUCESSO:</color> Todas as condições foram atendidas. Executando ação.");

        // 7. Execução e Consumo
        if (ExecuteAction(skill))
        {
            // Aplica Cooldown
            if (skill.cooldownDuration > 0)
            {
                skillCooldowns[skill] = skill.cooldownDuration;
            }

            // APLICA CONSUMO DE ENERGIA AQUI
            if (playerController != null && playerController.energyBar != null && skill.energyCost > 0)
            {
                playerController.energyBar.ConsumeEnergy(skill.energyCost);
            }

            return true;
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
    private bool CheckStateConditions(SkillSO skill, bool isDebugging)
    {
        foreach (var group in skill.conditionGroups)
        {
            if (isDebugging) Debug.Log($"Avaliando grupo de condições com lógica: {group.logicType}");

            if (group.logicType == ConditionLogic.AllOf)
            {
                if (!group.states.All(state =>
                {
                    bool result = movement.CheckState(state);
                    if (isDebugging) Debug.Log($"  - Checando se '{state}' é verdadeiro... Resultado: {result}");
                    return result;
                }))
                {
                    if (isDebugging) Debug.Log($"<color=red>FALHA:</color> Nem todos os estados no grupo 'AllOf' são verdadeiros.");
                    return false;
                }
            }
            else if (group.logicType == ConditionLogic.AnyOf)
            {
                if (!group.states.Any(state =>
                {
                    bool result = movement.CheckState(state);
                    if (isDebugging) Debug.Log($"  - Checando se '{state}' é verdadeiro... Resultado: {result}");
                    return result;
                }))
                {
                    if (isDebugging) Debug.Log($"<color=red>FALHA:</color> Nenhum dos estados no grupo 'AnyOf' é verdadeiro.");
                    return false;
                }
            }
        }

        foreach (var state in skill.forbiddenStates)
        {
            if (movement.CheckState(state))
            {
                if (isDebugging) Debug.Log($"<color=red>FALHA:</color> O estado proibido '{state}' está ATIVO.");
                return false;
            }
        }

        return true;
    }



    private bool ExecuteAction(SkillSO skill)
    {
        switch (skill.actionToPerform)
        {
            case MovementSkillType.SuperJump:
                if (movement.CanJumpFromGround() || currentAirJumps > 0)
                {
                    if (!movement.IsGrounded())
                    {
                        currentAirJumps--;
                    }
                    // O som de Pulo Básico já é tocado dentro do movement.DoJump, 
                    // mas se quiser um som específico de skill, poderia tocar aqui.
                    // Por enquanto deixamos o padrão.
                    movement.DoJump(skill.jumpForce);
                    return true;
                }
                return false;

            case MovementSkillType.Dash:
                // Som de Dash será tocado na corrotina
                currentActionCoroutine = StartCoroutine(ExecuteDashCoroutine(skill));
                return true;

            case MovementSkillType.WallDash:
                // Som de WallDash será tocado na corrotina
                currentActionCoroutine = StartCoroutine(ExecuteDashCoroutine(skill));
                return true;

            case MovementSkillType.DashJump:
                // TOCA SOM DE DASH JUMP
                if (playerSounds != null) playerSounds.PlayDashJumpSound();

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
                movement.DoLaunch(skill.dashJump_DashSpeed, skill.dashJump_JumpForce, launchDirection, skill.dashJump_GravityScaleOnFall, skill.dashJump_ParabolaDamping);
                return true;

            case MovementSkillType.WallJump:
                // O som de Wall Jump já está sendo chamado dentro do movement.DoWallJump,
                // que configuramos no passo anterior.
                movement.DoWallJump(skill.wallJumpForce);
                return true;

            case MovementSkillType.WallSlide:
                if (!movement.IsWallSliding())
                {
                    // O som de slide é contínuo e controlado pelo movement.StartWallSlide
                    movement.StartWallSlide(skill.wallSlideSpeed);
                    return true;
                }
                return false;

            case MovementSkillType.WallDashJump:
                // TOCA SOM DE WALL DASH JUMP
                if (playerSounds != null) playerSounds.PlayWallDashJumpSound();

                movement.DoWallLaunch(skill.wallDashJump_LaunchForceX, skill.wallDashJump_LaunchForceY, skill.wallDashJump_GravityScaleOnFall, skill.wallDashJump_ParabolaDamping);
                return true;
        }
        return false;
    }

    private IEnumerator ExecuteDashCoroutine(SkillSO skill)
    {
        bool isWallDash = skill.actionToPerform == MovementSkillType.WallDash;
        Rigidbody2D rb = movement.GetRigidbody();
        float originalGravity = rb.gravityScale;

        try
        {
            if (isWallDash)
            {
                movement.OnWallDashStart();
                // TOCA SOM DE WALL DASH
                if (playerSounds != null) playerSounds.PlayWallDashSound();
            }
            else
            {
                movement.OnDashStart();
                // TOCA SOM DE DASH COMUM
                if (playerSounds != null) playerSounds.PlayDashSound();
            }

            // ... (O resto da corrotina continua igualzinha) ...
            Vector2 direction;
            float inputX = Input.GetAxisRaw("Horizontal");
            // ... (Lógica de direção e while loop)

            // Vou replicar o trecho para você copiar e colar a função inteira se preferir:
            if (Mathf.Abs(inputX) > 0.1f && !isWallDash)
            {
                direction = new Vector2(Mathf.Sign(inputX), 0);
            }
            else
            {
                direction = movement.GetFacingDirection();
            }

            if (isWallDash)
            {
                movement.StopWallSlide();
                direction = movement.GetWallEjectDirection();
            }

            if ((direction.x > 0 && !movement.IsFacingRight()) || (direction.x < 0 && movement.IsFacingRight()))
            {
                movement.Flip();
            }

            rb.gravityScale = 0f;
            float timer = 0f;
            while (timer < skill.dashDuration)
            {
                if (movement.IsTouchingWall() && timer > 0.1f) break;
                rb.linearVelocity = new Vector2(direction.x * skill.dashSpeed, 0);
                timer += Time.deltaTime;
                yield return null;
            }
        }
        finally
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.gravityScale = originalGravity;

            if (isWallDash)
                movement.OnWallDashEnd();
            else
                movement.OnDashEnd();

            currentActionCoroutine = null;
        }
    }
}