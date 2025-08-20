// --- FILE: AIController.cs (Compilação Corrigida) ---
using UnityEngine;
using System.Collections;

// Versão CORRIGIDA: Resolve o loop de pulo, a caminhada infinita, melhora a percepção da cabeça e restaura as variáveis de memória.
[RequireComponent(typeof(AIPlatformerMotor))]
public class AIController : MonoBehaviour
{
    #region State Machine and References
    private enum State { Patrolling, Hunting, Attacking, Searching, Analyzing }
    private State currentState;
    private AIPlatformerMotor motor;
    private Transform playerTarget;
    public Transform eyes;
    #endregion

    #region Vision and Memory Parameters
    [Header("Parâmetros de Visão")]
    public float visionRange = 15f;
    [Range(0, 180)] public float visionAngle = 90f;
    public LayerMask visionBlockers;
    public LayerMask playerLayer;
    private Vector2 lookDirection;
    public float lookSpeed = 4f;

    [Header("Consciência Situacional")]
    [Range(0, 90)] public float upwardLookAngle = 45f;
    [Range(0, 90)] public float downwardLookAngle = 45f;
    public float platformCheckHeight = 5f;
    private float patrolLookScanSpeed = 1.0f;

    // --- CORREÇÃO: VARIÁVEIS RESTAURADAS ---
    [Header("Memória e Busca")]
    public float searchTime = 5f;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;
    private Vector3? lastSeenSafePlatform = null;
    public float ledgeAnalysisTime = 1.5f;
    // ------------------------------------

    #endregion

    #region Combat Parameters
    [Header("Combate Corpo a Corpo")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;
    #endregion

    #region Navigation Parameters
    [Header("Navegação")]
    public float jumpForce = 14f;
    public float airControlForce = 5f;
    public float maxJumpDistance = 5f;
    private bool isFacingRight = true;
    private bool isExecutingAction = false;
    [Tooltip("A altura máxima que a IA considera segura para descer de uma plataforma.")]
    public float maxSafeDropHeight = 4f;
    #endregion

    #region Unity Lifecycle Methods
    void Awake() { motor = GetComponent<AIPlatformerMotor>(); }

    void Start()
    {
        playerTarget = AIManager.Instance?.playerTarget;
        isFacingRight = transform.localScale.x > 0;
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
        lookDirection = isFacingRight ? Vector2.right : Vector2.left;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (playerTarget == null) { FindPlayer(); return; }
        HandleHeadLook();
        DecideState();
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;
        ExecuteBrain();
    }
    #endregion

    #region Core AI Logic
    void DecideState()
    {
        // Adicione ou modifique a primeira linha para isto:
        if (isExecutingAction || currentState == State.Analyzing) return;

        bool canSeePlayer = CanSeePlayer();
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = playerTarget.position;
            if (currentState != State.Hunting && currentState != State.Attacking) ChangeState(State.Hunting);
        }

        switch (currentState)
        {
            case State.Hunting:
                if (canSeePlayer) { if (IsPlayerInAttackRange()) ChangeState(State.Attacking); }
                else { ChangeState(State.Searching); }
                break;
            case State.Attacking:
                if (canSeePlayer) { if (!IsPlayerInAttackRange()) ChangeState(State.Hunting); }
                else { ChangeState(State.Searching); }
                break;
            case State.Searching:
                if (Vector2.Distance(transform.position, lastKnownPlayerPosition) < 1f)
                {
                    searchTimer -= Time.deltaTime;
                    if (searchTimer <= 0) ChangeState(State.Patrolling);
                }
                break;
        }
    }

