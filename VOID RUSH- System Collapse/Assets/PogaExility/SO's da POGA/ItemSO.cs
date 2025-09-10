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
    public float attackRate = 0.5f;
    public bool useAimMode = false;

    [Header("Ranger")]
    public int magazineSize;
    public float reloadTime;
    public GameObject bulletPrefab;
    [Space(5)] // Adiciona um pequeno espa�o no Inspector
    public float bulletDamage = 10f;
    public float bulletSpeed = 20f;
    public float bulletLifetime = 3f;
    [Space(5)]
    [Tooltip("Quantos inimigos a bala pode perfurar. 0 = n�o perfura.")]
    public int pierceCount = 0; // <-- ADICIONADO
    [Tooltip("Percentual de dano perdido a cada perfura��o. 0.3 = 30%. Use valores negativos para AUMENTAR o dano.")]
    public float damageFalloff = 0.3f; // <-- ADICIONADO
    [Space(5)]
    public float powderDamage = 2f;
    public float powderRange = 2f;
    public ItemSO[] acceptedAmmo;

    [Header("Meelee")]
    public float damage;
    public AnimationClip[] comboAnimations;
    public GameObject slashEffectPrefab;

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