using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// O "Notebook" da IA (v5.1 - Definitivo).
/// Este componente único e robusto gerencia TUDO: cérebro, sentidos, corpo e vitalidade.
/// Ele lê um perfil do EnemySO e executa todos os comportamentos de forma autônoma e tática.
/// Projetado para ser a única peça de lógica necessária para uma IA de plataforma completa.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AI_Agent : MonoBehaviour
{
    // =================================================================================================
    // 1. CONFIGURAÇÃO E DADOS
    // =================================================================================================

    #region Configuração e Dados

    [Header("▶ Perfil da IA")]
    [Tooltip("O ScriptableObject que define todos os atributos e comportamentos desta IA.")]
    public EnemySO profile;

    [Header("▶ Referências Físicas Essenciais")]
    [Tooltip("Ponto de origem da visão (na altura da cabeça).")]
    [SerializeField] private Transform eyes;
    [Tooltip("Ponto na base do inimigo para checar o chão.")]
    [SerializeField] private Transform groundProbe;
    [Tooltip("Ponto na frente do corpo para analisar o terreno.")]
    [SerializeField] private Transform navigationProbe;

    #endregion

    // =================================================================================================
    // 2. MÁQUINA DE ESTADOS
    // =================================================================================================

    #region Máquina de Estados

    /// <summary>
    /// Os estados comportamentais da IA.
    /// </summary>
    private enum State
    {
        Spawning,           // Estado inicial, aguardando para ativar.
        Patrolling,         // Movendo-se pelo cenário, procurando alvos.
        Engaging,           // Em modo de combate ativo (persegue, ataca, recua).
        Investigating,      // Movendo-se para uma posição de interesse.
        ExecutingManeuver,  // Travado enquanto executa uma ação de navegação (pulo, virada).
        Attacking,          // Travado enquanto executa uma animação de ataque.
        Stunned,            // Incapacitado temporariamente por dano (knockback).
        Dead                // Derrotado.
    }
    private State currentState;

    #endregion

    // =================================================================================================
    // 3. ESTADO INTERNO E REFERÊNCIAS
    // =================================================================================================

    #region Estado Interno

    // Componentes e Módulos Físicos
    private Rigidbody2D rb;
    private CapsuleCollider2D agentCollider;

    // Referências de Alvo
    private Transform playerTarget;
    private PlayerStats playerStats;

    // Estado do Motor
    private bool isFacingRight = true;
    private float currentSpeedTarget = 0f;
    private float originalGravityScale;

    // Estado da Percepção
    private bool canSeePlayer = false;
    private float awarenessTimer;
    private Vector2 lastKnownPlayerPosition;

    // Estado de Combate
    private float currentHealth;
    private bool canAttack = true;
    private Coroutine attackCooldownCoroutine;

    // --- VARIÁVEIS FALTANDO, ADICIONADAS AQUI ---
    private float stateTimer;
    private LayerMask obstacleLayer;

    #endregion

    // =================================================================================================
    // 4. CICLO DE VIDA E INICIALIZAÇÃO
    // =================================================================================================

    #region Inicialização

    /// <summary>
    /// Awake é chamado para inicializar componentes.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        agentCollider = GetComponent<CapsuleCollider2D>();
        originalGravityScale = rb.gravityScale;
    }

    /// <summary>
    /// Start é chamado para configurar o estado inicial e as referências.
    /// </summary>
    private void Start()
    {
        if (profile == null) { this.enabled = false; return; }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
        else { this.enabled = false; return; }

        // Atribui a obstacleLayer local com base no perfil.
        this.obstacleLayer = profile.obstacleLayer;

        // HealthSystem não precisa mais de Initialize.
        currentHealth = profile.maxHealth;

        ChangeState(State.Patrolling);
    }

    #endregion

    // =================================================================================================
    // 5. CICLO PRINCIPAL DA IA (PENSAR -> AGIR)
    // =================================================================================================

    #region Ciclo Principal

    /// <summary>
    /// O loop principal da IA, chamado a cada frame.
    /// </summary>
    private void Update()
    {
        // A IA está "travada" em um estado de ação que não pode ser interrompido?
        if (currentState == State.Dead || currentState == State.Stunned || currentState == State.ExecutingManeuver || currentState == State.Attacking)
        {
            return;
        }

        // Executa o ciclo de percepção, decisão e ação.
        Sense();
        Think();
        Act();
    }

    /// <summary>
    /// A fase de percepção. A IA coleta informações sobre o mundo.
    /// </summary>
    private void Sense()
    {
        canSeePlayer = CheckVision();
    }

    /// <summary>
    /// A fase de decisão. A IA usa as informações para escolher um estado.
    /// </summary>
    private void Think()
    {
        bool isAware = (currentState == State.Engaging || currentState == State.Investigating);
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (isAware && distanceToPlayer > profile.engagementRange)
        {
            ChangeState(State.Patrolling);
            return;
        }

        if (!isAware && canSeePlayer)
        {
            ChangeState(State.Engaging);
            return;
        }

        if (currentState == State.Engaging && !canSeePlayer)
        {
            lastKnownPlayerPosition = playerTarget.position;
            ChangeState(State.Investigating);
            return;
        }

        if (currentState == State.Investigating && stateTimer <= 0)
        {
            ChangeState(State.Patrolling);
        }
    }

    /// <summary>
    /// A fase de ação. A IA executa o comportamento do estado atual.
    /// </summary>
    private void Act()
    {
        if (stateTimer > 0) stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrolBehavior();
                break;
            case State.Engaging:
                HandleEngagementBehavior();
                break;
            case State.Investigating:
                FaceTarget(lastKnownPlayerPosition);
                if (Vector2.Distance(transform.position, lastKnownPlayerPosition) > 1f)
                {
                    Move(profile.chaseSpeed * 0.7f);
                }
                else
                {
                    Stop();
                }
                break;
        }
    }

    #endregion

    // =================================================================================================
    // 6. COMPORTAMENTOS DE ESTADO DETALHADOS
    // =================================================================================================

    #region Comportamentos

    /// <summary>
    /// Lógica de comportamento para o estado de Patrulha, com navegação de plataforma avançada.
    /// </summary>
    private void HandlePatrolBehavior()
    {
        if (!IsGrounded())
        {
            Move(0);
            return;
        }

        var navQuery = AnalyzePathAhead();

        // --- CORREÇÕES AQUI ---
        switch (navQuery.ObstacleType)
        {
            case PathObstacleType.Clear:
                Move(profile.patrolSpeed);
                break;

            case PathObstacleType.JumpableObstacle:
                if (navQuery.ObstacleHeight <= profile.maxJumpableHeight)
                {
                    StartCoroutine(JumpObstacleRoutine());
                }
                else
                {
                    StartCoroutine(EvaluateAndTurnRoutine());
                }
                break;

            case PathObstacleType.Wall:
            case PathObstacleType.Ledge:
                StartCoroutine(EvaluateAndTurnRoutine());
                break;
        }
    }

    /// <summary>
    /// Lógica de comportamento para o estado de Engajamento em Combate.
    /// </summary>
    private void HandleEngagementBehavior()
    {
        float distance = Vector2.Distance(transform.position, playerTarget.position);
        FaceTarget(playerTarget.position);

        if (canAttack && distance <= profile.attackRange)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        if (profile.aiType == AIType.Ranged && distance < profile.idealAttackDistance)
        {
            Move(-profile.chaseSpeed);
        }
        else
        {
            Move(profile.chaseSpeed);
        }
    }

    #endregion

    // =================================================================================================
    // 7. PERCEPÇÃO E ANÁLISE DE TERRENO
    // =================================================================================================

    #region Sensores e Análise

    /// <summary>
    /// Verifica se a IA pode ver o jogador (distância, ângulo e obstáculos).
    /// </summary>
    private bool CheckVision()
    {
        if (playerTarget == null || eyes == null) return false;
        Vector2 origin = eyes.position;
        Vector2 target = playerTarget.position;
        if (Vector2.SqrMagnitude(target - origin) > profile.visionRange * profile.visionRange) return false;

        Vector2 direction = (target - origin).normalized;
        if (Vector2.Angle(transform.right, direction) > profile.visionAngle / 2) return false;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, profile.visionRange, obstacleLayer);
        return hit.collider == null || hit.transform == playerTarget;
    }

    /// <summary>
    /// Estrutura para o relatório da análise de terreno.
    /// </summary>
    private enum PathObstacleType { Clear, Wall, JumpableObstacle, Ledge }
    private struct NavigationQueryResult
    {
        public PathObstacleType ObstacleType;
        public float ObstacleHeight;
    }

    /// <summary>
    /// Analisa o caminho à frente usando múltiplos Raycasts para uma detecção robusta.
    /// </summary>
    private NavigationQueryResult AnalyzePathAhead()
    {
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;

        RaycastHit2D wallHit = Physics2D.Raycast((Vector2)navigationProbe.position + Vector2.up * agentCollider.size.y * 0.8f, dir, 0.5f, obstacleLayer);
        if (wallHit.collider != null) return new NavigationQueryResult { ObstacleType = PathObstacleType.Wall };

        RaycastHit2D obstacleHit = Physics2D.Raycast(navigationProbe.position, dir, 0.5f, obstacleLayer);
        if (obstacleHit.collider != null)
        {
            float height = obstacleHit.collider.bounds.max.y - groundProbe.position.y;
            return new NavigationQueryResult { ObstacleType = PathObstacleType.JumpableObstacle, ObstacleHeight = height };
        }

        RaycastHit2D groundAheadHit = Physics2D.Raycast((Vector2)navigationProbe.position + dir * 0.5f, Vector2.down, 1f, obstacleLayer);
        if (groundAheadHit.collider == null) return new NavigationQueryResult { ObstacleType = PathObstacleType.Ledge };

        return new NavigationQueryResult { ObstacleType = PathObstacleType.Clear };
    }

    #endregion

    // =================================================================================================
    // 8. CORROTINAS E AÇÕES
    // =================================================================================================

    #region Corrotinas e Ações

    /// <summary>
    /// Realiza a transição para um novo estado e executa a lógica de entrada.
    /// </summary>
    private void ChangeState(State newState)
    {
        if (currentState == newState) return;

        // Limpa corrotinas de ações, mas não de cooldowns.
        if (attackCooldownCoroutine != null) StopCoroutine(attackCooldownCoroutine);

        //Debug.Log($"[AI State] {gameObject.name}: {currentState} -> {newState}");
        currentState = newState;

        if (currentState == State.Investigating)
        {
            stateTimer = profile.memoryDuration;
        }
    }

    private IEnumerator EvaluateAndTurnRoutine()
    {
        ChangeState(State.ExecutingManeuver);
        Stop();
        yield return new WaitForSeconds(profile.patrolPauseDuration);
        Flip();
        ChangeState(State.Patrolling);
    }

    private IEnumerator JumpObstacleRoutine()
    {
        ChangeState(State.ExecutingManeuver);
        Stop();
        yield return new WaitForSeconds(0.1f);
        Jump();
        yield return new WaitForSeconds(0.8f);
        ChangeState(State.Patrolling);
    }

    private IEnumerator AttackRoutine()
    {
        ChangeState(State.Attacking);
        canAttack = false;
        Stop();
        yield return new WaitForSeconds(0.5f);

        if (playerStats != null)
        {
            Vector2 knockbackDir = (playerTarget.position - transform.position).normalized;
            playerStats.TakeDamage(profile.attackDamage, knockbackDir, profile.attackKnockbackPower);
        }

        attackCooldownCoroutine = StartCoroutine(AttackCooldownRoutine());
        ChangeState(State.Engaging);
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(profile.attackCooldown);
        canAttack = true;
    }

    public void TriggerKnockback(Vector2 direction, float force)
    {
        StartCoroutine(KnockbackRoutine(direction, force));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        ChangeState(State.Stunned);
        ApplyKnockback(direction, force);
        yield return new WaitForSeconds(0.4f);
        ChangeState(State.Investigating);
    }

    public void OnDeath()
    {
        ChangeState(State.Dead);
        Stop();
        agentCollider.enabled = false;
        Destroy(gameObject, 3f);
    }
    #endregion

    // =================================================================================================
    // 9. COMANDOS DO MOTOR E FÍSICA
    // =================================================================================================

    #region Comandos de Motor e Física
    private void Move(float speed)
    {
        currentSpeedTarget = speed;
        float targetXVelocity = currentSpeedTarget * (isFacingRight ? 1 : -1);
        float accelRate = (Mathf.Abs(targetXVelocity) > 0.01f) ? profile.acceleration : profile.deceleration;
        float speedDifference = targetXVelocity - rb.linearVelocity.x;
        rb.AddForce(speedDifference * accelRate * Vector2.right);
    }

    private void Stop()
    {
        currentSpeedTarget = 0f;
        float accelRate = profile.deceleration;
        float speedDifference = 0 - rb.linearVelocity.x;
        rb.AddForce(speedDifference * accelRate * Vector2.right);
    }

    private void Jump(float strength = 1f)
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * (profile.maxJumpForce * Mathf.Clamp01(strength)), ForceMode2D.Impulse);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void ApplyKnockback(Vector2 direction, float force)
    {
        currentSpeedTarget = 0;
        if (rb.bodyType != RigidbodyType2D.Dynamic) return;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    private bool IsGrounded()
    {
        if (groundProbe == null) return false;
        return Physics2D.Raycast(groundProbe.position, Vector2.down, 0.1f, obstacleLayer);
    }

    private void FaceTarget(Vector2 targetPosition)
    {
        if (currentState == State.ExecutingManeuver || currentState == State.Attacking) return;
        if ((targetPosition.x > transform.position.x && !isFacingRight) ||
            (targetPosition.x < transform.position.x && isFacingRight))
        {
            Flip();
        }
    }

    #endregion

    // =================================================================================================
    // 10. GIZMOS PARA DEBUG
    // =================================================================================================

    #region Gizmos

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (profile == null) return;
        if (agentCollider == null) agentCollider = GetComponent<CapsuleCollider2D>();

        bool facingRight = Application.isPlaying ? isFacingRight : transform.localScale.x > 0;
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;

        // Gizmos de Navegação
        if (navigationProbe != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay((Vector2)navigationProbe.position + Vector2.up * agentCollider.size.y * 0.8f, dir * 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(navigationProbe.position, dir * 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay((Vector2)navigationProbe.position + dir * 0.5f, Vector2.down * 1f);
        }

        // Gizmos de Percepção
        if (eyes != null)
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.forward, profile.attackRange);
            Handles.color = Color.magenta;
            Handles.DrawWireDisc(transform.position, Vector3.forward, profile.personalSpaceRadius);

            Vector3 visionOrigin = eyes.position;
            Vector3 forward = transform.right;

            if (Application.isPlaying)
            {
                switch (currentState)
                {
                    case State.Engaging: Handles.color = new Color(1, 0, 0, 0.1f); break;
                    case State.Investigating: Handles.color = new Color(1, 0.5f, 0, 0.1f); break;
                    default: Handles.color = new Color(1, 1, 0, 0.1f); break;
                }
            }
            else
            {
                Handles.color = new Color(1, 1, 0, 0.1f);
            }

            Vector3 arcStart = Quaternion.AngleAxis(-profile.visionAngle / 2, Vector3.forward) * forward;
            Handles.DrawSolidArc(visionOrigin, Vector3.forward, arcStart, profile.visionAngle, profile.visionRange);
        }
    }
#endif

    #endregion
}