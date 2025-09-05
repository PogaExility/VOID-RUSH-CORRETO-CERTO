using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AIMotor_Basic))]
public class AIController_Basic : MonoBehaviour
{
    #region Enums e Referências
    private enum State { Patrolling, Hunting, Attacking, Dead }
    private enum AttackType { Melee, Ranged } // O tipo de ataque que esta IA usará

    private State currentState;
    private AIMotor_Basic motor;
    private Transform playerTarget;
    #endregion

    #region Parâmetros de Comportamento (SEU PAINEL DE CONTROLE)

    [Header("▶ STATUS E COMBATE")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float climbSpeed = 3f; // Usado se a escalada for reativada
    public float knockbackForce = 5f; // Força do knockback que o JOGADOR recebe

    [Header("▶ CONFIGURAÇÃO DE ATAQUE")]
    [Tooltip("Define se esta IA ataca de perto ou de longe.")]
    [SerializeField] private AttackType attackType = AttackType.Melee;
    public float attackDamage = 15f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;
    [Tooltip("Apenas para ataque à distância: o prefab do projétil a ser disparado.")]
    public GameObject projectilePrefab;
    [Tooltip("Apenas para ataque à distância: a força com que o projétil é disparado.")]
    public float projectileSpeed = 10f;

    [Header("▶ PERCEPÇÃO")]
    public Transform eyes;
    public float visionRange = 15f;
    [Range(0, 180)] public float visionAngle = 90f;
    public LayerMask playerLayer;
    public LayerMask visionBlockers;
    #endregion

    #region Variáveis Internas
    private float currentHealth;
    private bool isFacingRight = true;
    private bool canAttack = true;
    private bool isInvincible = false;
    private bool isExecutingAction = false; // <-- ADICIONE ESTA LINHA
    #endregion

    #region Unity Lifecycle
    void Awake() { motor = GetComponent<AIMotor_Basic>(); }

    void Start()
    {
        if (AIManager.Instance != null) playerTarget = AIManager.Instance.playerTarget;
        isFacingRight = transform.localScale.x > 0;
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
        currentHealth = maxHealth;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        bool canSeePlayer = CanSeePlayer();
        float distanceToPlayer = (playerTarget != null) ? Vector2.Distance(transform.position, playerTarget.position) : float.MaxValue;

        // --- TOMADA DE DECISÃO ---
        if (canSeePlayer)
        {
            // Se o jogador está dentro do alcance de ataque, ataca. Senão, caça.
            if (distanceToPlayer <= attackRange) ChangeState(State.Attacking);
            else ChangeState(State.Hunting);
        }
        else
        {
            // Se perdeu o jogador de vista, volta a patrulhar.
            if (currentState == State.Hunting || currentState == State.Attacking) ChangeState(State.Patrolling);
        }

        // --- EXECUÇÃO ---
        switch (currentState)
        {
            case State.Patrolling:
                if (motor.IsObstacleAhead() || !motor.IsGroundAhead()) Flip();
                else motor.Move(isFacingRight ? 1 : -1, moveSpeed);
                break;
            case State.Hunting:
                FaceTarget(playerTarget.position);
                if (motor.IsObstacleAhead()) motor.Stop();
                else motor.Move(isFacingRight ? 1 : -1, moveSpeed);
                break;
            case State.Attacking:
                FaceTarget(playerTarget.position);
                motor.Stop();
                if (canAttack) StartCoroutine(AttackCoroutine());
                break;
        }
    }
    #endregion

    #region Health and Damage

    /// <summary>
    /// Função pública para que outros objetos (como projéteis do jogador) possam causar dano a esta IA.
    /// </summary>
    public void TakeDamage(float amount, Vector2 attackDirection)
    {
        if (isInvincible || currentState == State.Dead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"{gameObject.name} tomou {amount} de dano. Vida restante: {currentHealth}");

        if (currentHealth > 0)
        {
            StartCoroutine(DamageFeedbackCoroutine());
            motor.ApplyKnockback(attackDirection);
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} foi derrotado.");
        ChangeState(State.Dead);
        GetComponent<Collider2D>().enabled = false;
        motor.Stop();
        // Adicione aqui animação de morte, efeitos, etc.
        Destroy(gameObject, 3f);
    }

    // Efeito visual de piscar ao tomar dano, inspirado no seu PlayerStats.
    private IEnumerator DamageFeedbackCoroutine()
    {
        isInvincible = true;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            for (int i = 0; i < 3; i++)
            {
                sprite.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                sprite.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
        }
        isInvincible = false;
    }
    #endregion

    #region Funções de Suporte e Corrotinas

    private IEnumerator AttackCoroutine()
    {
        canAttack = false;
        isExecutingAction = true;

        Debug.Log($"Inimigo preparando ataque do tipo: {attackType}");
        // Animação de "preparar o ataque"
        yield return new WaitForSeconds(0.5f);

        if (attackType == AttackType.Melee)
        {
            // ATAQUE CORPO A CORPO
            Collider2D[] targetsHit = Physics2D.OverlapCircleAll(eyes.position, attackRange, playerLayer);
            foreach (Collider2D target in targetsHit)
            {
                if (target.TryGetComponent<PlayerStats>(out PlayerStats player))
                {
                    Debug.Log($"Ataque corpo a corpo atingiu {player.name}!");
                    Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                    player.TakeDamage(attackDamage, knockbackDirection * knockbackForce);
                }
            }
        }
        else if (attackType == AttackType.Ranged)
        {
            // ATAQUE À DISTÂNCIA
            if (projectilePrefab != null)
            {
                Debug.Log("Disparando projétil!");
                GameObject projectile = Instantiate(projectilePrefab, eyes.position, Quaternion.identity);
                Vector2 fireDirection = (playerTarget.position - eyes.position).normalized;
                projectile.GetComponent<Rigidbody2D>().linearVelocity = fireDirection * projectileSpeed;
            }
        }

        yield return new WaitForSeconds(0.5f); // "Recuperação" do ataque
        isExecutingAction = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private bool CanSeePlayer()
    {
        if (playerTarget == null || eyes == null) return false;
        if (Vector2.Distance(eyes.position, playerTarget.position) > visionRange) return false;
        Vector2 directionToPlayer = (playerTarget.position - eyes.position).normalized;
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        if (Vector2.Angle(forward, directionToPlayer) > visionAngle / 2f) return false;
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToPlayer, visionRange, visionBlockers);
        return hit.collider != null && ((1 << hit.collider.gameObject.layer) & playerLayer) != 0;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        if (isExecutingAction) return;
        if ((targetPosition.x > transform.position.x && !isFacingRight) || (targetPosition.x < transform.position.x && isFacingRight)) Flip();
    }

    private bool IsPlayerInAttackRange()
    {
        if (playerTarget == null) return false;
        return Vector2.Distance(transform.position, playerTarget.position) <= attackRange;
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }
    #endregion
    #region Gizmos de Percepção
    void OnDrawGizmosSelected()
    {
        if (eyes == null) return;

        // Desenha o Cone de Visão
        Gizmos.color = Color.yellow;
        Vector3 forward = (Application.isPlaying ? isFacingRight : transform.localScale.x > 0) ? Vector3.right : Vector3.left;
        Vector3 up = Quaternion.Euler(0, 0, visionAngle / 2) * forward;
        Vector3 down = Quaternion.Euler(0, 0, -visionAngle / 2) * forward;

        Gizmos.DrawLine(eyes.position, eyes.position + up * visionRange);
        Gizmos.DrawLine(eyes.position, eyes.position + down * visionRange);

        // Se estiver vendo o jogador, desenha uma linha vermelha até ele.
        if (Application.isPlaying && CanSeePlayer())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyes.position, playerTarget.position);
        }
    }
    #endregion
}