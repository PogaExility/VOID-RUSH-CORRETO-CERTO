using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "AI/Enemy Ultimate Stats")]
public class SO_EnemyStats : ScriptableObject
{
    [Header("--- Identidade & Vida ---")]
    public string enemyName = "Inimigo";
    public float maxHealth = 100f;

    [Header("--- Efeitos Visuais (VFX) ---")]
    public GameObject hitVFX;         // Sangue/Faísca ao tomar dano
    public GameObject deathVFX;       // Fumaça/Explosão pequena ao morrer
    public GameObject meleeAttackVFX; // Faísca quando ele soca/bate

    [Header("--- Movimento ---")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public bool canWander = true;
    public float patrolWaitTime = 2f;

    [Header("--- Espaçamento ---")]
    public float stopDistancePadding = 0.5f;

    [Header("--- Comportamento de Patrulha ---")]
    public float patrolScanAngle = 45f;
    public float patrolScanSpeed = 2f;

    [Header("--- Sentidos ---")]
    public float visionRange = 10f;
    [Range(0, 360)] public float visionAngle = 110f;
    public float hearingRange = 15f;
    public float memoryDuration = 5f;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;

    [Header("--- Combate ---")]
    public bool isRanged = false;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float postAttackDelay = 0.5f;

    public float damage = 10f;
    public float knockbackPower = 10f;
    public bool canMoveWhileAttacking = false;

    [Header("--- Modo Kamikaze ---")]
    public bool isExploder = false; // <--- O NOME CORRETO É ESTE
    public float explosionRadius = 3f;
    public float explosionFuseTime = 1.5f;
    public GameObject explosionVFX; // Explosão GRANDE do Kamikaze

    [Header("--- Melee ---")]
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 hitboxOffset = new Vector2(0.5f, 0);

    [Header("--- Ranged ---")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
}