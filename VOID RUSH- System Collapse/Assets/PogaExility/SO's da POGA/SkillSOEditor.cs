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
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashType"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDistance"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                        break;

                    // ===== INÍCIO DA CORREÇÃO =====
                    case MovementSkillType.WallDash:
                        // Mostra os campos de Velocidade e agora DISTÂNCIA
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashDistance"));
                        break;
                    // ===== FIM DA CORREÇÃO =====

                    case MovementSkillType.SuperJump:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHeightMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("airJumps"));
                        break;
                }
                break;

            case SkillClass.Buff:
                EditorGUILayout.HelpBox("Configurações para skills de Buff ainda não implementadas.", MessageType.Info);
                break;

            case SkillClass.Dano:
                EditorGUILayout.HelpBox("Configurações para skills de Dano ainda não implementadas.", MessageType.Info);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}