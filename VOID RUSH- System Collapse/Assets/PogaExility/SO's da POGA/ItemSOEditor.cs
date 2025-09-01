using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ItemSO item = (ItemSO)target;

        // --- SE��O 1: CAMPOS GERAIS (SEMPRE VIS�VEIS) ---
        EditorGUILayout.LabelField("Informa��es Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Configura��o do Invent�rio", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stackable"));
        if (item.stackable)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStack"));
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isLostOnDeathDuringQuest"));
        EditorGUILayout.Space(10);

        // --- SE��O 2: CAMPOS CONTEXTUAIS ---
        switch (item.itemType)
        {
            case ItemType.Weapon:
                DrawWeaponFields();
                break;

            case ItemType.Consumable:
                EditorGUILayout.LabelField("Configura��es de Consum�vel", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("healthToRestore"));
                break;

            case ItemType.Ammo:
                EditorGUILayout.HelpBox("Este � um item de Muni��o.", MessageType.Info);
                break;

            case ItemType.Material:
                EditorGUILayout.HelpBox("Este � um Material de Crafting.", MessageType.Info);
                break;

            case ItemType.Utility:
                EditorGUILayout.HelpBox("Este � um item Utilit�rio.", MessageType.Info);
                break;

            case ItemType.KeyItem:
                EditorGUILayout.HelpBox("Este � um Item Chave. Geralmente n�o � empilh�vel.", MessageType.Info);
                if (item.stackable) item.stackable = false;
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawWeaponFields()
    {
        ItemSO item = (ItemSO)target;
        EditorGUILayout.LabelField("Configura��o de Combate", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useAimMode"));
        EditorGUILayout.Space(5);

        switch (item.weaponType)
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
    }
}