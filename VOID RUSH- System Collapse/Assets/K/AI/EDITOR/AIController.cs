// --- ARQUIVO FINAL: AIController.cs (Versão com Cérebro Humano e Hiper-Visualização) ---
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AIPlatformerMotor))]
public class AIController : MonoBehaviour
{
    // Enum para tornar a tomada de decisão clara e legível
    private enum PossibleLedgeAction { None, Descend, Jump, Retreat }

    #region State Machine and References
    private enum State { Patrolling, Hunting, Attacking, Searching, Analyzing, Dead, Climbing, LedgeGrabbing }
    private State currentState;
    private AIPlatformerMotor motor;
    private Transform playerTarget;
    public Transform eyes; // A posição real dos olhos será controlada via código
    #endregion

    #region Comportamento e Personalidade (SEU PAINEL DE CONTROLE)

    [Header("Percepção de Navegação (Raycasts)")]
    [Tooltip("O quão à frente da IA o sensor de chão 'sente' o terreno.")]
    public float groundProbeDistance = 0.8f;
    [Tooltip("O ângulo para baixo que o sensor de chão principal usa.")]
    [Range(0, 90)] public float groundProbeAngle = 45f;
    [Tooltip("Se a distância até o chão for maior que este valor, a IA considera uma beirada e para.")]
    public float ledgeDetectionThreshold = 1.2f;
    [Header("Análise e Deliberação")]
    [Tooltip("O tempo que a IA passa olhando para baixo antes de iniciar a varredura.")]
    public float ledgeDwellTime = 0.8f;
    [Tooltip("O tempo que a IA pausa no meio da varredura para cima.")]
    public float ledgeMidScanPauseTime = 0.3f;
    [Tooltip("O tempo total que a IA leva para escanear de baixo para cima.")]
    public float ledgeScanUpDuration = 1.2f;
    public float searchTime = 5f;
    [Header("Personalidade e Probabilidade")]
    [Tooltip("A velocidade geral com que a cabeça/olhos se movem para um novo alvo.")]
    public float eyeSpeed = 6f;
    [Range(0f, 1f)]
    private float retreatProbability = 0.5f; // Começa em 50%

    #endregion

    #region Parâmetros de Navegação e Combate
    [Header("Navegação e Combate")]
    public float visionRange = 15f;
    [Range(0, 180)] public float visionAngle = 90f;
    public LayerMask visionBlockers;
    public LayerMask playerLayer;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float jumpForce = 14f;
    public float airControlForce = 5f;
    public float maxJumpDistance = 5f;
    public float maxJumpHeight = 3f;
    public float maxSafeDropHeight = 4f;
    private bool canAttack = true; // <-- ADICIONE ESTA LINHA
    [Header("Status de Combate")]
    public float maxHealth = 100f;
    private float currentHealth;
    public float attackDamage = 15f;
    public float knockbackForce = 5f;



    [Header("Modo de Comportamento")]
    [Tooltip("Ative para um comportamento simples (andar, atacar). Desative para a IA tática completa.")]
    public bool isBrainedDead = false; // O "INTERRUPTOR DE CÉREBRO"
    [Header("Navegação Vertical")]
    [Tooltip("Ative para permitir que a IA escale paredes.")]
    public bool canClimb = true;
    public float climbSpeed = 3f;
    [Tooltip("A altura máxima que a IA pode pular para se agarrar em uma plataforma.")]
    public float maxLedgeGrabHeight = 4f;
    [Tooltip("O tempo que a IA espera agarrada na beirada antes de subir.")]
    public float ledgeHangTime = 1.0f;
    #endregion

    #region Variáveis Internas (Não Mexer)
    private Vector2 lookDirection;
    private bool isFacingRight = true;
    private bool isExecutingAction = false;
    private RaycastHit2D groundProbeHit;          // <-- ADICIONA A VARIÁVEL FALTANTE
    private Vector3? lastSeenSafePlatform = null; // <-- ADICIONA A VARIÁVEL FALTANTE
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;
    #endregion

