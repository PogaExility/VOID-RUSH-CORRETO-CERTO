using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "NEXUS/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Informações Gerais")]
    public string skillName;
    public KeyCode activationKey;
    public float energyCost = 0f;
    public SkillClass skillClass;

    [Header("Efeitos")]
    public GameObject visualEffectPrefab;

    [Header("Configurações de Movimento")]
    public MovementSkillType movementSkillType;

    [Header("Configurações de Dash")]
    public DashType dashType;
    public float dashDistance = 5f;
    public float dashSpeed = 25f;

    [Header("Configurações de Pulo")]
    public float jumpHeightMultiplier = 1f;
    public int airJumps = 0;

    [Header("Configurações Específicas de Parede")]
    public Vector2 wallJumpForce;
}