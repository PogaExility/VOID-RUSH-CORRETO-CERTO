// --- FILE: AIController.cs (Compila��o Corrigida) ---
using UnityEngine;
using System.Collections;

// Vers�o CORRIGIDA: Resolve o loop de pulo, a caminhada infinita, melhora a percep��o da cabe�a e restaura as vari�veis de mem�ria.
[RequireComponent(typeof(AIPlatformerMotor))]
public class AIController : MonoBehaviour
{
    private enum PossibleLedgeAction { None, Descend, Jump, Retreat }
    #region State Machine and References
    private enum State { Patrolling, Hunting, Attacking, Searching, Analyzing }
    private State currentState;
    private AIPlatformerMotor motor;
    private Transform playerTarget;
    public Transform eyes;
    #endregion

    #region Vision and Memory Parameters
    [Header("Par�metros de Vis�o")]
    public float visionRange = 15f;
    [Range(0, 180)] public float visionAngle = 90f;
    public LayerMask visionBlockers;
    public LayerMask playerLayer;
    private Vector2 lookDirection;
    public float lookSpeed = 4f;
    [Range(0f, 1f)]
    private float retreatProbability = 0.5f; // Come�a em 50%

    [Header("Consci�ncia Situacional")]
    [Range(0, 90)] public float upwardLookAngle = 45f;
    [Range(0, 90)] public float downwardLookAngle = 45f;
    public float platformCheckHeight = 5f;
    private float patrolLookScanSpeed = 1.0f;

    // --- CORRE��O: VARI�VEIS RESTAURADAS ---
    [Header("Mem�ria e Busca")]
    public float searchTime = 5f;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;
    private Vector3? lastSeenSafePlatform = null;
    public float ledgeAnalysisTime = 1.5f;
    // ------------------------------------

    [Header("Personalidade Ocular")]
    [Tooltip("A velocidade geral com que a cabe�a/olhos se movem para um novo alvo.")]
    public float eyeSpeed = 6f;
    [Tooltip("A velocidade da oscila��o dos olhos no modo de patrulha.")]
    public float patrolTwitchSpeed = 0.7f;
    [Tooltip("A amplitude (qu�o longe) da oscila��o dos olhos no modo de patrulha.")]
    public float patrolTwitchAmplitude = 0.15f;
    [Tooltip("A intensidade do tremor dos olhos ao analisar um alvo.")]
    public float analysisTwitchIntensity = 0.1f;
    #endregion