    #region Unity Lifecycle Methods
    void Awake() { motor = GetComponent<AIPlatformerMotor>(); }
    void Start()
    {
        playerTarget = AIManager.Instance?.playerTarget;
        isFacingRight = transform.localScale.x > 0;
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
        lookDirection = isFacingRight ? Vector2.right : Vector2.left;
        currentHealth = maxHealth; // Inicializa a vida
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
    private void HandleHuntingMovement()
    {
        FaceTarget(playerTarget.position);
        float direction = isFacingRight ? 1 : -1;

        // A única preocupação deste método agora é PAREDES. A beirada é tratada em DecideState.
        if (motor.IsObstacleAhead())
        {
            // Se pode escalar, entra no estado de escalada.
            if (canClimb)
            {
                ChangeState(State.Climbing);
                return;
            }

            // Se não pode escalar, para.
            motor.Stop();
        }
        else
        {
            // Se não há obstáculos, move-se em direção ao alvo.
            motor.Move(direction);
        }
    }
    void DecideState()
    {
        if (isExecutingAction || currentState == State.Analyzing || currentState == State.Dead) return;

        // --- MODO BÁSICO ---
        if (isBrainedDead)
        {
            if (playerTarget == null) return;

            // No modo básico, a IA só se preocupa com distância.
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer <= attackRange)
            {
                ChangeState(State.Attacking);
            }
            else if (distanceToPlayer <= visionRange)
            {
                ChangeState(State.Hunting);
            }
            else
            {
                ChangeState(State.Patrolling);
            }
            return;
        }

        // --- MODO TÁTICO (sua lógica existente) ---
        // Gatilho de análise por Raycast
        bool shouldAnalyze = !groundProbeHit.collider || groundProbeHit.distance > ledgeDetectionThreshold;
        if (shouldAnalyze && (currentState == State.Patrolling || currentState == State.Hunting))
        {
            StartCoroutine(ExecuteLedgeAnalysis(currentState == State.Hunting));
            return;
        }

        // Lógica de estado tática
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
        if (isExecutingAction || currentState == State.Dead) { motor.Stop(); return; }
        if (!motor.IsGrounded())
        {
            float airMove = 0f;
            if (currentState == State.Hunting || currentState == State.Searching)
            {
                if (playerTarget != null) airMove = Mathf.Sign(playerTarget.position.x - transform.position.x);
            }
            motor.Move(airMove * 0.5f);
            return;
        }

        // --- MODO BÁSICO ---
        if (isBrainedDead)
        {
            if (playerTarget != null) FaceTarget(playerTarget.position);

            switch (currentState)
            {
                case State.Patrolling:
                    // No modo básico, patrulhar significa ficar parado se não vê o jogador.
                    motor.Stop();
                    break;
                case State.Hunting:
                    motor.Move(isFacingRight ? 1 : -1);
                    break;
                case State.Attacking:
                    if (canAttack) StartCoroutine(AttackCoroutine());
                    break;
            }
            return;
        }

        // --- MODO TÁTICO (sua lógica existente) ---
        switch (currentState)
        {
            case State.Patrolling:
                motor.Move(isFacingRight ? 1 : -1);
                break;
            case State.Hunting:
                if (playerTarget != null) FaceTarget(playerTarget.position);
                motor.Move(isFacingRight ? 1 : -1);
                break;
            case State.Searching:
                if (Vector2.Distance(transform.position, lastKnownPlayerPosition) > 1.5f)
                {
                    motor.Move(isFacingRight ? 1 : -1);
                }
                else
                {
                    motor.Stop();
                }
                break;
            case State.Attacking:
                if (canAttack) StartCoroutine(AttackCoroutine());
                break;
            case State.Climbing:
                HandleClimbingMovement();
                break;
        }
    }
    private IEnumerator ExecuteLedgeAnalysis(bool isAggressive)
    {
        ChangeState(State.Analyzing);
        isExecutingAction = true;

        // --- FASE 1: ANIMAÇÃO DE ANÁLISE VISUAL ---
        Vector2 lookDownDir = ((isFacingRight ? Vector2.right : Vector2.left) + Vector2.down).normalized;
        Vector2 lookForwardDir = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 lookUpDir = ((isFacingRight ? Vector2.right : Vector2.left) + Vector2.up).normalized;
        float timer;

        // 1A: OLHAR PARA BAIXO E PONDERAR
        timer = 0f;
        while (timer < ledgeDwellTime)
        {
            lookDirection = Vector2.Lerp(lookDirection, lookDownDir, Time.deltaTime * eyeSpeed);
            timer += Time.deltaTime;
            yield return null;
        }

        // 1B: SUBIR ATÉ O MEIO
        timer = 0f;
        while (timer < ledgeScanUpDuration / 2f)
        {
            lookDirection = Vector2.Lerp(lookDownDir, lookForwardDir, timer / (ledgeScanUpDuration / 2f));
            timer += Time.deltaTime;
            yield return null;
        }

        // 1C: PAUSA NO MEIO
        yield return new WaitForSeconds(ledgeMidScanPauseTime);

        // 1D: SUBIR DO MEIO ATÉ O TOPO
        timer = 0f;
        while (timer < ledgeScanUpDuration / 2f)
        {
            lookDirection = Vector2.Lerp(lookForwardDir, lookUpDir, timer / (ledgeScanUpDuration / 2f));
            timer += Time.deltaTime;
            yield return null;
        }

        // 1E: OLHAR PARA BAIXO NOVAMENTE
        timer = 0f;
        while (timer < 0.4f)
        {
            lookDirection = Vector2.Lerp(lookUpDir, lookDownDir, timer / 0.4f);
            timer += Time.deltaTime;
            yield return null;
        }

        // --- FASE 2: COLETA DE DADOS ---
        RaycastHit2D dropCheck = Physics2D.Raycast(motor.ledgeCheck.position, Vector2.down, maxSafeDropHeight, motor.groundLayer);
        RaycastHit2D jumpCheck = Physics2D.Raycast(eyes.position, isFacingRight ? Vector2.right : Vector2.left, maxJumpDistance, motor.groundLayer);

        bool canDescend = dropCheck.collider != null;
        bool canJump = jumpCheck.collider != null && IsJumpTargetValid(jumpCheck.point);

        // --- FASE 3: DECISÃO ---
        var availableActions = new List<PossibleLedgeAction>();
        if (canDescend) availableActions.Add(PossibleLedgeAction.Descend);
        if (canJump) availableActions.Add(PossibleLedgeAction.Jump);

        PossibleLedgeAction chosenAction = PossibleLedgeAction.Retreat;
        if (isAggressive && availableActions.Count > 0)
        {
            if (Random.value < retreatProbability)
            {
                chosenAction = PossibleLedgeAction.Retreat;
            }
            else
            {
                chosenAction = availableActions[Random.Range(0, availableActions.Count)];
            }
        }

        Debug.Log("Análise: Decisão final -> " + chosenAction);

        // --- FASE 4: EXECUÇÃO ---
        switch (chosenAction)
        {
            case PossibleLedgeAction.Descend:
                retreatProbability += 0.05f;
                float stepOffTimer = 0.5f;
                while (motor.IsGrounded() && stepOffTimer > 0)
                {
                    motor.Move((isFacingRight ? 1 : -1) * 0.5f);
                    stepOffTimer -= Time.deltaTime;
                    yield return null;
                }
                isExecutingAction = false;
                break;
            case PossibleLedgeAction.Jump:
                retreatProbability += 0.05f;
                yield return StartCoroutine(JumpRoutine(isFacingRight ? 1 : -1));
                break;
            case PossibleLedgeAction.Retreat:
                retreatProbability -= 0.05f;
                yield return StartCoroutine(LookAnRetreatRoutine());
                break;
        }

        retreatProbability = Mathf.Clamp(retreatProbability, 0.05f, 0.95f);
        ChangeState(isAggressive ? State.Hunting : State.Patrolling);
    }
    private void HandleClimbingMovement()
    {
        if (!motor.IsTouchingWall())
        {
            motor.RestoreGravity();
            ChangeState(State.Hunting);
            return;
        }

        if (!motor.IsObstacleAhead())
        {
            motor.RestoreGravity();
            StartCoroutine(VaultOverLedgeRoutine());
        }
        else
        {
            // --- CORREÇÃO ---
            // Passa a velocidade de escalada para o motor.
            motor.Climb(1f, climbSpeed);
        }
    }
    #endregion

