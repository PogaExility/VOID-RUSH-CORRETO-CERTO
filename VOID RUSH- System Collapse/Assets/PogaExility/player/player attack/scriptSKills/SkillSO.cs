using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "NEXUS/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Informações Gerais")]
    public string skillName;
    public KeyCode activationKey;
    public float energyCost;
    public SkillClass skillClass;

    [Header("Efeitos")]
    public GameObject visualEffectPrefab;

    // --- PARÂMETROS DE MOVIMENTO ---
    [Header("Configurações de Movimento")]
    public MovementSkillType movementSkillType;
    // Parâmetros do Dash
    public int dashCount;
    public float dashDistance;
    public float dashSpeed;
    // Parâmetros do SuperJump
    public float jumpHeightMultiplier;
    public int airJumps; // <-- A LINHA QUE FALTAVA

    // --- PARÂMETROS DE BUFF ---
    [Header("Configurações de Buff")]
    public float buffDuration;
    public float buffAmount;

    // --- PARÂMETROS DE DANO ---
    [Header("Configurações de Dano")]
    public float damageAmount;
    public float attackRange;
}