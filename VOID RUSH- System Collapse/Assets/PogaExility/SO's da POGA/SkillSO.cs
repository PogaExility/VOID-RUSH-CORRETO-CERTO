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

    // --- Configurações de Dash ---
    // <<< MUDANÇA: 'Can Dash In Air' e 'ignoresGravity' foram substituídos pelo enum >>>
    public DashType dashType;
    public float dashDistance = 5f;
    public float dashSpeed = 25f;

    // --- Configurações de Pulo ---
    public float jumpHeightMultiplier = 1f;
    public int airJumps = 0;
}