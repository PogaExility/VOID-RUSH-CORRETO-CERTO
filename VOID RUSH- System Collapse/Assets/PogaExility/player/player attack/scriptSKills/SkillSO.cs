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

    // Dash
    public float dashDistance = 5f;
    public float dashSpeed = 25f;
    public bool canDashInAir = false;
    [Tooltip("Se marcado, o dash será uma linha reta que ignora a gravidade. Senão, será uma parábola.")]
    public bool ignoresGravity = false; // <-- CAMPO ADICIONADO

    // Pulo
    public float jumpHeightMultiplier = 1f;
    public int airJumps = 0;
}