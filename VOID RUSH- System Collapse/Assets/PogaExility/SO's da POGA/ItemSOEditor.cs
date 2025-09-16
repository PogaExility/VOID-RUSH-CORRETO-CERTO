// ItemSOEditor.cs - VERS�O COMPLETA E FINAL
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
        EditorGUILayout.LabelField("Configura��es de Quest", EditorStyles.boldLabel);
        var pQuestLoss = serializedObject.FindProperty("isLostOnDeathDuringQuest");
        if (item.itemType == ItemType.KeyItem)
        {
            EditorGUILayout.PropertyField(pQuestLoss, new GUIContent("Is Lost On Death During Quest"));
        }
        else
        {
            pQuestLoss.boolValue = false;
            EditorGUILayout.HelpBox("S� itens de Quest exibem esta op��o.", MessageType.None);
        }
        EditorGUILayout.Space(8);

        // ---- Prefabs & V�nculos ----
        EditorGUILayout.LabelField("Prefabs & V�nculos", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("worldPickupPrefab"));
        // O prefab de equipar s� aparece se o item for uma arma.
        if (item.itemType == ItemType.Weapon)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("equipPrefab"));
        EditorGUILayout.Space(8);

        // ---- Campos contextuais por tipo ----
        switch (item.itemType)
        {
            case ItemType.Weapon:
                DrawWeaponFields(item);
                break;

            case ItemType.Ammo:
                // Quando o item � do tipo Ammo, desenha as estat�sticas da muni��o.
                DrawAmmoFields(item);
                break;

            case ItemType.Consumable:
                EditorGUILayout.LabelField("Configura��es de Consum�vel", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("healthToRestore"));
                break;

            case ItemType.Material:
            case ItemType.Utility:
            case ItemType.KeyItem:
                // Nenhum campo extra necess�rio.
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawWeaponFields(ItemSO item)
    {
        EditorGUILayout.LabelField("Configura��es de Arma", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
        EditorGUILayout.Space(4);

        if (item.weaponType == WeaponType.Ranger)
        {
            EditorGUILayout.LabelField("Ranger", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAimMode"));
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Recoil", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("returnSpeed"));
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("P�lvora (Tiro sem Muni��o)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gunpowderPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gunpowderSpawnOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("powderDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("powderRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("powderKnockback"));
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Muni��es aceitas", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("acceptedAmmo"), true);
        }

        else if (item.weaponType == WeaponType.Meelee)
        {
            EditorGUILayout.LabelField("Meelee", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comboResetTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lungeDuration")); 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comboSteps"), true);
        }
        else if (item.weaponType == WeaponType.Buster)
        {
            // ... (c�digo do Buster) ...
        }
    }

    // A NOVA FUN��O QUE ESTAVA FALTANDO
    void DrawAmmoFields(ItemSO item)
    {
        EditorGUILayout.LabelField("Estat�sticas da Muni��o", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletDamage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletLifetime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletKnockback"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pierceCount"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damageFalloff"));
    }
}