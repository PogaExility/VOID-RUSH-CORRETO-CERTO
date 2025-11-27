using UnityEngine;
using UnityEditor; // Essencial para criar um Editor customizado

/// <summary>
/// Este script customiza a forma como o SkillSO é exibido no Inspector da Unity.
/// Ele organiza os campos em seções lógicas e mostra apenas os parâmetros
/// de física relevantes para a ação de movimento selecionada, mantendo a
/// interface limpa e fácil de usar.
/// </summary>
[CustomEditor(typeof(SkillSO))]
public class SkillSOEditor : Editor
{
    /// <summary>
    /// Esta função é chamada pela Unity para desenhar a UI do Inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Padrão de início para qualquer editor customizado. Prepara o objeto para ser modificado.
        serializedObject.Update();
        SkillSO skill = (SkillSO)target;

        // --- Bloco de Informações Gerais (Sempre Visível) ---
        EditorGUILayout.LabelField("Informações Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldownDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillClass"));

        // --- ADIÇÃO: Exibe o campo SkillTier logo abaixo da Classe ---
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillTier"));

        EditorGUILayout.Space(10);

        // --- Bloco do Sistema de Ativação (Sempre Visível) ---
        EditorGUILayout.LabelField("Sistema de Ativação", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerKeys"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredKeys"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cancelIfKeysHeld"), true);
        EditorGUILayout.Space(10);

        // --- Bloco de Lógica da Ação ---



        // --- A NOVA UI DE CONDIÇÕES AVANÇADAS ---

        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionGroups"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("forbiddenStates"), true);
        EditorGUILayout.Space(10);

        // --- Bloco de Parâmetros de Física (Contextual) ---
        EditorGUILayout.LabelField("Parâmetros da Ação", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("actionToPerform"));
        EditorGUILayout.Space(10);

        if (skill.skillClass == SkillClass.Movimento)
        {
            switch (skill.actionToPerform)
            {
                case MovementSkillType.SuperJump:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("airJumps"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScaleOnFall"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coyoteTime"));
                    break;

                case MovementSkillType.Dash:
                case MovementSkillType.WallDash:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDuration"));
                    break;

                case MovementSkillType.WallJump:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpForce"));
                    break;

                case MovementSkillType.WallSlide:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideSpeed"));
                    break;

                case MovementSkillType.WallDashJump:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashJump_LaunchForceX"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashJump_LaunchForceY"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashJump_ParabolaDamping"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashJump_GravityScaleOnFall"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDashJump_InputBuffer"));
                    break;

                case MovementSkillType.DashJump:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_DashSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_DashDuration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_JumpForce"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_ParabolaDamping"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_GravityScaleOnFall"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashJump_InputBuffer"));
                    break;
            }
        }


        else if (skill.skillClass == SkillClass.Combate)
        {
            // --- O NOVO SWITCH CASE PARA COMBATE ---
            switch (skill.combatActionToPerform)
            {
                case CombatSkillType.Block:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("block_ParryWindow"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("block_DamageReduction"));
                    break;

                case CombatSkillType.Parry:
                    EditorGUILayout.HelpBox("Os parâmetros de Parry são usados quando um Block resulta em um Parry. Configure-os aqui.", MessageType.Info);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("parry_CounterDamageMultiplier"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("parry_StunDuration"));
                    break;

                    // Adicione cases para MeleeAttack, FirearmAttack, etc. aqui no futuro
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}