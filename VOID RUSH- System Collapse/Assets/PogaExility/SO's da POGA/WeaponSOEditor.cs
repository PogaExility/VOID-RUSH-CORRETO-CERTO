using UnityEngine;
using UnityEditor;

// Este atributo diz à Unity para usar este editor customizado para todos os objetos do tipo 'WeaponSO'.
[CustomEditor(typeof(WeaponSO))]
public class WeaponSOEditor : Editor
{
    // Esta função redesenha o Inspector para o WeaponSO.
    public override void OnInspectorGUI()
    {
        // Puxa as propriedades mais recentes do objeto para evitar bugs
        serializedObject.Update();

        // Pega uma referência ao objeto que estamos inspecionando
        WeaponSO weapon = (WeaponSO)target;

        // --- Desenha os campos do ItemSO (Pai) ---
        EditorGUILayout.LabelField("Informações Gerais do Item", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));

        EditorGUILayout.Space(10); // Adiciona um espaço para organização

        // --- Desenha os campos do WeaponSO (Filho) ---
        EditorGUILayout.LabelField("Configuração de Combate", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));

        EditorGUILayout.Space(5);

        // --- Lógica para mostrar/esconder campos ---
        // Desenha campos específicos dependendo do tipo de arma selecionado no dropdown.
        switch (weapon.weaponType)
        {
            case WeaponType.Melee:
                EditorGUILayout.LabelField("Exclusivo: Corpo a Corpo", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("comboAnimations"), true); // 'true' permite editar o array
                break;

            case WeaponType.Firearm:
                EditorGUILayout.LabelField("Exclusivo: Arma de Fogo", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadTime"));
                break;

            case WeaponType.Buster:
                EditorGUILayout.LabelField("Exclusivo: Buster", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chargeTime"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCostPerChargeSecond"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("baseEnergyCost"));
                break;
        }

        // Aplica todas as mudanças feitas no Inspector para que sejam salvas.
        serializedObject.ApplyModifiedProperties();
    }
}