    #region Combat Parameters
    [Header("Combate Corpo a Corpo")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;
    #endregion

    #region Navigation Parameters
    [Header("Navega��o")]
    public float jumpForce = 14f;
    public float airControlForce = 5f;
    public float maxJumpDistance = 5f;
    private bool isFacingRight = true;
    private bool isExecutingAction = false;
    [Tooltip("A altura m�xima que a IA considera segura para descer de uma plataforma.")]
    public float maxSafeDropHeight = 4f;
    [Tooltip("A altura vertical m�xima que a IA consegue alcan�ar com um pulo.")]
    public float maxJumpHeight = 3f;
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
            StartCoroutine(ExecuteLedgeAnalysis(false)); // Chama a nova rotina
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

    private void HandleHuntingMovement()
    {
        FaceTarget(playerTarget.position);
        float direction = isFacingRight ? 1 : -1;

        if (motor.IsLedgeAhead())
        {
            StartCoroutine(ExecuteLedgeAnalysis(true)); // Chama a nova rotina
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
    private IEnumerator ExecuteLedgeAnalysis(bool isAggressive)
    {
        ChangeState(State.Analyzing);
        isExecutingAction = true;

        // --- FASE 1: COLETA DE INTELIG�NCIA ---
        // A IA primeiro re�ne todos os fatos antes de tomar qualquer decis�o.

        // 1.1: Existe um caminho para baixo?
        yield return StartCoroutine(LookDownAndMeasureDepth());
        bool canSafelyDrop = lastSeenSafePlatform.HasValue;
        Vector3? dropTarget = lastSeenSafePlatform; // Salva o alvo de descida

        // 1.2: Existe um caminho para frente/cima?
        yield return StartCoroutine(LookForwardAndFindJumpTarget());
        bool canSafelyJump = lastSeenSafePlatform.HasValue;
        Vector3? jumpTarget = lastSeenSafePlatform; // Salva o alvo de pulo

        // --- FASE 2: DELIBERA��O E DECIS�O ---
        // Com todos os fatos, a IA agora pondera suas op��es.

        PossibleLedgeAction chosenAction = PossibleLedgeAction.None;

        // Constr�i a lista de op��es t�ticas
        var availableActions = new System.Collections.Generic.List<PossibleLedgeAction>();
        if (canSafelyDrop) availableActions.Add(PossibleLedgeAction.Descend);
        if (canSafelyJump) availableActions.Add(PossibleLedgeAction.Jump);

        // Se n�o houver op��es agressivas, a �nica escolha � recuar.
        if (availableActions.Count == 0 || !isAggressive)
        {
            chosenAction = PossibleLedgeAction.Retreat;
        }
        else // Se houver op��es, use a probabilidade para decidir.
        {
            if (Random.value < retreatProbability)
            {
                chosenAction = PossibleLedgeAction.Retreat;
            }
            else // Se n�o for recuar, escolha a melhor op��o agressiva (prioriza descer)
            {
                chosenAction = availableActions.Contains(PossibleLedgeAction.Descend)
                    ? PossibleLedgeAction.Descend
                    : PossibleLedgeAction.Jump;
            }
        }

        // --- FASE 3: EXECU��O DA A��O ESCOLHIDA ---

        switch (chosenAction)
        {
            case PossibleLedgeAction.Descend:
                Debug.Log($"An�lise: DECIDIU DESCER. (Chance de recuo era {retreatProbability * 100}%)");
                retreatProbability += 0.05f; // Aumenta a chance de recuar da pr�xima vez
                float stepOffTimer = 0.5f;
                while (motor.IsGrounded() && stepOffTimer > 0)
                {
                    motor.Move((isFacingRight ? 1 : -1) * 0.5f);
                    stepOffTimer -= Time.deltaTime;
                    yield return null;
                }
                break;

            case PossibleLedgeAction.Jump:
                Debug.Log($"An�lise: DECIDIU PULAR. (Chance de recuo era {retreatProbability * 100}%)");
                retreatProbability += 0.05f; // Aumenta a chance de recuar da pr�xima vez
                yield return StartCoroutine(PonderTargetWithTwitchingEyes(jumpTarget.Value));
                yield return StartCoroutine(JumpRoutine(isFacingRight ? 1 : -1));
                break;

            case PossibleLedgeAction.Retreat:
                Debug.Log($"An�lise: DECIDIU RECUAR. (Chance de recuo era {retreatProbability * 100}%)");
                retreatProbability -= 0.05f; // Diminui a chance de recuar da pr�xima vez
                yield return StartCoroutine(LookAnRetreatRoutine());
                break;
        }

        retreatProbability = Mathf.Clamp(retreatProbability, 0.05f, 0.95f);
        isExecutingAction = false;
        ChangeState(isAggressive ? State.Hunting : State.Patrolling);
    }

    // NOVO M�TODO HELPER PARA VALIDAR O PULO
    private bool IsJumpTargetValid(Vector3 target)
    {
        // O alvo � muito alto para pular?
        if (target.y - transform.position.y > maxJumpHeight) return false;

        // O alvo � abaixo de n�s? (N�o queremos pular para baixo)
        if (target.y < motor.groundCheck_A.position.y) return false;

        // O alvo tem uma superf�cie plana para pousar?
        // (Verifica se h� espa�o para os p�s)
        RaycastHit2D surfaceCheck = Physics2D.Raycast(target + Vector3.up * 0.1f, Vector2.down, 0.5f, motor.groundLayer);
        if (surfaceCheck.collider == null) return false;

        // Se passou em todos os testes, o alvo � v�lido.
        return true;
    }

    private IEnumerator LookAnRetreatRoutine()
    {
        isExecutingAction = true;

        // ETAPA 1: Olhar para frente primeiro.
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        float timer = 0f;
        while (timer < 0.3f) // Gasta um tempo virando a cabe�a
        {
            lookDirection = Vector2.Lerp(lookDirection, forward, Time.deltaTime * lookSpeed * 2);
            timer += Time.deltaTime;
            yield return null;
        }
        lookDirection = forward; // Garante que olhou para frente

        yield return new WaitForSeconds(0.2f); // Pausa para "pensar"

        // ETAPA 2: Virar o corpo.
        Flip();

        yield return new WaitForSeconds(0.5f); // Pausa ap�s virar antes de se mover.

        isExecutingAction = false; // Libera o controle para o ExecuteBrain
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

        // --- CORRE��O FUNDAMENTAL ---
        // Inverte a dire��o do olhar INSTANTANEAMENTE junto com o corpo.
        lookDirection.x *= -1;
    }

    // SUBSTITUA O M�TODO FaceTarget() INTEIRO POR ESTE:

    private void FaceTarget(Vector3 targetPosition)
    {
        if (isExecutingAction) return;
        if (targetPosition.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        else if (targetPosition.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
    }
    #endregion

    #region Perception Helpers (Vision, etc.)
    void HandleHeadLook()
    {
        // Se estamos analisando, a rotina de an�lise tem controle total sobre a cabe�a.
        if (currentState == State.Analyzing) return;

        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 targetLookDirection = forward;

        if (currentState == State.Hunting || currentState == State.Attacking)
        {
            targetLookDirection = (playerTarget.position - eyes.position).normalized;
        }
        else if (currentState == State.Searching)
        {
            targetLookDirection = (lastKnownPlayerPosition - eyes.position).normalized;
        }
        else
        {
            // --- NOVO: COMPORTAMENTO OCULAR NATURAL ---
            // Adiciona uma oscila��o vertical sutil e lenta � dire��o do olhar.
            float verticalOscillation = Mathf.Sin(Time.time * patrolTwitchSpeed) * patrolTwitchAmplitude;
            targetLookDirection = (forward + new Vector2(0, verticalOscillation)).normalized;
        }

        // Garante que a cabe�a n�o vire para tr�s
        if (Vector2.Dot(targetLookDirection, forward) < 0)
        {
            targetLookDirection = forward;
        }

        // Interpola suavemente a dire��o do olhar
        lookDirection = Vector2.Lerp(lookDirection, targetLookDirection, Time.deltaTime * eyeSpeed);
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
    private IEnumerator LookDownAndMeasureDepth()
    {
        lastSeenSafePlatform = null; // Reseta a mem�ria para esta verifica��o

        // Anima��o de olhar para baixo
        float lookTimer = 0f;
        Vector2 startLookDir = lookDirection;
        Vector2 targetLookDir = ((isFacingRight ? Vector2.right : Vector2.left) + Vector2.down).normalized;
        while (lookTimer < 0.5f)
        {
            lookDirection = Vector2.Lerp(startLookDir, targetLookDir, lookTimer / 0.5f);
            lookTimer += Time.deltaTime;
            yield return null;
        }

        // Pausa com "twitching"
        float twitchTimer = 0f;
        while (twitchTimer < 0.7f)
        {
            Vector2 twitchOffset = Random.insideUnitCircle * analysisTwitchIntensity;
            lookDirection = Vector2.Lerp(lookDirection, targetLookDir + twitchOffset, Time.deltaTime * eyeSpeed);
            twitchTimer += Time.deltaTime;
            yield return null;
        }

        // A medi��o real
        RaycastHit2D hit = Physics2D.Raycast(motor.ledgeCheck.position, Vector2.down, maxSafeDropHeight, motor.groundLayer);
        if (hit.collider != null)
        {
            lastSeenSafePlatform = hit.point;
        }
    }

    private IEnumerator LookForwardAndFindJumpTarget()
    {
        lastSeenSafePlatform = null; // Reseta a mem�ria para esta verifica��o

        // Anima��o de olhar de baixo para cima
        float lookTimer = 0f;
        Vector2 startLookDir = lookDirection;
        while (lookTimer < 0.8f)
        {
            float scanProgress = lookTimer / 0.8f;
            float currentAngle = Mathf.Lerp(-45, upwardLookAngle, scanProgress); // Come�a olhando um pouco pra baixo
            Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
            Vector2 scanDirection = Quaternion.Euler(0, 0, currentAngle * (isFacingRight ? 1 : -1)) * forward;
            lookDirection = Vector2.Lerp(startLookDir, scanDirection, scanProgress);

            RaycastHit2D hit = Physics2D.Raycast(eyes.position, scanDirection, maxJumpDistance, motor.groundLayer);
            if (hit.collider != null && IsJumpTargetValid(hit.point))
            {
                lastSeenSafePlatform = hit.point;
            }
            lookTimer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator PonderTargetWithTwitchingEyes(Vector3 target)
    {
        float ponderTimer = 0f;
        Vector2 directionToTarget = (target - eyes.position).normalized;

        while (ponderTimer < 1.0f) // Tempo total de pondera��o
        {
            Vector2 twitchOffset = Random.insideUnitCircle * (analysisTwitchIntensity * 1.5f); // A intensidade do twitch
            lookDirection = Vector2.Lerp(lookDirection, directionToTarget + twitchOffset, Time.deltaTime * eyeSpeed * 2f);
            ponderTimer += Time.deltaTime;
            yield return null;
        }
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
        Debug.Log(gameObject.name + " est� ATACANDO o jogador de perto!");
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