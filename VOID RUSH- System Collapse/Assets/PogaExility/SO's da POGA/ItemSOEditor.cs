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
        EditorGUILayout.LabelField("Informa��es Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemPrefab"));
        EditorGUILayout.Space(8);

        // ---- Invent�rio ----
        EditorGUILayout.LabelField("Configura��o do Invent�rio", EditorStyles.boldLabel);
        var pStackable = serializedObject.FindProperty("stackable");
        var pMaxStack = serializedObject.FindProperty("maxStack");

        if (item.itemType == ItemType.Weapon)
        {
            // Arma nunca empilha
            pStackable.boolValue = false;
            EditorGUILayout.HelpBox("Armas n�o s�o empilh�veis.", MessageType.None);
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
        EditorGUILayout.LabelField("Configura��es de Quest", EditorStyles.boldLabel);
        if (item.itemType == ItemType.KeyItem)
        {
            EditorGUILayout.PropertyField(pQuestLoss, new GUIContent("Is Lost On Death During Quest"));
        }
        else
        {
            pQuestLoss.boolValue = false; // oculta e for�a false
            EditorGUILayout.HelpBox("S� itens de Quest exibem esta op��o.", MessageType.None);
        }
        EditorGUILayout.Space(8);

        // ---- Prefabs & V�nculos ----
        EditorGUILayout.LabelField("Prefabs & V�nculos", EditorStyles.boldLabel);
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
                EditorGUILayout.LabelField("Configura��es de Consum�vel", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("healthToRestore"));
                break;
            case ItemType.Ammo:
                EditorGUILayout.LabelField("Muni��o", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Este item � muni��o. Use maxStack alto (ex.: 9999).", MessageType.Info);
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
            EditorGUILayout.LabelField("Muni��es aceitas", EditorStyles.boldLabel);
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
