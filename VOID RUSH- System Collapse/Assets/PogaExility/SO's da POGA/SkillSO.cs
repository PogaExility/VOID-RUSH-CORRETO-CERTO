using UnityEngine;
using System.Collections.Generic; // Essencial para usar List<T>
using System; // Essencial para a tag [Serializable]

/// <summary>
/// Uma estrutura que define um grupo de condições de estado.
/// Permite criar lógicas "E" (Todas as condições precisam ser verdadeiras)
/// e "OU" (Pelo menos uma condição precisa ser verdadeira).
/// </summary>
[Serializable]
public class ConditionGroup
{
    [Tooltip("Define como as condições nesta lista devem ser avaliadas.")]
    public ConditionLogic logicType = ConditionLogic.AllOf;

    [Tooltip("A lista de estados que serão avaliados de acordo com o Logic Type.")]
    public List<PlayerState> states = new List<PlayerState>();
}

/// <summary>
/// O SkillSO é a "ficha técnica" de uma habilidade. Ele não contém lógica,
/// apenas os dados (parâmetros de física, teclas de ativação, condições)
/// que o SkillRelease vai ler para executar a ação.
/// </summary>
[CreateAssetMenu(fileName = "NewSkill", menuName = "NEXUS/Skill (Avançado)")]
public class SkillSO : ScriptableObject
{
    [Header("Informações Gerais")]
    [Tooltip("Nome da habilidade para referência no editor.")]
    public string skillName;
    [Tooltip("Custo de energia para usar a habilidade.")]
    public float energyCost = 0f;
    [Tooltip("A categoria geral da habilidade (usado para organizar o editor).")]
    public SkillClass skillClass;

    [Header("Sistema de Ativação")]
    [Tooltip("Todas as teclas que DEVEM estar sendo seguradas para que a skill possa ser ativada.")]
    public List<KeyCode> requiredKeys = new List<KeyCode>();
    [Tooltip("QUALQUER uma destas teclas, quando pressionada, irá disparar a ação.")]
    public List<KeyCode> triggerKeys = new List<KeyCode>();

    // --- ADICIONE ESTA NOVA LISTA AQUI ---
    [Tooltip("Se QUALQUER uma destas teclas estiver sendo segurada, a ativação desta skill falha. Útil para evitar que Pulo e DashJump ativem juntos.")]
    public List<KeyCode> cancelIfKeysHeld = new List<KeyCode>();

    [Header("Lógica da Ação")]
    [Tooltip("A ação principal que esta skill executa. Determina quais parâmetros abaixo são usados.")]
    public MovementSkillType actionToPerform;

    // --- O SISTEMA DE CONDIÇÕES AVANÇADO ---
    [Header("Condições de Ativação")]
    [Tooltip("O jogador deve satisfazer TODOS os grupos de condições nesta lista para ativar a skill.")]
    public List<ConditionGroup> conditionGroups = new List<ConditionGroup>();
    [Tooltip("O jogador NÃO PODE estar em NENHUM destes estados para ativar a skill.")]
    public List<PlayerState> forbiddenStates = new List<PlayerState>();

    // --- SEÇÃO DE MODIFICADORES DE FÍSICA INDEPENDENTES ---

    [Header("-> Parâmetros de Pulo (SuperJump)")]
    public float jumpForce = 12f;
    public int airJumps = 1;
    public float gravityScaleOnFall = 1.6f;
    public float coyoteTime = 0.1f;

    [Header("-> Parâmetros de Parede (WallJump & WallSlide)")]
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    public float wallSlideSpeed = 2f;

    [Header("-> Parâmetros de Dash (Padrão & WallDash)")]
    public DashType dashType = DashType.Normal;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;

    // Em SkillSO.cs

    [Header("-> Parâmetros de Dash com Pulo (DashJump)")]
    public float dashJump_DashSpeed = 25f;
    public float dashJump_DashDuration = 0.4f;
    public float dashJump_JumpForce = 15f;
    [Tooltip("Janela de tempo para registrar o input combinado. 0.15 é um bom valor.")]
    public float dashJump_InputBuffer = 0.15f;
    [Tooltip("Atrito do ar via Linear Damping. 0 = sem atrito. 1 = atrito suave. 5 = atrito forte.")]
    public float dashJump_ParabolaDamping = 1f;
    public float dashJump_GravityScaleOnFall = 2.5f;

    [Header("-> Parâmetros de Lançamento da Parede (WallDashJump)")]
    public float wallDashJump_LaunchForceX = 25f;
    public float wallDashJump_LaunchForceY = 15f;
    [Tooltip("Janela de tempo para registrar o input combinado. 0.15 é um bom valor.")]
    public float wallDashJump_InputBuffer = 0.15f;
    [Tooltip("Atrito do ar via Linear Damping. 0 = sem atrito. 1 = atrito suave. 5 = atrito forte.")]
    public float wallDashJump_ParabolaDamping = 1f;
    public float wallDashJump_GravityScaleOnFall = 2.5f;
}