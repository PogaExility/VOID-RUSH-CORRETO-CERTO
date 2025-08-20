using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponSO))]
public class WeaponSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        WeaponSO weapon = (WeaponSO)target;

        // --- Campos do ItemSO (Pai) ---
        EditorGUILayout.LabelField("Informa��es Gerais do Item", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));

        EditorGUILayout.Space(10);

        // --- Campos do WeaponSO (Filho) ---
        EditorGUILayout.LabelField("Configura��o de Combate", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));

        // ===== IN�CIO DA ALTERA��O: EXIBINDO OS NOVOS CAMPOS GERAIS =====
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useAimMode"));
        // ===== FIM DA ALTERA��O =====

        EditorGUILayout.Space(5);

        // --- L�gica para mostrar/esconder campos ---
        switch (weapon.weaponType)
        {
            case WeaponType.Melee:
                EditorGUILayout.LabelField("Exclusivo: Corpo a Corpo", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("comboAnimations"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("slashEffectPrefab"));
                break;

            case WeaponType.Firearm:
                EditorGUILayout.LabelField("Exclusivo: Arma de Fogo", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadTime"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletPrefab"));
                break;

            case WeaponType.Buster:
                EditorGUILayout.LabelField("Exclusivo: Buster", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("baseEnergyCost"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("busterShotPrefab"));
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Tiro Carregado", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chargeTime"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCostPerChargeSecond"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chargedShotDamage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chargedShotPrefab"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}