// Salve como "SkillSOEditor.cs"

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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillClass"));
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Sistema de Ativação", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredKeys"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerKeys"), true);
        EditorGUILayout.Space(10);

        if (skill.skillClass == SkillClass.Movimento)
        {
            EditorGUILayout.LabelField("Configuração da Lógica de Movimento", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSkillType"));
            EditorGUILayout.Space();

            switch (skill.movementSkillType)
            {
                case MovementSkillType.SuperJump:
                    EditorGUILayout.LabelField("Modificadores de Pulo", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("airJumps"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScaleOnFall"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coyoteTime"));
                    break;

                case MovementSkillType.Dash:
                case MovementSkillType.WallDash:
                    EditorGUILayout.LabelField("Modificadores de Dash", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDuration"));
                    break;

                case MovementSkillType.WallJump:
                    EditorGUILayout.LabelField("Modificadores de Pulo de Parede", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpForce"));
                    break;

                case MovementSkillType.WallSlide:
                    EditorGUILayout.LabelField("Modificadores de Deslize na Parede", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideSpeed"));
                    break;

                case MovementSkillType.WallDashJump:
                    EditorGUILayout.LabelField("Modificadores de Lançamento da Parede", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashJump_LaunchForceX"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashJump_LaunchForceY"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashJump_ParabolaDamping"));
                    break;

                case MovementSkillType.DashJump:
                    EditorGUILayout.LabelField("Modificadores de Dash com Pulo", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_DashSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_DashDuration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_JumpForce"));
                    break;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}