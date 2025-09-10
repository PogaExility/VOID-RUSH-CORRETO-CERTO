// FILE: ItemSOEditor.cs
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ItemSO item = (ItemSO)target;

        // ---- Gerais ----
        EditorGUILayout.LabelField("Informações Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemPrefab"));
        EditorGUILayout.Space(8);

        // ---- Inventário ----
        EditorGUILayout.LabelField("Configuração do Inventário", EditorStyles.boldLabel);
        var pStackable = serializedObject.FindProperty("stackable");
        var pMaxStack = serializedObject.FindProperty("maxStack");

        if (item.itemType == ItemType.Weapon)
        {
            // Arma nunca empilha
            pStackable.boolValue = false;
            EditorGUILayout.HelpBox("Armas não são empilháveis.", MessageType.None);
        }
        else
        {
            EditorGUILayout.PropertyField(pStackable);
            if (pStackable.boolValue)
                EditorGUILayout.PropertyField(pMaxStack);
        }
        EditorGUILayout.Space(8);

        // ---- Quest ----
        var pQuestLoss = serializedObject.FindProperty("isLostOnDeathDuringQuest");
        EditorGUILayout.LabelField("Configurações de Quest", EditorStyles.boldLabel);
        if (item.itemType == ItemType.KeyItem)
        {
            EditorGUILayout.PropertyField(pQuestLoss, new GUIContent("Is Lost On Death During Quest"));
        }
        else
        {
            pQuestLoss.boolValue = false; // oculta e força false
            EditorGUILayout.HelpBox("Só itens de Quest exibem esta opção.", MessageType.None);
        }
        EditorGUILayout.Space(8);

        // ---- Prefabs & Vínculos ----
        EditorGUILayout.LabelField("Prefabs & Vínculos", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("worldPickupPrefab"));
        if (item.itemType == ItemType.Weapon)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("equipPrefab"));

        // ---- Campos contextuais por tipo ----
        switch (item.itemType)
        {
            case ItemType.Weapon:
                DrawWeaponFields(item);
                break;
            case ItemType.Consumable:
                EditorGUILayout.LabelField("Configurações de Consumível", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("healthToRestore"));
                break;
            case ItemType.Ammo:
                EditorGUILayout.LabelField("Munição", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Este item é munição. Use maxStack alto (ex.: 9999).", MessageType.Info);
                break;
            case ItemType.Material:
            case ItemType.Utility:
            case ItemType.KeyItem:
                // nada extra
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawWeaponFields(ItemSO item)
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Arma (Combate)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useAimMode"));

        if (item.weaponType == WeaponType.Meelee)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comboAnimations"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slashEffectPrefab"));
        }
        else if (item.weaponType == WeaponType.Ranger)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletPrefab"));
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Munições aceitas", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("acceptedAmmo"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletLifetime"));
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pierceCount"));      
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damageFalloff"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("powderDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("powderRange"));
        }
        else if (item.weaponType == WeaponType.Buster)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseEnergyCost"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("busterShotPrefab"));
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Tiro Carregado", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chargeTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("energyCostPerChargeSecond"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chargedShotDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chargedShotPrefab"));
        }
    }
}
