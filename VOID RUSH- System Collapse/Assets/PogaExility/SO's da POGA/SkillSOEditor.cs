using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkillSO))]
public class SkillSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Puxa as propriedades mais recentes do objeto SkillSO
        serializedObject.Update();
        SkillSO skill = (SkillSO)target;

        // Seção de Informações Gerais (sempre visível)
        EditorGUILayout.LabelField("Informações Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activationKey"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("visualEffectPrefab"));

        EditorGUILayout.Space(10);

        // Seção de Configuração da Skill (mostra campos diferentes com base na classe)
        EditorGUILayout.LabelField("Configuração da Skill", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillClass"));

        // Mostra campos específicos dependendo da classe de skill selecionada
        switch (skill.skillClass)
        {
            case SkillClass.Movimento:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSkillType"));

                // Mostra sub-campos dependendo do tipo de movimento
                switch (skill.movementSkillType)
                {
                    case MovementSkillType.Dash:
                        // Campos para o Dash NORMAL (de chão ou aéreo)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashType"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDistance"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                        break;

                    // ===== INÍCIO DA CORREÇÃO =====
                    // NOVO CASO: Mostra os campos corretos APENAS para o Wall Dash
                    case MovementSkillType.WallDash:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashDuration"));
                        break;
                    // ===== FIM DA CORREÇÃO =====

                    case MovementSkillType.SuperJump:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHeightMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("airJumps"));
                        break;
                }
                break;

            case SkillClass.Buff:
                // Exemplo para futuras skills
                EditorGUILayout.HelpBox("Configurações para skills de Buff ainda não implementadas.", MessageType.Info);
                break;

            case SkillClass.Dano:
                // Exemplo para futuras skills
                EditorGUILayout.HelpBox("Configurações para skills de Dano ainda não implementadas.", MessageType.Info);
                break;
        }

        // Aplica todas as mudanças feitas no Inspector
        serializedObject.ApplyModifiedProperties();
    }
}