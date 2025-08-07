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
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashCount"), new GUIContent("Quantidade de Dash"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDistance"), new GUIContent("Distância do Dash"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"), new GUIContent("Velocidade do Dash"));
                        break;

                    case MovementSkillType.SuperJump:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHeightMultiplier"), new GUIContent("Multiplicador de Altura"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("airJumps"), new GUIContent("Pulos Aéreos Extras")); // <-- A LINHA QUE FALTAVA
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