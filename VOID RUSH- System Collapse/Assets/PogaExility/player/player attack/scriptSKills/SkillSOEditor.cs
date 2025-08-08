using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkillSO))]
public class SkillSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SkillSO skill = (SkillSO)target;

        EditorGUILayout.LabelField("Informações Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activationKey"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("visualEffectPrefab"));

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Configuração da Skill", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillClass"));

        switch (skill.skillClass)
        {
            case SkillClass.Movimento:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSkillType"));

                switch (skill.movementSkillType)
                {
                    case MovementSkillType.Dash:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDistance"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canDashInAir"));
                        break;

                    case MovementSkillType.SuperJump:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHeightMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("airJumps"));
                        break;
                }
                break;

            case SkillClass.Buff:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("buffDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("buffAmount"));
                break;

            case SkillClass.Dano:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damageAmount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRange"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}