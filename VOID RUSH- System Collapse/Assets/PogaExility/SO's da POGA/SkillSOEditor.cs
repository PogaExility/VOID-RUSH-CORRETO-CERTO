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

        // Se��o de Informa��es Gerais (sempre vis�vel)
        EditorGUILayout.LabelField("Informa��es Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activationKey"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("visualEffectPrefab"));

        EditorGUILayout.Space(10);

        // Se��o de Configura��o da Skill (mostra campos diferentes com base na classe)
        EditorGUILayout.LabelField("Configura��o da Skill", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillClass"));

        // Mostra campos espec�ficos dependendo da classe de skill selecionada
        switch (skill.skillClass)
        {
            case SkillClass.Movimento:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSkillType"));

                // Mostra sub-campos dependendo do tipo de movimento
                switch (skill.movementSkillType)
                {
                    case MovementSkillType.Dash:
                        // ====================================================================
                        // MUDAN�A APLICADA AQUI
                        // ====================================================================
                        // Mostra o novo dropdown 'Dash Type' em vez do antigo booleano.
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashType"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDistance"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                        break;

                    case MovementSkillType.SuperJump:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHeightMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("airJumps"));
                        break;
                }
                break;

            case SkillClass.Buff:
                // Exemplo para futuras skills (n�o implementado no seu c�digo de jogo)
                // EditorGUILayout.PropertyField(serializedObject.FindProperty("buffDuration"));
                // EditorGUILayout.PropertyField(serializedObject.FindProperty("buffAmount"));
                break;

            case SkillClass.Dano:
                // Exemplo para futuras skills (n�o implementado no seu c�digo de jogo)
                // EditorGUILayout.PropertyField(serializedObject.FindProperty("damageAmount"));
                // EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRange"));
                break;
        }

        // Aplica todas as mudan�as feitas no Inspector
        serializedObject.ApplyModifiedProperties();
    }
}