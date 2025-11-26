using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "AI/Enemy Ultimate Stats")]
public class SO_EnemyStats : ScriptableObject
{
    [Header("--- Identidade & Vida ---")]
    public string enemyName = "Inimigo Padrão";
    public float maxHealth = 100f;

    [Header("--- Movimento ---")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;
    public bool canWander = true;
    public float patrolWaitTime = 2f;

    [Header("--- Comportamento de Patrulha (Aura) ---")]
    public float patrolScanAngle = 45f;
    public float patrolScanSpeed = 2f;

    [Header("--- Sentidos ---")]
    public float visionRange = 15f;
    [Range(0, 360)] public float visionAngle = 90f;
    public float hearingRange = 15f;
    public float memoryDuration = 5f;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;

    [Header("--- Combate ---")]
    public bool isRanged = false;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float damage = 15f;

    // CORREÇÃO: Renomeado para bater com o resto do sistema
    public float knockbackPower = 10f;

    public bool canMoveWhileAttacking = false;

    [Header("--- Configuração Melee ---")]
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 hitboxOffset = new Vector2(0.5f, 0);

    [Header("--- Configuração Ranged ---")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
}