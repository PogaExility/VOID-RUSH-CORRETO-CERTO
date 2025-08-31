using UnityEngine;
using UnityEditor; // Essencial para criar um Editor customizado

/// <summary>
/// Este script customiza a forma como o SkillSO � exibido no Inspector da Unity.
/// Ele organiza os campos em se��es l�gicas e mostra apenas os par�metros
/// de f�sica relevantes para a a��o de movimento selecionada, mantendo a
/// interface limpa e f�cil de usar.
/// </summary>
[CustomEditor(typeof(SkillSO))]
public class SkillSOEditor : Editor
{
    /// <summary>
    /// Esta fun��o � chamada pela Unity para desenhar a UI do Inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Padr�o de in�cio para qualquer editor customizado. Prepara o objeto para ser modificado.
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cancelIfKeysHeld"), true);
        EditorGUILayout.Space(10);

        // --- Bloco de L�gica da A��o ---
        EditorGUILayout.LabelField("L�gica da A��o", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("actionToPerform"));
        EditorGUILayout.Space(10);

        // --- A NOVA UI DE CONDI��ES AVAN�ADAS ---
        EditorGUILayout.LabelField("Condi��es de Ativa��o", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionGroups"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("forbiddenStates"), true);
        EditorGUILayout.Space(10);

        // --- Bloco de Par�metros de F�sica (Contextual) ---
        // Mostra apenas os campos de f�sica relevantes para a a��o selecionada.
        if (skill.skillClass == SkillClass.Movimento && skill.actionToPerform != MovementSkillType.None)
        {
            EditorGUILayout.LabelField("Par�metros de F�sica", EditorStyles.boldLabel);

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

        // Padr�o de finaliza��o. Aplica todas as mudan�as feitas no Inspector.
        serializedObject.ApplyModifiedProperties();
    }
}