// Salve como "SkillSO.cs"

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkill", menuName = "NEXUS/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Informações Gerais")]
    public string skillName;
    public float energyCost = 0f;
    public SkillClass skillClass;

    [Header("Sistema de Ativação")]
    public List<KeyCode> requiredKeys = new List<KeyCode>();
    public List<KeyCode> triggerKeys = new List<KeyCode>();

    [Header("Configuração da Lógica de Movimento")]
    public MovementSkillType movementSkillType;

    // --- SEÇÃO DE MODIFICADORES DE FÍSICA INDEPENDENTES ---

    [Header("-> Pulo (SuperJump)")]
    public float jumpForce = 12f;
    public int airJumps = 1;
    public float gravityScaleOnFall = 1.6f;
    public float coyoteTime = 0.1f;

    [Header("-> Parede")]
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    public float wallSlideSpeed = 2f;

    [Header("-> Dash (Padrão & WallDash)")]
    public DashType dashType = DashType.Normal;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;

    [Header("-> Dash com Pulo (DashJump)")]
    public float dashJump_DashSpeed = 25f;
    public float dashJump_DashDuration = 0.4f;
    public float dashJump_JumpForce = 15f;

    [Header("-> Lançamento da Parede (WallDashJump)")]
    public float wallDashJump_LaunchForceX = 25f;
    public float wallDashJump_LaunchForceY = 15f;
    public float wallDashJump_ParabolaDamping = 0.3f;
}