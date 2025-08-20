using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemSO))] // AGORA EDITA O ItemSO
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ItemSO item = (ItemSO)target;

        // --- SEÇÃO 1: CAMPOS GERAIS (SEMPRE VISÍVEIS) ---
        EditorGUILayout.LabelField("Informações Gerais", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Configuração do Inventário (Grid)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));

        EditorGUILayout.Space(10);

        // --- SEÇÃO 2: CAMPOS CONTEXTUAIS ---
        // Mostra campos diferentes com base no ItemType selecionado
        switch (item.itemType)
        {
            case ItemType.Weapon:
                DrawWeaponFields();
                break;

            case ItemType.Consumable:
                EditorGUILayout.LabelField("Configurações de Consumível", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("healthToRestore"));
                // (Desenhe outros campos de consumível aqui)
                break;

            case ItemType.KeyItem:
                // Itens chave podem não ter campos extras
                EditorGUILayout.HelpBox("Este é um Item Chave.", MessageType.Info);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Função auxiliar para desenhar todos os campos de arma
    private void DrawWeaponFields()
    {
        ItemSO item = (ItemSO)target;

        EditorGUILayout.LabelField("Configuração de Combate", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useAimMode"));

        EditorGUILayout.Space(5);

        // Mostra campos exclusivos do WeaponType selecionado
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