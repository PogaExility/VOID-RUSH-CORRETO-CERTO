using UnityEngine;
using BehaviorTree;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AIPerceptionSystem), typeof(AIPlatformerMotor), typeof(AINavigationSystem))]
public class AIController_Stalker : MonoBehaviour
{
    private Node _rootNode;
    private AIPerceptionSystem _perception;
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private Transform _player;

    [Header("▶ Configuração de Combate")]
    public float moveSpeed = 4f;
    public float huntSpeed = 6f;
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

    // CORRIGIDO: Agora usa a nova estrutura QueryEnvironment
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
            if (query.wallAhead || query.jumpablePlatformAhead) _motor.Jump(jumpForce);
            else if (query.holeAhead) _motor.Stop();
            else _motor.Move(huntSpeed);
        }
        return NodeState.RUNNING;
    }

    // CORRIGIDO: Agora usa a nova estrutura QueryEnvironment
    private NodeState ProcessPatrolLogic()
    {
        var query = _navigation.QueryEnvironment();

        if (query.ceilingAhead && !_motor.IsCrouching) _motor.StartCrouch();
        else if (!query.ceilingAhead && _motor.IsCrouching) _motor.StopCrouch();

        if (_canFlip && (query.wallAhead || query.holeAhead))
        {
            _motor.Flip();
            StartCoroutine(FlipCooldownRoutine());
        }

        _motor.Move(moveSpeed);
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