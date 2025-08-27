using UnityEngine;

// Definições de tipos (Enums)
public enum ItemType { Consumable, KeyItem, Weapon }
public enum WeaponType { Melee, Firearm, Buster }

[CreateAssetMenu(fileName = "NewItem", menuName = "NEXUS/Itens/Novo Item", order = 0)]
public class ItemSO : ScriptableObject
{
    [Tooltip("O prefab do objeto que representa este item no mundo do jogo (para ser coletado ou jogado fora).")]
    public GameObject itemPrefab;
    [Header("Informações Gerais")]
    public string itemName;
    public Sprite itemIcon;
    [Tooltip("Define o tipo principal do item.")]
    public ItemType itemType;
    [Tooltip("Se marcado, este item será removido do inventário se o jogador morrer durante uma quest ativa.")]
    public bool isLostOnDeathDuringQuest = false;

    [Header("Configuração do Inventário (Grid)")]
    [Range(1, 6)] public int width = 1;
    [Range(1, 6)] public int height = 1;

    // --- CAMPOS DE ARMA (WEAPON) ---
    [Header("Configurações de Combate (se for Arma)")]
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

    // --- CAMPOS DE CONSUMÍVEL (CONSUMABLE) ---
    [Header("Configurações de Consumível")]
    public float healthToRestore;
    // (Adicione outros efeitos de consumíveis aqui no futuro)
}