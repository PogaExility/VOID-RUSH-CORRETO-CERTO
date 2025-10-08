using UnityEngine;

[RequireComponent(typeof(EnemyStatsVD))]
[RequireComponent(typeof(Rigidbody2D))]
public class AIControllerVD : MonoBehaviour
{
    [Header("Configuração de Detecção")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float visionRange = 7f;
    [SerializeField] private float visionConeAngle = 45f;
    [SerializeField] private int visionConeRayCount = 5;

    [Header("Pontos de Referência (Transforms)")]
    [SerializeField] private Transform visionOriginPoint;
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private Transform ledgeCheckPoint;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float environmentCheckDistance = 0.2f;

    // --- Referências de Componentes ---
    private EnemyStatsVD enemyStats;
    private Rigidbody2D rb;
    private Transform playerTransform;

    // --- Controle da Máquina de Estados ---
    private AIState currentState;
    private int facingDirection = 1;
    private float lastMeleeAttackTime = -999f;
    private float lastRangedAttackTime = -999f;

    void Awake()
    {
        enemyStats = GetComponent<EnemyStatsVD>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        else Debug.LogError("IA não encontrou o jogador! Verifique se o jogador tem a tag 'Player'.");

        enemyStats.OnEnemyDied += HandleDeath;
        enemyStats.OnDamageTaken += HandleDamageTaken;

        ChangeState(enemyStats.EnemyData.initialState);
    }

    void OnDestroy()
    {
        enemyStats.OnEnemyDied -= HandleDeath;
        enemyStats.OnDamageTaken -= HandleDamageTaken;
    }

    void Update()
    {
        if (playerTransform == null || currentState == AIState.Dead || currentState == AIState.Stunned) return;

        switch (currentState)
        {
            case AIState.Patrolling: UpdatePatrolState(); break;
            case AIState.Chasing: UpdateChaseState(); break;
            case AIState.MeleeAttacking: UpdateMeleeAttackState(); break;
            case AIState.RangedAttacking: UpdateRangedAttackState(); break;
        }
    }

    private void ChangeState(AIState newState)
    {
        if (currentState == newState) return;

        // --- DEBUG ADICIONADO ---
        Debug.Log(gameObject.name + " mudando de estado: " + currentState + " -> " + newState);

        currentState = newState;
    }

    // --- LÓGICA DOS ESTADOS ---

    private void UpdatePatrolState()
    {
        rb.linearVelocity = new Vector2(enemyStats.EnemyData.patrolSpeed * facingDirection, rb.linearVelocity.y);
        if (IsNearWall() || !IsGroundAhead()) Flip();
        if (CanSeePlayer()) ChangeState(AIState.Chasing);
    }

    private void UpdateChaseState()
    {
        var enemyData = enemyStats.EnemyData;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > enemyData.chaseRange)
        {
            ChangeState(AIState.Patrolling);
            return;
        }

        if (enemyData.meleeAttackPrefab != null && distanceToPlayer <= enemyData.meleeAttackRange)
        {
            ChangeState(AIState.MeleeAttacking);
            return;
        }
        if (enemyData.rangedAttackPrefab != null && distanceToPlayer <= enemyData.rangedAttackRange)
        {
            ChangeState(AIState.RangedAttacking);
            return;
        }

        FacePlayer();
        rb.linearVelocity = new Vector2(enemyData.chaseSpeed * facingDirection, rb.linearVelocity.y);
    }

    private void UpdateMeleeAttackState()
    {
        rb.linearVelocity = Vector2.zero;
        var enemyData = enemyStats.EnemyData;

        if (Vector2.Distance(transform.position, playerTransform.position) > enemyData.meleeAttackRange)
        {
            ChangeState(AIState.Chasing);
            return;
        }

        FacePlayer();
        if (Time.time >= lastMeleeAttackTime + enemyData.meleeAttackCooldown)
        {
            PerformMeleeAttack();
            lastMeleeAttackTime = Time.time;
        }
    }

    private void UpdateRangedAttackState()
    {
        rb.linearVelocity = Vector2.zero;
        var enemyData = enemyStats.EnemyData;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        bool shouldReassess = distanceToPlayer > enemyData.rangedAttackRange ||
                              (enemyData.meleeAttackPrefab != null && distanceToPlayer < enemyData.meleeAttackRange);
        if (shouldReassess)
        {
            ChangeState(AIState.Chasing);
            return;
        }

        FacePlayer();
        if (Time.time >= lastRangedAttackTime + enemyData.rangedAttackCooldown)
        {
            PerformRangedAttack();
            lastRangedAttackTime = Time.time;
        }
    }

    // --- FUNÇÕES DE ATAQUE ---

