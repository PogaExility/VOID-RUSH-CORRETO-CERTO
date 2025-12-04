using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "AI/Enemy Ultimate Stats")]
public class SO_EnemyStats : ScriptableObject
{
    [Header("--- Identidade & Vida ---")]
    public string enemyName = "Inimigo";
    public float maxHealth = 100f;

    [Header("--- Efeitos Visuais (VFX) ---")]
    public GameObject hitVFX;
    public GameObject deathVFX;
    public GameObject meleeAttackVFX;

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
    public float visionRange = 10f; // Alcance do Cone

    // --- NOVO: Visão em Área (Proximidade) ---
    [Tooltip("Se o player entrar nesse raio, é detectado independente do ângulo ou paredes.")]
    public float proximityDetectionRange = 3f;
    // ----------------------------------------

    [Range(0, 360)] public float visionAngle = 110f; // Ângulo do Cone
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
    public bool isImmuneToKnockback = false;
    [Tooltip("Quanto de força é subtraída do empurrão recebido.")]
    public float knockbackResistance = 0f;
    public bool canMoveWhileAttacking = false;

    [Header("--- Modo Kamikaze ---")]
    public bool isExploder = false;
    public float explosionRadius = 3f;
    public float explosionDamage = 50f;
    public float explosionFuseTime = 1.5f;
    public GameObject explosionVFX;

    [Header("--- Melee ---")]
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 hitboxOffset = new Vector2(0.5f, 0);

    [Header("--- Ranged ---")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;

    [Header("--- Áudio (SFX) ---")]
    public AudioClip idleSound;
    public AudioClip[] footstepSounds;
    public AudioClip attackSound;
    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioClip explosionSound;
    public AudioClip fuseSound;
}