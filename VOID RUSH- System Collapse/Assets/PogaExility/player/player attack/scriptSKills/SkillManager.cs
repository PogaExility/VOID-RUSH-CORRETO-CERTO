using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("Lista de Skills disponíveis")]
    public List<SkillSO> availableSkills = new List<SkillSO>();

    [Header("Limite de Skills que o personagem pode usar")]
    public int maxActiveSkills = 3;

    private List<SkillSO> activeSkills = new List<SkillSO>();

    public void AddSkill(SkillSO skill)
    {
        if (activeSkills.Count >= maxActiveSkills)
        {
            Debug.LogWarning("Limite de skills ativas atingido.");
            return;
        }
        if (!activeSkills.Contains(skill))
        {
            activeSkills.Add(skill);
            Debug.Log("Skill adicionada: " + skill.skillName);
        }
    }

    public void RemoveSkill(SkillSO skill)
    {
        if (activeSkills.Contains(skill))
        {
            activeSkills.Remove(skill);
            Debug.Log("Skill removida: " + skill.skillName);
        }
    }

    public bool HasSkill(SkillSO skill)
    {
        return activeSkills.Contains(skill);
    }

    public List<SkillSO> GetActiveSkills()
    {
        return new List<SkillSO>(activeSkills);
    }

    // Futuramente, métodos para ativar/desativar skills, cooldowns, etc.
}
