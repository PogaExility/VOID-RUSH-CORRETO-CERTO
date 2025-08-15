using UnityEngine;
using System.Collections;

// Versão FINAL com lógica de controle aéreo e anti-flick.
[RequireComponent(typeof(AIPlatformerMotor))]
public class AIController : MonoBehaviour
{
    private enum State { Patrolling, Engaging, Attacking, Searching, Stuck }
    private State currentState;

    [Header("Referências")]
    private AIPlatformerMotor motor;
    private Transform playerTarget;
    public Transform eyes;

    [Header("Parâmetros de Visão")]
    public float visionRange = 15f;
    [Range(0, 360)] public float visionAngle = 90f;
    public LayerMask visionBlockers;
    public LayerMask playerLayer;

    [Header("Memória e Busca")]
    public float searchTime = 5f;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;

    [Header("Combate Tático")]
    public float idealCombatDistance = 8f;
    public float repositionDistanceThreshold = 2f;
    public float rangedAttackCooldown = 3f;
    private bool canAttack = true;

    [Header("Navegação")]
    public Vector2 jumpOverObstacleVelocity = new Vector2(5f, 14f);
    public float maxJumpDistance = 5f;
    public float patrolStopTime = 1.5f;
    private bool isFacingRight = true;
    private Coroutine navigationCoroutine;

    [Header("Anti-Armadilha")]
    public float timeUntilStuck = 3.0f;
    private float stuckTimer;
    private Vector3 lastPositionCheck;
    private float positionCheckInterval = 1.0f;
    private float positionCheckTimer;

    void Awake()
    {
        motor = GetComponent<AIPlatformerMotor>();
    }

    void Start()
    {
        playerTarget = AIManager.Instance?.playerTarget;
        isFacingRight = transform.localScale.x > 0;
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
        lastPositionCheck = transform.position;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (playerTarget == null) return;
        HandleStateTransitions();
        CheckIfStuck();
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;
        HandleActions();
    }

    // A lógica para detectar se a IA está presa foi movida para cá
    void CheckIfStuck()
    {
        // Se a IA não estiver tentando se mover ativamente, não está presa.
        if (currentState != State.Engaging && currentState != State.Searching && currentState != State.Patrolling)
        {
            stuckTimer = 0f;
            return;
        }

        positionCheckTimer += Time.deltaTime;
        if (positionCheckTimer >= positionCheckInterval)
        {
            positionCheckTimer = 0f;
            // Se moveu muito pouco E está no chão, pode estar preso
            if (Vector3.Distance(transform.position, lastPositionCheck) < 0.5f && motor.IsGrounded())
            {
                stuckTimer += positionCheckInterval;
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPositionCheck = transform.position;
        }

        if (stuckTimer >= timeUntilStuck)
        {
            ChangeState(State.Stuck);
        }
    }

    void HandleStateTransitions()
    {
        // Não mude de estado se estiver no ar
        if (!motor.IsGrounded()) return;

        bool canSeePlayer = CanSeePlayer();
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = playerTarget.position;
            if (currentState != State.Engaging && currentState != State.Attacking) ChangeState(State.Engaging);
        }

        switch (currentState)
        {
            case State.Engaging:
                if (canSeePlayer) { if (IsPlayerInAttackRange()) ChangeState(State.Attacking); }
                else { ChangeState(State.Searching); }
                break;
            case State.Attacking:
                if (canSeePlayer) { if (!IsPlayerInAttackRange()) ChangeState(State.Engaging); }
                else { ChangeState(State.Searching); }
                break;
        }
    }

    void HandleActions()
    {
        // Se a IA está no ar, sua única prioridade é controlar a queda.
        if (!motor.IsGrounded())
        {
            // Lógica de controle aéreo (opcional, por enquanto não faz nada para evitar flicks)
            return;
        }

        switch (currentState)
        {
            case State.Patrolling: Patrol(); break;
            case State.Engaging: NavigateTowards(playerTarget.position); break;
            case State.Searching:
                if (Vector2.Distance(transform.position, lastKnownPlayerPosition) < 1f)
                {
                    motor.Stop();
                    searchTimer -= Time.fixedDeltaTime;
                    if (searchTimer <= 0) ChangeState(State.Patrolling);
                }
                else
                {
                    NavigateTowards(lastKnownPlayerPosition);
                }
                break;
            case State.Attacking: Attack(); break;
            case State.Stuck: UnstuckRoutine(); break;
        }
    }

