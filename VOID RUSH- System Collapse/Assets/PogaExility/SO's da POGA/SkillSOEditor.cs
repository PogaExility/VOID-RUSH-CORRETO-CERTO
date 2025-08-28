using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkillSO))]
public class SkillSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Padr�o de in�cio para qualquer editor customizado
        serializedObject.Update();
        SkillSO skill = (SkillSO)target;

        // --- Bloco de Informa��es Gerais (Sempre Vis�vel) ---
        EditorGUILayout.LabelField("Informa��es Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillClass"));
        EditorGUILayout.Space(10);

        // --- Bloco do Sistema de Ativa��o (Sempre Vis�vel) ---
        EditorGUILayout.LabelField("Sistema de Ativa��o", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredKeys"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerKeys"), true);
        EditorGUILayout.Space(10);

        // --- Bloco Contextual de Skills (Aparece se for de Movimento) ---
        if (skill.skillClass == SkillClass.Movimento)
        {
            EditorGUILayout.LabelField("Configura��o da L�gica de Movimento", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSkillType"));
            EditorGUILayout.Space();

            // Mostra os campos de modificadores relevantes baseados no TIPO de movimento selecionado.
            // Isso mant�m o Inspector limpo.
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
                    EditorGUILayout.LabelField("Modificadores de Parede", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpForce"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideSpeed"));
                    break;

                case MovementSkillType.WallDashJump:
                    EditorGUILayout.LabelField("Modificadores de WallDashJump", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("launchForceX"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("launchForceY"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("parabolaLinearDamping"));
                    break;

                case MovementSkillType.None:
                case MovementSkillType.Stealth:
                    // N�o mostra nenhum campo extra para estes tipos por enquanto
                    break;
            }
        }
        else if (skill.skillClass == SkillClass.Buff)
        {
            EditorGUILayout.HelpBox("Configura��es para Skills de Buff.", MessageType.Info);
            // Adicionar campos de buff aqui no futuro
        }
        else if (skill.skillClass == SkillClass.Dano)
        {
            EditorGUILayout.HelpBox("Configura��es para Skills de Dano.", MessageType.Info);
            // Adicionar campos de dano aqui no futuro
        }


        // Padr�o de finaliza��o para qualquer editor customizado
        serializedObject.ApplyModifiedProperties();
    }
}