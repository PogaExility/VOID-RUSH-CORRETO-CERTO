using UnityEngine;
using System.Collections.Generic;

public enum ItemType { Consumable, KeyItem, Weapon, Ammo, Material, Utility }
public enum WeaponType { Melee, Firearm, Buster }

[CreateAssetMenu(fileName = "NewItem", menuName = "NEXUS/Itens/Novo Item", order = 0)]
public class ItemSO : ScriptableObject
{

    [Header("Informa��es Gerais")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    [Tooltip("O prefab do objeto que representa este item no mundo do jogo.")]
    public GameObject itemPrefab;

    [Header("Prefabs & V�nculos")]
    public GameObject worldPickupPrefab;   // prefab do item no mundo (para soltar/loot)
    public GameObject equipPrefab;         // prefab da arma na m�o (m�o do player / socket)


    [Header("Configura��o do Invent�rio")]
    public bool stackable = true;
    public int maxStack = 999;
    [Header("Arma (v�nculos de muni��o)")]
    public ItemSO[] acceptedAmmo;
    [Header("Dano por modo")]
    public float bulletDamage = 10f;       // dano quando tem muni��o
    public float powderDamage = 2f;        // �tiro de p�lvora� (curto alcance) quando SEM muni��o
    public float powderRange = 1.8f;      // a

    [HideInInspector] public int width = 1;
    [HideInInspector] public int height = 1;

    [Header("Configura��es de Quest")]
    public bool isLostOnDeathDuringQuest = false;

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

    [Header("Configura��es de Consum�vel")]
    public float healthToRestore;
}