    // --- LÓGICAS DE ESTADO ---

    void Patrol()
    {
        if ((motor.IsObstacleAhead() || motor.IsLedgeAhead()))
        {
            if (navigationCoroutine == null)
                navigationCoroutine = StartCoroutine(StopAndTurnRoutine());
            return;
        }
        motor.Move(isFacingRight ? 1 : -1);
    }

    void NavigateTowards(Vector3 targetPosition)
    {
        FaceTarget(targetPosition);
        float direction = isFacingRight ? 1 : -1;

        if (motor.IsObstacleAhead())
        {
            motor.SetVelocity(jumpOverObstacleVelocity.x * direction, jumpOverObstacleVelocity.y);
            return;
        }

        if (motor.IsLedgeAhead())
        {
            if (CanMakeTheJump())
            {
                motor.SetVelocity(jumpOverObstacleVelocity.x * direction, jumpOverObstacleVelocity.y);
            }
            else
            {
                motor.Stop();
                if (CanSeePlayer()) ChangeState(State.Attacking);
            }
            return;
        }

        if (currentState == State.Engaging)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer > idealCombatDistance) motor.Move(direction);
            else if (distanceToPlayer < idealCombatDistance - repositionDistanceThreshold) motor.Move(-direction);
            else motor.Stop();
        }
        else
        {
            motor.Move(direction);
        }
    }

    void Attack()
    {
        FaceTarget(playerTarget.position);
        motor.Stop();
        if (canAttack) StartCoroutine(AttackCoroutine());
    }

    void UnstuckRoutine()
    {
        Debug.Log("Estou preso! Tentando um pulo para sair.");
        FaceTarget(playerTarget.position); // Vira para o jogador antes de pular
        float direction = isFacingRight ? 1 : -1;
        motor.SetVelocity(jumpOverObstacleVelocity.x * direction, jumpOverObstacleVelocity.y);

        searchTimer -= Time.fixedDeltaTime;
        if (searchTimer <= 0)
        {
            // Se ainda estiver preso, reseta e tenta de novo. Se saiu, o estado mudará naturalmente.
            ChangeState(State.Patrolling);
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTarget == null || eyes == null) return false;
        if (((1 << playerTarget.gameObject.layer) & playerLayer) == 0) return false;
        float distanceToPlayer = Vector3.Distance(eyes.position, playerTarget.position);
        if (distanceToPlayer > visionRange) return false;
        Vector3 directionToPlayer = (playerTarget.position - eyes.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.right * motor.currentFacingDirection, directionToPlayer);
        if (angleToPlayer > visionAngle / 2f) return false;
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToPlayer, distanceToPlayer, visionBlockers);
        if (hit.collider != null && hit.transform != playerTarget) return false;
        return true;
    }

    private bool CanMakeTheJump()
    {
        if (motor.ledgeCheck == null) return false;
        RaycastHit2D hit = Physics2D.Raycast(motor.ledgeCheck.position, Vector2.right * motor.currentFacingDirection, maxJumpDistance, motor.groundLayer);
        return hit.collider != null;
    }

    private bool IsPlayerInAttackRange()
    {
        if (playerTarget == null) return false;
        float dist = Vector2.Distance(transform.position, playerTarget.position);
        return dist <= idealCombatDistance + repositionDistanceThreshold;
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        stuckTimer = 0f; // Reseta o timer de 'preso' toda vez que o estado muda

        if (navigationCoroutine != null)
        {
            StopCoroutine(navigationCoroutine);
            navigationCoroutine = null;
        }

        if (currentState == State.Searching || currentState == State.Stuck)
        {
            searchTimer = searchTime;
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        if (targetPosition.x > transform.position.x && !isFacingRight) Flip();
        else if (targetPosition.x < transform.position.x && isFacingRight) Flip();
    }

    private IEnumerator StopAndTurnRoutine()
    {
        motor.Stop();
        yield return new WaitForSeconds(patrolStopTime);
        Flip();
        navigationCoroutine = null;
    }

    private IEnumerator AttackCoroutine()
    {
        canAttack = false;
        Debug.Log(gameObject.name + " está ATIRANDO no jogador!");
        yield return new WaitForSeconds(rangedAttackCooldown);
        canAttack = true;
    }
}