    #region Health
    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        if (currentState == State.Dead) return; // Não pode tomar dano se já estiver morto

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} tomou {damage} de dano. Vida restante: {currentHealth}");

        // Aplica o knockback
        motor.ApplyKnockback(knockbackDirection * knockbackForce);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        Debug.Log($"{gameObject.name} foi derrotado.");
        ChangeState(State.Dead); // Adicione 'Dead' ao seu enum State
        isExecutingAction = true; // Para todas as outras ações
                                  // Adicione aqui a lógica de morte (ex: animação, desativar colisor, etc.)
        GetComponent<Collider2D>().enabled = false;
        motor.Stop();
        Destroy(gameObject, 3f); // Opcional: destruir o objeto após 3 segundos
    }
    #endregion

    #region Movement and Navigation Helpers
    void FindPlayer() { playerTarget = AIManager.Instance?.playerTarget; }
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
        lookDirection.x *= -1;
    }
    private void FaceTarget(Vector3 targetPosition)
    {
        if (isExecutingAction) return;
        if (targetPosition.x > transform.position.x && !isFacingRight) Flip();
        else if (targetPosition.x < transform.position.x && isFacingRight) Flip();
    }
    private bool IsJumpTargetValid(Vector3 target)
    {
        if (target.y - transform.position.y > maxJumpHeight) return false;
        if (target.y < motor.groundCheck_A.position.y) return false;
        RaycastHit2D surfaceCheck = Physics2D.Raycast(target + Vector3.up * 0.1f, Vector2.down, 0.5f, motor.groundLayer);
        return surfaceCheck.collider != null;
    }
    #endregion

    #region Perception Helpers (Vision, etc.)
    void HandleHeadLook()
    {
        if (isExecutingAction || currentState == State.Analyzing) return;

        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 targetLookDirection = forward;

        // --- LÓGICA DE PERCEPÇÃO BASEADA EM RAYCAST (CORRIGIDA) ---
        // 1. A origem agora leva em conta a direção de forma mais simples.
        Vector2 probeOrigin = (Vector2)transform.position + new Vector2(groundProbeDistance * (isFacingRight ? 1 : -1), 0.1f);

        // 2. A direção agora é construída a partir da direção 'forward' para ser imune a problemas de escala.
        Vector2 probeDirection = Quaternion.Euler(0, 0, -groundProbeAngle * (isFacingRight ? 1 : -1)) * forward;

        // 3. O resto funciona como antes.
        groundProbeHit = Physics2D.Raycast(probeOrigin, probeDirection, 15f, motor.groundLayer);

        if (currentState == State.Hunting || currentState == State.Attacking)
        {
            if (playerTarget != null) targetLookDirection = (playerTarget.position - eyes.position).normalized;
        }
        else if (currentState == State.Patrolling || currentState == State.Hunting)
        {
            if (groundProbeHit.collider)
            {
                targetLookDirection = (groundProbeHit.point - (Vector2)eyes.position).normalized;
            }
            else
            {
                targetLookDirection = probeDirection;
            }
        }

        lookDirection = Vector2.Lerp(lookDirection, targetLookDirection, Time.deltaTime * eyeSpeed);
    }
    private bool CanSeePlayer()
    {
        if (playerTarget == null) return false;
        float distanceToPlayer = Vector3.Distance(eyes.position, playerTarget.position);
        if (distanceToPlayer > visionRange) return false;
        Vector3 directionToPlayer = (playerTarget.position - eyes.position).normalized;
        if (Vector2.Angle(lookDirection, directionToPlayer) > visionAngle / 2f) return false;
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToPlayer, distanceToPlayer, visionBlockers);
        return hit.collider == null || hit.transform == playerTarget;
    }
    private bool IsPlayerInAttackRange()
    {
        if (playerTarget == null) return false;
        return Vector2.Distance(transform.position, playerTarget.position) <= attackRange;
    }
    private bool CanJumpOverWall()
    {
        // Esta função pode precisar de refinamento, mas é um bom começo.
        return !Physics2D.Raycast(eyes.position, Vector2.up, 3f, visionBlockers);
    }
    #endregion

    #region State Machine and Coroutines
    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        Debug.Log(gameObject.name + " mudando de estado: " + currentState + " -> " + newState);
        currentState = newState;
        if (currentState == State.Searching) { searchTimer = searchTime; }
    }
    private IEnumerator PatrolTurnRoutine()
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

        motor.Stop(); // Para no lugar para atacar

        // Pequena pausa para a "antecipação" do ataque
        yield return new WaitForSeconds(0.25f);

        // --- A LÓGICA DE DANO ---
        Debug.Log($"{gameObject.name} ATACA!");

        // 1. Detecta todos os colisores em um círculo de ataque à frente da IA
        Vector2 attackOrigin = (Vector2)transform.position + new Vector2(0.5f * (isFacingRight ? 1 : -1), 0.5f);
        Collider2D[] targetsHit = Physics2D.OverlapCircleAll(attackOrigin, attackRange, playerLayer);

        // 2. Itera por tudo que foi atingido
        foreach (Collider2D target in targetsHit)
        {
            PlayerStats player = target.GetComponent<PlayerStats>();
            if (player != null)
            {
                Debug.Log($"Atingiu {target.name}!");
                // 3. Calcula a direção do knockback
                Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;

                // 4. Chama a função TakeDamage do jogador
                player.TakeDamage(attackDamage, knockbackDirection);
            }
        }

        // Tempo de "recuperação" da animação de ataque
        yield return new WaitForSeconds(0.5f);

        isExecutingAction = false;

        // Inicia o cooldown global do ataque
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    private IEnumerator JumpRoutine(float direction)
    {
        isExecutingAction = true;
        motor.AddJumpForce(new Vector2(direction * airControlForce, jumpForce));
        yield return new WaitForSeconds(0.2f);
        while (!motor.IsGrounded()) { yield return null; }
        yield return new WaitForSeconds(0.5f);
        isExecutingAction = false;
    }
    private IEnumerator LookAnRetreatRoutine()
    {
        isExecutingAction = true;
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        float timer = 0f;
        while (timer < 0.3f)
        {
            lookDirection = Vector2.Lerp(lookDirection, forward, Time.deltaTime * eyeSpeed * 2);
            timer += Time.deltaTime;
            yield return null;
        }
        lookDirection = forward;
        yield return new WaitForSeconds(0.2f);
        Flip();
        yield return new WaitForSeconds(0.3f);
        float retreatTimer = 1.0f;
        while (retreatTimer > 0)
        {
            if (motor.IsGrounded())
            {
                motor.Move(isFacingRight ? 1 : -1);
            }
            retreatTimer -= Time.deltaTime;
            yield return null;
        }
        isExecutingAction = false;
    }
    private IEnumerator TacticalJumpAndGrabRoutine(Vector3 ledgePoint)
    {
        ChangeState(State.LedgeGrabbing);
        isExecutingAction = true;
        motor.Stop();
        motor.DisableGravity();

        float gravity = Physics2D.gravity.y * motor.GetRigidbody().gravityScale;
        float height = ledgePoint.y - transform.position.y;
        float displacementY = height + 0.5f;
        float velocityY = Mathf.Sqrt(-2 * gravity * displacementY);
        float timeToApex = velocityY / -gravity;
        float timeTotal = timeToApex * 2;
        float displacementX = Mathf.Abs(ledgePoint.x - transform.position.x);
        float velocityX = displacementX / timeTotal;

        Vector2 jumpVelocity = new Vector2(velocityX * (isFacingRight ? 1 : -1), velocityY);
        motor.ApplyVelocity(jumpVelocity);

        while (Vector2.Distance(transform.position, ledgePoint) > 0.5f)
        {
            yield return null;
        }

        motor.Stop();
        transform.position = new Vector3(ledgePoint.x - (0.5f * (isFacingRight ? 1 : -1)), ledgePoint.y - 0.5f, 0);

        yield return new WaitForSeconds(ledgeHangTime);

        transform.position += new Vector3(0.5f * (isFacingRight ? 1 : -1), 1f, 0);

        motor.EnableGravity();
        isExecutingAction = false;
        ChangeState(State.Hunting);
    }
    private IEnumerator VaultOverLedgeRoutine()
    {
        isExecutingAction = true;
        motor.AddJumpForce(new Vector2(isFacingRight ? 2f : -2f, 4f));
        yield return new WaitForSeconds(0.5f);
        isExecutingAction = false;
        ChangeState(State.Hunting);
    }
    #endregion

    #region Gizmos (HIPER-VISUALIZAÇÃO DE RAYCAST)
    void OnDrawGizmosSelected()
    {
        if (motor == null && GetComponent<AIPlatformerMotor>() != null) motor = GetComponent<AIPlatformerMotor>();
        if (motor == null) return;

        bool editorIsFacingRight = transform.localScale.x > 0;

        // 1. O SENSOR DE NAVEGAÇÃO (GROUND PROBE)
        Vector2 probeOrigin = (Vector2)transform.position + new Vector2(editorIsFacingRight ? groundProbeDistance : -groundProbeDistance, 0.1f);
        Vector2 probeDirection = Quaternion.Euler(0, 0, editorIsFacingRight ? -groundProbeAngle : groundProbeAngle) * Vector2.right;

        // Simula o Raycast no editor para feedback em tempo real
        RaycastHit2D hit = Physics2D.Raycast(probeOrigin, probeDirection, 15f, motor.groundLayer);

        if (hit.collider)
        {
            // A cor muda baseada na distância vs. o limite de perigo
            Gizmos.color = (hit.distance > ledgeDetectionThreshold) ? Color.red : Color.red;
            Gizmos.DrawLine(probeOrigin, hit.point);
            Gizmos.DrawWireSphere(hit.point, 0.15f);
        }
        else
        {
            // Se não atingiu nada, é sempre perigo (vermelho)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(probeOrigin, probeOrigin + probeDirection * 15f);
        }

        // 2. O CONE DE VISÃO TRADICIONAL (mantido para detecção de jogador)
        if (eyes == null) return;
        Vector3 forward = Application.isPlaying ? (Vector3)lookDirection.normalized : (transform.right * (transform.localScale.x > 0 ? 1 : -1));
        Gizmos.color = Color.yellow;
        Vector3 p1 = eyes.position + (Quaternion.Euler(0, 0, visionAngle / 2) * forward) * visionRange;
        Vector3 p2 = eyes.position + (Quaternion.Euler(0, 0, -visionAngle / 2) * forward) * visionRange;
        Gizmos.DrawLine(eyes.position, p1);
        Gizmos.DrawLine(eyes.position, p2);
        if (Application.isPlaying && CanSeePlayer()) { Gizmos.color = Color.red; Gizmos.DrawLine(eyes.position, playerTarget.position); }
        if (Application.isPlaying && lastSeenSafePlatform.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastSeenSafePlatform.Value, 0.5f);
            Gizmos.DrawLine(eyes.position, lastSeenSafePlatform.Value);
        }
        Gizmos.color = Color.red;
        Vector2 attackOriginGizmo = (Vector2)transform.position + new Vector2(0.5f * (editorIsFacingRight ? 1 : -1), 0.5f);
        Gizmos.DrawWireSphere(attackOriginGizmo, attackRange);
    }
    #endregion
}