using UnityEngine;

// Defini��es de tipos (Enums)
// N�O REORDENE - Adicione novos tipos apenas no final.
public enum ItemType { Consumable, KeyItem, Weapon, Ammo, Material, Utility }
public enum WeaponType { Melee, Firearm, Buster }

[CreateAssetMenu(fileName = "NewItem", menuName = "NEXUS/Itens/Novo Item", order = 0)]
public class ItemSO : ScriptableObject
{
    [Header("Informa��es Gerais")]
    public string itemName;
    public Sprite itemIcon;
    [Tooltip("Define o tipo principal do item.")]
    public ItemType itemType;

    [Header("Configura��o do Invent�rio")]
    [Tooltip("Se marcado, este item pode ser empilhado no mesmo slot.")]
    public bool stackable = true;
    [Tooltip("A quantidade m�xima deste item por slot de invent�rio.")]
    public int maxStack = 999;

    // Campo antigo, mantido para compatibilidade, mas ignorado pelo novo sistema 1x1.
    [HideInInspector] public int width = 1;
    [HideInInspector] public int height = 1;

    [Header("Configura��es de Quest")]
    [Tooltip("Se marcado, este item ser� removido do invent�rio se o jogador morrer enquanto uma quest estiver ativa.")]
    public bool isLostOnDeathDuringQuest = false;

    // --- CAMPOS DE ARMA (WEAPON) ---
    [Header("Configura��es de Combate (se for Arma)")]
    public WeaponType weaponType;
    public float damage;
    public float attackRate = 0.5f;
    public bool useAimMode = false;
    public GameObject slashEffectPrefab;
    public GameObject bulletPrefab;
    public GameObject busterShotPrefab;
    public GameObject chargedShotPrefab;

    [Header("Melee")]
    public AnimationClip[] comboAnimations;

    [Header("Firearm")]
    public int magazineSize;
    public float reloadTime;

    [Header("Buster")]
    public float chargeTime;
    public float energyCostPerChargeSecond;
    public float baseEnergyCost;
    public float chargedShotDamage;

    // --- CAMPOS DE CONSUM�VEL (CONSUMABLE) ---
    [Header("Configura��es de Consum�vel")]
    public float healthToRestore;
}