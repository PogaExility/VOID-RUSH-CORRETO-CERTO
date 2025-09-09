using UnityEngine;
using BehaviorTree;
using System.Collections;
using System.Collections.Generic;

public class AIController_Stalker : MonoBehaviour
{
    private Node _rootNode;
    private AIPerceptionSystem _perception;
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private Transform _player;

    [Header("▶ Configuração de Combate")]
    public float patrolTopSpeed = 4f;
    public float huntTopSpeed = 7f; // Renomeado para clareza
    public float jumpForce = 15f;
    public float attackRange = 1.5f;

    [Header("▶ Lógica de Patrulha")]
    public float flipCooldown = 0.5f;
    private bool _canFlip = true;

    void Start()
    {
        _perception = GetComponent<AIPerceptionSystem>();
        _motor = GetComponent<AIPlatformerMotor>();
        _navigation = GetComponent<AINavigationSystem>();
        _player = AIManager.Instance.playerTarget;

        _rootNode = new Selector(new List<Node>
        {
            new ActionNode(ProcessClimbingLogic), // A escalada tem prioridade máxima
            new Sequence(new List<Node>
            {
                new ActionNode(CheckIfAwareOfPlayer),
                new ActionNode(ProcessCombatLogic)
            }),
            new ActionNode(ProcessPatrolLogic)
        });
    }

    void Update()
    {
        _rootNode?.Evaluate();
    }

    private NodeState CheckIfAwareOfPlayer() => _perception.IsAwareOfPlayer ? NodeState.SUCCESS : NodeState.FAILURE;

    private NodeState ProcessClimbingLogic()
    {
        if (!_motor.IsClimbing) return NodeState.FAILURE; // Se não está a escalar, este nó falha.

        Debug.Log("[AIController] A processar lógica de ESCALADA.");
        var query = _navigation.QueryEnvironment();

        if (query.canMantleLedge)
        {
            Debug.Log("[AIController] Decisão: Terminar escalada (chegou ao topo).");
            _motor.StopClimb();
            // Adicionar uma pequena força para o "empurrar" para cima da plataforma
            _motor.GetComponent<Rigidbody2D>().AddForce(transform.right * 3f + Vector3.up * 2f, ForceMode2D.Impulse);
        }
        else
        {
            _motor.Climb(1f); // Continua a subir
        }
        return NodeState.RUNNING; // Escalar é uma ação contínua
    }

    private NodeState ProcessCombatLogic()
    {
        FaceTarget(_player.position);

        if (Vector2.Distance(transform.position, _player.position) <= attackRange)
        {
            _motor.Stop();
        }
        else
        {
            var query = _navigation.QueryEnvironment();
            if (query.climbableWallAhead) _motor.StartClimb();
            else if (query.wallAhead) _motor.Jump(jumpForce);

            else if (query.isAtLedge) _motor.Stop();
            else _motor.Move(huntTopSpeed);
        }
        return NodeState.RUNNING;
    }

    private NodeState ProcessPatrolLogic()
    {
        var query = _navigation.QueryEnvironment();

        if (query.climbableWallAhead)
        {
            Debug.Log("[AIController] Decisão de Patrulha: Iniciar escalada.");
            _motor.StartClimb();
        }
        else if (_canFlip && (query.wallAhead || query.isAtLedge))
        {
            _motor.Flip();
            StartCoroutine(FlipCooldownRoutine());
        }

        _motor.Move(patrolTopSpeed);
        return NodeState.RUNNING;
    }

    #region Lógica de "Flip" e Cooldown
    private void FaceTarget(Vector3 targetPosition)
    {
        float dotProduct = Vector2.Dot((targetPosition - transform.position).normalized, transform.right);
        if (_canFlip && dotProduct < -0.5f)
        {
            _motor.Flip();
            StartCoroutine(FlipCooldownRoutine());
        }
    }

    private IEnumerator FlipCooldownRoutine()
    {
        _canFlip = false;
        yield return new WaitForSeconds(flipCooldown);
        _canFlip = true;
    }
    #endregion
}