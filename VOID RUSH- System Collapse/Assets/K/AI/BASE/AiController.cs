using UnityEngine;
using System.Collections;

// Versão FINAL do Controller. Lógica de virar mais estável.
[RequireComponent(typeof(AIPlatformerMotor))]
public class AIController : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Attacking }
    private State currentState;

    [Header("Referências")]
    private AIPlatformerMotor motor;
    private Transform playerTarget;

    [Header("Parâmetros de Comportamento")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float patrolWaitTime = 2f;
    public float attackCooldown = 2f;

    private bool isPatrolWaiting;
    private bool canAttack = true;
    private bool isFacingRight = true;

    void Awake()
    {
        motor = GetComponent<AIPlatformerMotor>();
    }

    void Start()
    {
        playerTarget = AIManager.Instance.playerTarget;
        // Verifica a direção inicial com base na escala do objeto
        isFacingRight = transform.localScale.x > 0;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (playerTarget == null) return;

        switch (currentState)
        {
            case State.Patrolling: PatrolLogic(); break;
            case State.Chasing: ChaseLogic(); break;
            case State.Attacking: AttackLogic(); break;
        }

        // Transições de estado
        if (currentState != State.Attacking && IsPlayerInAttackRange()) ChangeState(State.Attacking);
        else if (currentState != State.Chasing && IsPlayerInDetectionRange() && !IsPlayerInAttackRange()) ChangeState(State.Chasing);
        else if ((currentState == State.Chasing || currentState == State.Attacking) && !IsPlayerInDetectionRange()) ChangeState(State.Patrolling);
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    // --- LÓGICAS DOS ESTADOS ---

    void PatrolLogic()
    {
        if (isPatrolWaiting) return;

        if ((motor.IsWallAhead() || motor.IsLedgeAhead()) && motor.IsGrounded())
        {
            StartCoroutine(PatrolWaitAndTurn());
            return;
        }

        float direction = isFacingRight ? 1 : -1;
        motor.Move(direction);
    }

    private IEnumerator PatrolWaitAndTurn()
    {
        isPatrolWaiting = true;
        motor.Stop();
        yield return new WaitForSeconds(patrolWaitTime);
        Flip();
        isPatrolWaiting = false;
    }

    void ChaseLogic()
    {
        // Vira para encarar o jogador
        if (playerTarget.position.x > transform.position.x && !isFacingRight) Flip();
        else if (playerTarget.position.x < transform.position.x && isFacingRight) Flip();

        if ((motor.IsWallAhead() || motor.IsLedgeAhead()) && motor.IsGrounded())
        {
            float direction = isFacingRight ? 1 : -1;
            motor.DirectionalJump(direction);
            return;
        }

        float moveDirection = isFacingRight ? 1 : -1;
        motor.Move(moveDirection);
    }

    void AttackLogic()
    {
        if (playerTarget.position.x > transform.position.x && !isFacingRight) Flip();
        else if (playerTarget.position.x < transform.position.x && isFacingRight) Flip();

        motor.Stop();
        if (canAttack) StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        canAttack = false;
        Debug.Log(gameObject.name + " está ATACANDO o jogador!");
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // --- FUNÇÕES DE VERIFICAÇÃO ---
    private bool IsPlayerInDetectionRange() => Vector2.Distance(transform.position, playerTarget.position) < detectionRange;
    private bool IsPlayerInAttackRange() => Vector2.Distance(transform.position, playerTarget.position) < attackRange;
}