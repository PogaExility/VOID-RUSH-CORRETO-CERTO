using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "NEXUS/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Informações Gerais")]
    public string skillName;
    public KeyCode activationKey;
    [Tooltip("Custo de energia para usar a skill. 0 para não custar nada.")]
    public float energyCost = 0f;
    public SkillClass skillClass;

    [Header("Efeitos")]
    public GameObject visualEffectPrefab;

    // --- PARÂMETROS DE MOVIMENTO ---
    [Header("Configurações de Movimento")]
    public MovementSkillType movementSkillType;

    // Dash
    [Tooltip("A distância que o dash percorre.")]
    public float dashDistance = 5f;
    [Tooltip("A velocidade do dash.")]
    public float dashSpeed = 25f;
    [Tooltip("Se marcado, o jogador pode usar este dash no ar.")]
    public bool canDashInAir = false;

    // Pulo
    [Tooltip("Multiplicador da altura do pulo. 1 = normal.")]
    public float jumpHeightMultiplier = 1f;
    [Tooltip("Quantos pulos extras o jogador pode dar no ar.")]
    public int airJumps = 0;

    // --- PARÂMETROS DE BUFF (Ainda não usados, mas declarados) ---
    [Header("Configurações de Buff")]
    public float buffDuration;
    public float buffAmount;

    // --- PARÂMETROS DE DANO (Ainda não usados, mas declarados) ---
    [Header("Configurações de Dano")]
    public float damageAmount;
    public float attackRange;
}