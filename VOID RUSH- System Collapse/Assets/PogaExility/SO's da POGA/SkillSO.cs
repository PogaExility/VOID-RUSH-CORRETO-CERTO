using UnityEngine;

public enum SkillClass
{
    Movement,
    Damage,
    Buff,
    // Adicione outras classes conforme necessário
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "NEXUS/Skill")]
public class SkillSO : ScriptableObject
{
    public string skillName;
    public SkillClass skillClass;

    [Header("Parâmetros de Movimento (se aplicável)")]
    public float dashDuration;
    public float dashDistance;
    public float dashSpeed;

    [Header("Prefab do efeito visual da skill")]
    public GameObject visualEffectPrefab;

    // Adicione outros parâmetros específicos para outras classes de skill conforme necessário
}