    void ExecuteBrain()
    {
        if (isExecutingAction) { motor.Stop(); return; }

        if (!motor.IsGrounded())
        {
            float airMove = 0f;
            if (currentState == State.Hunting || currentState == State.Searching)
            {
                airMove = Mathf.Sign(playerTarget.position.x - transform.position.x);
            }
            motor.Move(airMove * 0.5f);
            return;
        }

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrolMovement();
                break;
            case State.Hunting:
                HandleHuntingMovement();
                break;
            case State.Searching:
                HandleSearchingMovement();
                break;
            case State.Attacking:
                if (canAttack) StartCoroutine(AttackCoroutine());
                break;
        }
    }

    private void HandlePatrolMovement()
    {
        if (motor.IsLedgeAhead())
        {
            StartCoroutine(AnalyzeLedgeRoutine(false)); // Analisa e vira se não achar nada
        }
        else if (motor.IsObstacleAhead())
        {
            StartCoroutine(PatrolTurnRoutine());
        }
        else
        {
            motor.Move(isFacingRight ? 1 : -1);
        }
    }
    private IEnumerator AnalyzeAndDropRoutine()
    {
        // ETAPA 2: PARAR
        ChangeState(State.Analyzing);
        isExecutingAction = true;
        lastSeenSafePlatform = null; // Reseta a memória

        // ETAPA 3: OLHAR PARA BAIXO
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 targetLook = (forward + Vector2.down * 2f).normalized; // Força o olhar bem para baixo
        float lookTime = 0f;
        while (lookTime < 0.75f) // Gasta um tempo olhando
        {
            lookDirection = Vector2.Lerp(lookDirection, targetLook, Time.deltaTime * lookSpeed);
            lookTime += Time.deltaTime;
            yield return null;
        }

        // ETAPA 4 & 5: ANALISAR E SALVAR NA MEMÓRIA
        Debug.Log("Analisando se a queda é segura...");
        RaycastHit2D hit = Physics2D.Raycast(motor.ledgeCheck.position, Vector2.down, maxSafeDropHeight, motor.groundLayer);

        if (hit.collider != null)
        {
            // A queda é segura!
            lastSeenSafePlatform = hit.point;
            Debug.Log($"Queda segura detectada. Chão encontrado em {hit.point}");

            // ETAPA 6: DESCER
            // A IA vai dar um pequeno passo para fora da borda para começar a cair.
            float stepOffTimer = 0.5f; // Tempo máximo para sair da plataforma
            while (motor.IsGrounded() && stepOffTimer > 0)
            {
                motor.Move((isFacingRight ? 1 : -1) * 0.5f); // Move-se lentamente para fora da borda
                stepOffTimer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // A queda é perigosa ou não há chão visível.
            Debug.Log("Queda perigosa. Abortando e virando.");
            Flip();
            yield return new WaitForSeconds(1f); // Pausa para "repensar" a estratégia
        }

        isExecutingAction = false;
        ChangeState(State.Hunting); // Sempre volta a caçar após a ação
    }
    private void HandleHuntingMovement()
    {
        FaceTarget(playerTarget.position);
        float direction = isFacingRight ? 1 : -1;

        if (motor.IsLedgeAhead())
        {
            // --- NOVA LÓGICA DE DECISÃO ---
            bool isPlayerBelow = playerTarget.position.y < eyes.position.y;
            bool isPlayerCloseHorizontally = Mathf.Abs(playerTarget.position.x - transform.position.x) < 2.5f;

            if (isPlayerBelow && isPlayerCloseHorizontally)
            {
                // O jogador está abaixo e perto. A IA deve considerar descer.
                StartCoroutine(AnalyzeAndDropRoutine());
            }
            else
            {
                // O jogador está longe ou em outra plataforma. A IA deve considerar pular.
                StartCoroutine(AnalyzeLedgeRoutine(true));
            }
        }
        else if (motor.IsObstacleAhead())
        {
            if (CanJumpOverWall())
            {
                StartCoroutine(JumpRoutine(direction));
            }
        }
        else
        {
            motor.Move(direction);
        }
    }

    private void HandleSearchingMovement()
    {
        FaceTarget(lastKnownPlayerPosition);
        if (Vector2.Distance(transform.position, lastKnownPlayerPosition) > 1.5f)
        {
            motor.Move(isFacingRight ? 1 : -1);
        }
    }
    #endregion

    #region Movement and Navigation Helpers
    void FindPlayer() { playerTarget = AIManager.Instance?.playerTarget; }

    void FaceDirection(float direction)
    {
        if (isExecutingAction) return;
        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        if (isExecutingAction) return;
        if (targetPosition.x > transform.position.x && !isFacingRight) Flip();
        else if (targetPosition.x < transform.position.x && isFacingRight) Flip();
    }
    #endregion

    #region Perception Helpers (Vision, etc.)

    private IEnumerator AnalyzeLedgeRoutine(bool isAggressive)
    {
        ChangeState(State.Analyzing);
        isExecutingAction = true;
        lastSeenSafePlatform = null; // Reseta a memória

        float timeSpent = 0f;
        while (timeSpent < ledgeAnalysisTime)
        {
            // Movimento de varredura da cabeça
            float scanProgress = timeSpent / ledgeAnalysisTime;
            float currentAngle = Mathf.Lerp(0, -downwardLookAngle, scanProgress);
            Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
            lookDirection = Quaternion.Euler(0, 0, currentAngle * (isFacingRight ? 1 : -1)) * forward;

            // Lança um raio para procurar chão
            RaycastHit2D hit = Physics2D.Raycast(eyes.position, lookDirection, maxJumpDistance, motor.groundLayer);
            if (hit.collider != null)
            {
                lastSeenSafePlatform = hit.point;
                // Se estiver caçando e vir o jogador perto da plataforma, confirma o pulo
                if (isAggressive && Vector2.Distance(lastSeenSafePlatform.Value, playerTarget.position) < 3f)
                {
                    break; // Encontrou um bom alvo, para de analisar
                }
            }

            timeSpent += Time.deltaTime;
            yield return null;
        }

        // Fim da análise, hora de decidir
        if (lastSeenSafePlatform.HasValue)
        {
            Debug.Log("Análise concluída: Ponto seguro encontrado. Pulando.");
            StartCoroutine(JumpRoutine(isFacingRight ? 1 : -1));
        }
        else
        {
            Debug.Log("Análise concluída: Nenhum ponto seguro. Virando.");
            Flip();
            yield return new WaitForSeconds(0.5f); // Pausa após virar
        }

        isExecutingAction = false;
        // Volta para o estado anterior (ou um estado padrão)
        ChangeState(isAggressive ? State.Hunting : State.Patrolling);
    }
    void HandleHeadLook()
    {
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 targetLookDirection = forward;

        // Durante a análise, a corrotina tem controle total sobre a cabeça.
        if (currentState == State.Analyzing) return;

        if (currentState == State.Hunting || currentState == State.Attacking)
        {
            targetLookDirection = (playerTarget.position - eyes.position).normalized;
        }
        else if (currentState == State.Searching)
        {
            targetLookDirection = (lastKnownPlayerPosition - eyes.position).normalized;
        }
        else if (currentState == State.Patrolling)
        {
            // --- CORREÇÃO "Olhar para dentro da parede" ---
            if (motor.IsObstacleAhead() && motor.IsGrounded())
            {
                // Força um olhar para cima de forma mais direta
                targetLookDirection = (forward + Vector2.up).normalized;
            }
            else if (motor.IsLedgeAhead() && motor.IsGrounded())
            {
                // A análise cuidará do olhar detalhado, aqui apenas uma olhadinha
                targetLookDirection = (forward + Vector2.down).normalized;
            }
            else
            {
                targetLookDirection = forward;
            }
        }

        if (Vector2.Dot(targetLookDirection, forward) < 0) { targetLookDirection = forward; }
        lookDirection = Vector2.Lerp(lookDirection, targetLookDirection, Time.deltaTime * lookSpeed);
    }

    private bool CanSeePlayer()
    {
        if (playerTarget == null || eyes == null) return false;
        if (((1 << playerTarget.gameObject.layer) & playerLayer) == 0) return false;
        float distanceToPlayer = Vector3.Distance(eyes.position, playerTarget.position);
        if (distanceToPlayer > visionRange) return false;
        Vector3 directionToPlayer = (playerTarget.position - eyes.position).normalized;
        if (Vector2.Angle(lookDirection, directionToPlayer) > visionAngle / 2f) return false;
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToPlayer, distanceToPlayer, visionBlockers);
        return hit.collider == null || hit.transform == playerTarget;
    }

    private bool CanJumpOverWall() { return !Physics2D.Raycast(eyes.position, Vector2.up, 3f, motor.groundLayer); }
    private bool CanMakeTheJump() { RaycastHit2D hit = Physics2D.Raycast(motor.ledgeCheck.position, Vector2.right * motor.currentFacingDirection, maxJumpDistance, motor.groundLayer); return hit.collider != null; }
    private bool IsPlayerInAttackRange() { if (playerTarget == null) return false; return Vector2.Distance(transform.position, playerTarget.position) <= attackRange; }
    #endregion

    #region State Machine and Coroutines
    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        Debug.Log(gameObject.name + " mudando de estado: " + currentState + " -> " + newState);
        currentState = newState;
        if (currentState == State.Searching) { searchTimer = searchTime; }
    }

    IEnumerator PatrolTurnRoutine()
    {
        isExecutingAction = true;
        yield return new WaitForSeconds(0.75f);
        Flip();
        yield return new WaitForSeconds(0.5f);
        isExecutingAction = false;
    }

    private IEnumerator AttackCoroutine()
    {
        isExecutingAction = true;
        canAttack = false;
        Debug.Log(gameObject.name + " está ATACANDO o jogador de perto!");
        yield return new WaitForSeconds(0.5f);
        isExecutingAction = false;
        yield return new WaitForSeconds(attackCooldown - 0.5f);
        canAttack = true;
    }

    private IEnumerator JumpRoutine(float direction)
    {
        isExecutingAction = true;
        motor.AddJumpForce(new Vector2(direction * airControlForce, jumpForce));
        yield return new WaitForSeconds(0.2f);
        while (!motor.IsGrounded())
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        isExecutingAction = false;
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        if (eyes == null) return;
        Vector3 forward = Application.isPlaying ? (Vector3)lookDirection.normalized : (transform.right * (transform.localScale.x > 0 ? 1 : -1));
        Gizmos.color = Color.yellow;
        Vector3 p1 = eyes.position + (Quaternion.Euler(0, 0, visionAngle / 2) * forward) * visionRange;
        Vector3 p2 = eyes.position + (Quaternion.Euler(0, 0, -visionAngle / 2) * forward) * visionRange;
        Gizmos.DrawLine(eyes.position, p1);
        Gizmos.DrawLine(eyes.position, p2);
        if (Application.isPlaying && CanSeePlayer()) { Gizmos.color = Color.red; Gizmos.DrawLine(eyes.position, playerTarget.position); }
        if (lastSeenSafePlatform.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastSeenSafePlatform.Value, 0.5f);
            Gizmos.DrawLine(eyes.position, lastSeenSafePlatform.Value);
        }
    }
    #endregion
}