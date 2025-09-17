// ItemSO.cs - VERS�O COMPLETA E CORRIGIDA
using UnityEngine;
using System.Collections.Generic;

public enum ItemType { Consumable, KeyItem, Weapon, Ammo, Material, Utility }
public enum WeaponType { Meelee, Ranger, Buster }

[CreateAssetMenu(fileName = "NewItem", menuName = "NEXUS/Itens/Novo Item", order = 0)]
public class ItemSO : ScriptableObject
{
    [Header("Informa��es Gerais")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public GameObject itemPrefab;

    [Header("Prefabs & V�nculos")]
    public GameObject worldPickupPrefab;
    public GameObject equipPrefab;

    [Header("Configura��o do Invent�rio")]
    public bool stackable = true;
    public int maxStack = 999;

    [Header("Configura��es de Quest")]
    public bool isLostOnDeathDuringQuest = false;

    [Header("Configura��es de Combate (se for Arma)")]
    public WeaponType weaponType;
   

    [Header("Ranger (Arma)")]
    public int magazineSize;
    public float reloadTime;
    public float attackRate = 0.5f;
    public bool useAimMode = false;

    // ADICIONE ESTAS 3 LINHAS PARA O RECOIL
    [Header("Recoil")]
    public float recoilDistance = 0.5f; // O qu�o para tr�s a arma vai
    public float recoilSpeed = 20f;     // A velocidade do "soco" para tr�s
    public float returnSpeed = 5f;      // A velocidade da volta suave para o lugar

    [Header("P�lvora")] // Reorganizado para clareza
    public GameObject gunpowderPrefab; // ADICIONE ESTA LINHA PARA O PREFAB DA EXPLOS�O
    public float powderDamage = 2f;
    public float powderRange = 2f;
    public float powderKnockback = 3f;
    public ItemSO[] acceptedAmmo;
    [Tooltip("A dist�ncia para frente do cano onde o efeito de p�lvora vai aparecer.")]
    public float gunpowderSpawnOffset = 0.5f;

    [Header("Ammo Stats (se itemType for Ammo)")]
    public GameObject bulletPrefab;
    public float bulletDamage = 10f;
    public float bulletSpeed = 20f;
    public float bulletLifetime = 3f;
    public float bulletKnockback = 5f;
    public int pierceCount = 0;
    public float damageFalloff = 0.3f;

    [Header("Meelee")]
    [Tooltip("Cooldown em segundos AP�S o �ltimo golpe de um combo antes que um novo possa come�ar.")]
    public float attackCooldown = 0.2f; // <<-- ADICIONE ESTA LINHA
    [Tooltip("Tempo em segundos que o combo fica 'aberto' antes de resetar para o primeiro golpe.")]
    public float comboResetTime = 0.8f;
    public float lungeDuration = 0.15f;
    [Tooltip("A lista de golpes que comp�em a sequ�ncia de combo desta arma.")]
    public System.Collections.Generic.List<ComboStepData> comboSteps = new System.Collections.Generic.List<ComboStepData>();

    [Header("Buster")]
    public float chargeTime;
    public float energyCostPerChargeSecond;
    public float baseEnergyCost;
    public float chargedShotDamage;
    public GameObject busterShotPrefab;
    public GameObject chargedShotPrefab;

    [Header("Configura��es de Consum�vel")]
    public float healthToRestore;
}