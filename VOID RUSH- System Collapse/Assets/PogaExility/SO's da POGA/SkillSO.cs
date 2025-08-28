using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkill", menuName = "NEXUS/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Informações Gerais")]
    public string skillName;
    public float energyCost = 0f;
    public SkillClass skillClass;

    [Tooltip("Se marcado, esta skill pode interromper outras skills em andamento (como um dash). Use com cuidado.")]
    public bool canInterrupt = false; // A VARIÁVEL QUE FALTAVA

    [Header("Sistema de Ativação")]
    [Tooltip("Todas as teclas que DEVEM estar sendo seguradas para que a skill possa ser ativada.")]
    public List<KeyCode> requiredKeys = new List<KeyCode>();
    [Tooltip("QUALQUER uma destas teclas, quando pressionada, irá disparar a ação.")]
    public List<KeyCode> triggerKeys = new List<KeyCode>();

    [Header("Configuração da Lógica de Movimento")]
    [Tooltip("Define qual lógica principal de movimento esta skill vai usar. Isso determinará quais campos abaixo são relevantes.")]
    public MovementSkillType movementSkillType;

    // --- SEÇÃO DE MODIFICADORES DE FÍSICA ---

    [Header("-> Modificadores de Pulo (SuperJump)")]
    [Tooltip("A força inicial aplicada no pulo.")]
    public float jumpForce = 12f;
    [Tooltip("O número de pulos extras permitidos no ar.")]
    public int airJumps = 1;
    [Tooltip("A gravidade extra aplicada quando o jogador está caindo (multiplicador).")]
    public float gravityScaleOnFall = 1.6f;
    [Tooltip("O pequeno tempo em segundos que o jogador ainda pode pular após sair de uma plataforma.")]
    public float coyoteTime = 0.1f;

    [Header("-> Modificadores de Pulo de Parede (WallJump)")]
    [Tooltip("A força do pulo para longe da parede (X=horizontal, Y=vertical).")]
    public Vector2 wallJumpForce = new Vector2(10f, 12f);

    [Header("-> Modificadores de Deslize na Parede (WallSlide)")]
    [Tooltip("A velocidade com que o jogador desliza pela parede.")]
    public float wallSlideSpeed = 2f;

    [Header("-> Modificadores de Dash (Dash & WallDash)")]
    [Tooltip("O tipo de dash (Normal = só no chão, Aereo = pode ser usado no ar).")]
    public DashType dashType = DashType.Normal;
    [Tooltip("A velocidade do jogador durante o dash.")]
    public float dashSpeed = 30f;
    [Tooltip("A duração do dash em segundos.")]
    public float dashDuration = 0.2f;

    [Header("-> Modificadores de Lançamento (WallDashJump)")]
    [Tooltip("A força horizontal do lançamento inicial.")]
    public float launchForceX = 25f;
    [Tooltip("A força vertical do lançamento inicial.")]
    public float launchForceY = 15f;
    [Tooltip("O atrito linear (arrasto) aplicado durante a parábola para uma perda de velocidade gradual.")]
    public float parabolaLinearDamping = 0.3f;
}