    private void PerformMeleeAttack()
    {
        var enemyData = enemyStats.EnemyData;
        if (enemyData.meleeAttackPrefab == null || attackPoint == null) return;
        GameObject attackGO = Instantiate(enemyData.meleeAttackPrefab, attackPoint.position, attackPoint.rotation);
        if (attackGO.TryGetComponent<EnemyAttackPrefabVD>(out var attackScript))
            attackScript.Initialize(enemyData.meleeAttackDamage, enemyData.meleeAttackKnockbackPower, 0, Vector2.zero);
    }

    private void PerformRangedAttack()
    {
        var enemyData = enemyStats.EnemyData;
        if (enemyData.rangedAttackPrefab == null || attackPoint == null) return;
        GameObject attackGO = Instantiate(enemyData.rangedAttackPrefab, attackPoint.position, attackPoint.rotation);
        if (attackGO.TryGetComponent<EnemyAttackPrefabVD>(out var attackScript))
        {
            Vector2 direction = (playerTransform.position - attackPoint.position).normalized;
            attackScript.Initialize(enemyData.rangedAttackDamage, enemyData.rangedAttackKnockbackPower, enemyData.projectileSpeed, direction);
        }
    }

    // --- FUNÇÕES DE DETECÇÃO E REAÇÃO ---

    private bool IsNearWall() => Physics2D.Raycast(wallCheckPoint.position, Vector2.right * facingDirection, environmentCheckDistance, groundLayer);
    private bool IsGroundAhead() => Physics2D.Raycast(ledgeCheckPoint.position, Vector2.down, environmentCheckDistance, groundLayer);

    private bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        float halfAngle = visionConeAngle / 2;
        float angleStep = visionConeAngle / (visionConeRayCount - 1);

        for (int i = 0; i < visionConeRayCount; i++)
        {
            float angle = -halfAngle + (angleStep * i);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * (transform.right * facingDirection);
            RaycastHit2D hit = Physics2D.Raycast(visionOriginPoint.position, direction, visionRange, playerLayer | groundLayer);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                RaycastHit2D wallCheck = Physics2D.Raycast(visionOriginPoint.position, direction, Vector2.Distance(transform.position, playerTransform.position), groundLayer);
                if (wallCheck.collider == null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // --- FUNÇÕES AUXILIARES ---

    private void FacePlayer()
    {
        if (playerTransform == null) return; // Checagem de segurança
        if (playerTransform.position.x > transform.position.x && facingDirection == -1) Flip();
        else if (playerTransform.position.x < transform.position.x && facingDirection == 1) Flip();
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(0, 180, 0);
    }

    private void HandleDeath()
    {
        ChangeState(AIState.Dead);
        rb.linearVelocity = Vector2.zero;
    }

    private void HandleDamageTaken(Vector2 attackDirection)
    {
        if (attackDirection.x > 0 && facingDirection == -1) Flip();
        else if (attackDirection.x < 0 && facingDirection == 1) Flip();

        if (currentState == AIState.Patrolling) ChangeState(AIState.Chasing);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // --- FUNÇÃO DE GIZMOS REESCRITA E MAIS SEGURA ---
        if (enemyStats == null) enemyStats = GetComponent<EnemyStatsVD>();
        bool hasData = enemyStats != null && enemyStats.EnemyData != null;

        Vector3 forward = transform.right * facingDirection;

        // Gizmos de Detecção de Ambiente
        Gizmos.color = Color.blue;
        if (wallCheckPoint != null) Gizmos.DrawRay(wallCheckPoint.position, forward * environmentCheckDistance);
        if (ledgeCheckPoint != null) Gizmos.DrawRay(ledgeCheckPoint.position, Vector2.down * environmentCheckDistance);

        // Gizmos de Visão (Raios Amarelos)
        if (visionOriginPoint != null)
        {
            Gizmos.color = Color.yellow;
            if (visionConeRayCount > 1)
            {
                float halfAngle = visionConeAngle / 2;
                float angleStep = visionConeAngle / (visionConeRayCount - 1);
                for (int i = 0; i < visionConeRayCount; i++)
                {
                    float angle = -halfAngle + (angleStep * i);
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * forward;
                    Gizmos.DrawRay(visionOriginPoint.position, direction * visionRange);
                }
            }
            else
            {
                Gizmos.DrawRay(visionOriginPoint.position, forward * visionRange);
            }
        }

        // Gizmos de Alcance
        if (hasData)
        {
            var enemyData = enemyStats.EnemyData;

            // Gizmo do Chase Range (Círculo Verde)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, enemyData.chaseRange);

            // Gizmos de Ataque (Raios Direcionais)
            Vector3 attackOrigin = attackPoint != null ? attackPoint.position : transform.position;

            if (enemyData.meleeAttackPrefab != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(attackOrigin, forward * enemyData.meleeAttackRange);
            }
            if (enemyData.rangedAttackPrefab != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(attackOrigin, forward * enemyData.rangedAttackRange);
            }
        }
    }
#endif
}