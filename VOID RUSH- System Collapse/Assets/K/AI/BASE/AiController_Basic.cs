using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AIMotor_Basic))]
public class AIController_Basic : MonoBehaviour
{
    #region Enums e Referências
    // NOVO ESTADO "ANALYZING" PARA PATRULHA INTELIGENTE
    private enum State { Patrolling, Analyzing, Hunting, Searching, Attacking, Dead }
    private enum AttackType { Melee, Ranged }
    private State currentState;
    private AIMotor_Basic motor;
    #endregion

    #region Parâmetros de Comportamento
    [Header("▶ STATUS E COMBATE")]
    public float maxHealth = 100f;
    public float patrolSpeed = 3f;
    public float combatSpeed = 5f;
    public float attackKnockbackPower = 5f;
    public float knockbackResistance = 2f;

    [Header("▶ CONFIGURAÇÃO DE ATAQUE")]
    [SerializeField] private AttackType attackType = AttackType.Melee;
    // NOVO CAMPO PARA O PONTO DE ATAQUE
    [Tooltip("O ponto de onde os ataques (melee e ranged) se originarão.")]
    public Transform attackPoint;
    public float attackDamage = 15f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    [Header("▶ PERCEPÇÃO INTELIGENTE")]
    public Transform playerTarget;
    public Transform eyes;
    public float visionRange = 15f;
    [Range(0, 180)] public float visionAngle = 90f;
    public float memoryDuration = 5f;
    // NOVO CAMPO PARA A PAUSA NA PATRULHA
    [Tooltip("Quanto tempo (em segundos) a IA para para 'analisar' um obstáculo antes de virar.")]
    public float patrolPauseDuration = 1.5f;
    [Tooltip("O ângulo (em graus) que os olhos escaneiam para cima e para baixo durante a análise.")]
    public float patrolScanAngle = 45f; // <<< LINHA FALTANDO
    [Tooltip("A velocidade da rotação dos olhos durante o escaneamento.")]
    public float patrolScanSpeed = 2f; // <<< LINHA FALTANDO
    public LayerMask playerLayer;
    public LayerMask visionBlockers;
    #endregion

