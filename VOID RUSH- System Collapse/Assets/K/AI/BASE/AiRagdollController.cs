using UnityEngine;
using System.Collections;

// O Cérebro principal do Ragdoll. Ele dá as intenções de movimento.
public class AIRagdollController : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Attacking }
    private State currentState;

    [Header("Referências")]
    public Transform playerTarget;
    public AIHeadController headController;
    public AILegController legController; // <- NOVO

    [Header("Parâmetros de Comportamento")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    // ... outros parâmetros ...

    void Start()
    {
        playerTarget = AIManager.Instance.playerTarget;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (playerTarget == null) return;

        // --- TRANSIÇÕES DE ESTADO ---
        if (IsPlayerInAttackRange()) ChangeState(State.Attacking);
        else if (IsPlayerInDetectionRange()) ChangeState(State.Chasing);
        else ChangeState(State.Patrolling);

        // --- ORDENS CONTÍNUAS ---
        HandleHeadLook();
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;

        // --- LÓGICA DE AÇÃO FÍSICA ---
        switch (currentState)
        {
            case State.Patrolling: PatrolLogic(); break;
            case State.Chasing: ChaseLogic(); break;
            case State.Attacking: AttackLogic(); break;
        }
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    void PatrolLogic()
    {
        // Intenção: andar para a direita
        legController.MoveInDirection(1);
    }

    void ChaseLogic()
    {
        float directionToPlayer = (playerTarget.position.x > transform.position.x) ? 1 : -1;
        // Intenção: andar na direção do jogador
        legController.MoveInDirection(directionToPlayer);
    }

    void AttackLogic()
    {
        // Intenção: parar de andar
        legController.StopMoving();
    }

    void HandleHeadLook()
    {
        if (headController == null) return;
        if (currentState == State.Chasing || currentState == State.Attacking)
        {
            headController.LookAt(playerTarget.position);
        }
        else
        {
            headController.ResetLookDirection(legController.GetBodyTransform()); // Usa o corpo como referência
        }
    }

    private bool IsPlayerInDetectionRange() => Vector2.Distance(legController.GetBodyTransform().position, playerTarget.position) < detectionRange;
    private bool IsPlayerInAttackRange() => Vector2.Distance(legController.GetBodyTransform().position, playerTarget.position) < attackRange;
}