    #region Variáveis Internas
    private float currentHealth;
    private bool isFacingRight = true;
    private bool canAttack = true;
    private bool isInvincible = false;
    private bool isExecutingAction = false;
    private bool isTakingKnockback = false;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        motor = GetComponent<AIMotor_Basic>();
    }

    void Start()
    {
        // Fallback para o attackPoint, caso não seja definido
        if (attackPoint == null)
        {
            Debug.LogWarning($"'attackPoint' não foi definido para '{gameObject.name}'. Usando 'eyes' como fallback.");
            attackPoint = eyes;
        }

        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerTarget = playerObject.transform;
            else { Debug.LogError($"CRÍTICO: Inimigo '{gameObject.name}' não encontrou o jogador. IA desativada."); this.enabled = false; return; }
        }

        isFacingRight = transform.localScale.x > 0;
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
        currentHealth = maxHealth;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (currentState == State.Dead || isTakingKnockback || isExecutingAction) return;

        bool canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= attackRange) ChangeState(State.Attacking);
            else ChangeState(State.Hunting);
        }
        else
        {
            if (currentState == State.Hunting || currentState == State.Attacking) ChangeState(State.Searching);
            else if (currentState == State.Searching)
            {
                searchTimer -= Time.deltaTime;
                if (searchTimer <= 0) ChangeState(State.Patrolling);
            }
        }
        ExecuteCurrentStateBehavior();
    }
    #endregion

    #region Lógica dos Estados
    private void ExecuteCurrentStateBehavior()
    {
        switch (currentState)
        {
            case State.Patrolling:
                // Se encontrar um obstáculo E não estiver já "pensando", inicia a análise.
                if (motor.IsObstacleAhead() || !motor.IsGroundAhead())
                {
                    StartCoroutine(AnalyzeObstacleRoutine());
                }
                else
                {
                    motor.Move(isFacingRight ? 1 : -1, patrolSpeed);
                }
                break;

            case State.Analyzing:
                // Enquanto analisa, a IA fica parada. A corrotina tem o controle.
                motor.Stop();
                break;

            case State.Hunting:
                FaceTarget(playerTarget.position);
                if (motor.IsObstacleAhead()) motor.Stop();
                else motor.Move(isFacingRight ? 1 : -1, combatSpeed);
                break;
            case State.Searching:
                FaceTarget(lastKnownPlayerPosition);
                if (Vector2.Distance(transform.position, lastKnownPlayerPosition) < 1f) motor.Stop();
                else motor.Move(isFacingRight ? 1 : -1, combatSpeed);
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
    // (Esta seção não precisou de mudanças)
    public void TakeDamage(float amount, Vector2 attackDirection, float incomingKnockbackPower)
    {
        if (isInvincible || currentState == State.Dead) return;
        currentHealth -= amount;
        float finalForce = incomingKnockbackPower - knockbackResistance;
        if (finalForce > 0 && !isTakingKnockback)
        {
            motor.ExecuteKnockback(finalForce, attackDirection);
            StartCoroutine(KnockbackStateCoroutine());
        }

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(DamageFeedbackCoroutine());
    }
    private void Die()
    {
        ChangeState(State.Dead);
        GetComponent<Collider2D>().enabled = false;
        motor.Stop();
        Destroy(gameObject, 3f);
    }



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



    private IEnumerator KnockbackStateCoroutine()
    {
        isTakingKnockback = true;
        yield return new WaitForSeconds(0.3f);
        isTakingKnockback = false;
    }
    #endregion







    #region Funções de Suporte e Corrotinas

    private IEnumerator AnalyzeObstacleRoutine()
    {
        isExecutingAction = true;
        ChangeState(State.Analyzing);

        // Chama a nova rotina para escanear com os olhos
        yield return StartCoroutine(ScanSurroundingsRoutine());

        Flip();
        isExecutingAction = false;
        ChangeState(State.Patrolling);
    }
    private bool CanSeePlayer()
    {
        if (playerTarget == null || eyes == null) return false;

        float distanceToPlayer = Vector2.Distance(eyes.position, playerTarget.position);

        // 1. O jogador está longe demais? Se sim, impossível ver.
        if (distanceToPlayer > visionRange) return false;

        Vector2 directionToPlayer = (playerTarget.position - eyes.position).normalized;

        // 2. A VISÃO ESTÁ BLOQUEADA? Lança um raio para checar por paredes/chão.
        // Se o raio acertar qualquer coisa na layer 'visionBlockers', a visão está bloqueada.
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToPlayer, distanceToPlayer, visionBlockers);
        if (hit.collider != null)
        {
            // Opcional: Adicione um Debug.DrawLine para ver o raio de visão sendo bloqueado!
            // Debug.DrawLine(eyes.position, hit.point, Color.magenta);
            return false;
        }

        // 3. O JOGADOR ESTÁ NO ÂNGULO DE VISÃO?
        // Só checa isso se a visão NÃO estiver bloqueada.
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        if (Vector2.Angle(forward, directionToPlayer) > visionAngle / 2f) return false;

        // Se passou por todas as verificações, então a IA pode ver o jogador.
        return true;
    }

    // (O resto das funções não precisou de mudanças)



    private IEnumerator ScanSurroundingsRoutine()
    {
        Quaternion initialRotation = eyes.localRotation;
        Quaternion upRotation = Quaternion.Euler(0, 0, patrolScanAngle);
        Quaternion downRotation = Quaternion.Euler(0, 0, -patrolScanAngle);

        // Usa um terço do tempo total de pausa para cada movimento
        float segmentDuration = patrolPauseDuration / 3f;

        // Olha para cima
        float timer = 0f;
        while (timer < segmentDuration)
        {
            eyes.localRotation = Quaternion.Slerp(initialRotation, upRotation, timer / segmentDuration);
            timer += Time.deltaTime * patrolScanSpeed;
            yield return null;
        }

        // Olha para baixo
        timer = 0f;
        while (timer < segmentDuration)
        {
            eyes.localRotation = Quaternion.Slerp(upRotation, downRotation, timer / segmentDuration);
            timer += Time.deltaTime * patrolScanSpeed;
            yield return null;
        }

        // Volta ao normal
        timer = 0f;
        while (timer < segmentDuration)
        {
            eyes.localRotation = Quaternion.Slerp(downRotation, initialRotation, timer / segmentDuration);
            timer += Time.deltaTime * patrolScanSpeed;
            yield return null;
        }
        eyes.localRotation = initialRotation;
    }

    private IEnumerator AttackCoroutine()
    {
        canAttack = false;
        isExecutingAction = true;
        yield return new WaitForSeconds(0.5f);

        if (attackType == AttackType.Melee)
        {
            Collider2D[] targetsHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
            foreach (Collider2D target in targetsHit)
            {
                if (target.TryGetComponent<PlayerStats>(out PlayerStats player))
                {
                    Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                    player.TakeDamage(attackDamage, knockbackDirection, attackKnockbackPower);
                }
            }
        }
        else if (attackType == AttackType.Ranged)
        {
            if (projectilePrefab != null && playerTarget != null)
            {
                Vector2 fireDirection = (playerTarget.position - attackPoint.position).normalized;
                float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
                Quaternion projectileRotation = Quaternion.Euler(0, 0, angle);

                // CRIA E APLICA VELOCIDADE
                GameObject projectile = Instantiate(projectilePrefab, attackPoint.position, projectileRotation);
                if (projectile.TryGetComponent<Rigidbody2D>(out var projRb))
                {
                    projRb.linearVelocity = fireDirection * projectileSpeed;
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        isExecutingAction = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
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
        if ((targetPosition.x > transform.position.x && !isFacingRight) || (targetPosition.x < transform.position.x && isFacingRight))
            Flip();
    }
    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        if (currentState == State.Hunting || currentState == State.Attacking)
        {
            if (playerTarget != null)
                lastKnownPlayerPosition = playerTarget.position;
        }
        currentState = newState;
        if (currentState == State.Searching)
        {
            searchTimer = memoryDuration;
        }
    }
    #endregion

    #region Gizmos de Percepção
    void OnDrawGizmosSelected()
    {
        if (eyes == null) return;

        Vector3 attackOrigin = (attackPoint != null) ? attackPoint.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin, attackRange);

        Vector3 forward = (Application.isPlaying ? isFacingRight : transform.localScale.x > 0) ? Vector3.right : Vector3.left;

        // Desenha o leque de visão com Raycasts
        float stepCount = 20; // Mais steps = cone mais suave
        float stepAngle = visionAngle / stepCount;

        for (float angle = -visionAngle / 2; angle <= visionAngle / 2; angle += stepAngle)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * forward;

            RaycastHit2D hit = Physics2D.Raycast(eyes.position, direction, visionRange, visionBlockers);

            Vector3 endPoint = eyes.position + direction * visionRange;
            if (hit.collider != null)
            {
                endPoint = hit.point;
            }
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eyes.position, endPoint);
        }

        if (Application.isPlaying && currentState == State.Searching)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 1f);
        }
    }
    #endregion
}