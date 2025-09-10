using UnityEngine;
using BehaviorTree;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AIPerceptionSystem), typeof(AIPlatformerMotor), typeof(AINavigationSystem))]
public class AIController_Stalker : MonoBehaviour
{
    #region REFERENCES & STATE
    private Node _rootNode;
    private AIPerceptionSystem _perception;
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private Transform _player;
    private bool _canFlip = true;
    private bool _isAnalyzingLedge = false;
    private bool _isAnalyzingWall = false;
    #endregion

    #region CONFIGURATION
    [Header("▶ Configuração de Combate")]
    public float patrolTopSpeed = 4f;
    public float huntTopSpeed = 7f;
    public float jumpForce = 15f;
    public float attackRange = 1.5f;

    [Header("▶ Lógica de Análise")]
    public float flipCooldown = 0.5f;
    public float ledgeAnalysisDuration = 2.0f;
    public float wallAnalysisDuration = 1.5f;
    #endregion

    #region UNITY LIFECYCLE & BEHAVIOR TREE SETUP
    void Start()
    {
        _perception = GetComponent<AIPerceptionSystem>();
        _motor = GetComponent<AIPlatformerMotor>();
        _navigation = GetComponent<AINavigationSystem>();
        _player = AIManager.Instance.playerTarget;

        _rootNode = new Selector(new List<Node>
        {
            new ActionNode(ProcessClimbingLogic),
            new Sequence(new List<Node>
            {
                new ActionNode(CheckIfAwareOfPlayer),
                new ActionNode(ProcessCombatLogic)
            }),
            new ActionNode(ProcessLedgeAnalysisLogic),
            new ActionNode(ProcessWallAnalysisLogic),
            new ActionNode(ProcessPatrolLogic)
        });
    }

    void Update()
    {
        _rootNode?.Evaluate();
    }
    #endregion

    #region BEHAVIOR TREE NODES
    private NodeState CheckIfAwareOfPlayer() => _perception.IsAwareOfPlayer ? NodeState.SUCCESS : NodeState.FAILURE;

    private NodeState ProcessClimbingLogic()
    {
        if (!_motor.IsClimbing) return NodeState.FAILURE;
        var query = _navigation.QueryEnvironment();
        if (query.canMantleLedge)
        {
            _motor.StopClimb();
            _motor.GetComponent<Rigidbody2D>().AddForce(transform.right * 3f + Vector3.up * 2f, ForceMode2D.Impulse);
        }
        else { _motor.Climb(1f); }
        return NodeState.RUNNING;
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
            if (query.isAtLedge || query.dangerLedgeAhead) { _motor.Stop(); }
            else if (query.anticipationLedgeAhead) { _motor.Move(huntTopSpeed / 2); }
            else
            {
                if (query.climbableWallAhead) _motor.StartClimb();
                else if (query.wallAhead) _motor.Jump(jumpForce);
                else _motor.Move(huntTopSpeed);
            }
        }
        return NodeState.RUNNING;
    }

    private NodeState ProcessLedgeAnalysisLogic()
    {
        if (_isAnalyzingLedge) return NodeState.RUNNING;

        var query = _navigation.QueryEnvironment();
        if (query.isAtLedge)
        {
            StartCoroutine(AnalyzeLedgeRoutine());
            return NodeState.RUNNING;
        }
        return NodeState.FAILURE;
    }

    private NodeState ProcessWallAnalysisLogic()
    {
        if (_isAnalyzingWall) return NodeState.RUNNING;

        var query = _navigation.QueryEnvironment();
        if (query.wallAhead && !query.climbableWallAhead && query.isGrounded)
        {
            StartCoroutine(AnalyzeWallRoutine());
            return NodeState.RUNNING;
        }
        return NodeState.FAILURE;
    }

    private NodeState ProcessPatrolLogic()
    {
        var query = _navigation.QueryEnvironment();

        if (query.anticipationLedgeAhead || (query.wallAhead && !query.climbableWallAhead))
        {
            _motor.Brake();
        }
        else
        {
            _motor.Move(patrolTopSpeed);
        }

        return NodeState.RUNNING;
    }
    #endregion

    #region HELPER ROUTINES & LOGIC
    private IEnumerator AnalyzeWallRoutine()
    {
        _isAnalyzingWall = true;
        _motor.Stop();
        _perception.StartObstacleAnalysis(_navigation.wallProbeOrigin.position, isLedge: false);

        yield return new WaitForSeconds(wallAnalysisDuration);

        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());

        _perception.StopObstacleAnalysis();
        _isAnalyzingWall = false;
    }

    private IEnumerator AnalyzeLedgeRoutine()
    {
        _isAnalyzingLedge = true;
        _motor.Stop();
        _perception.StartObstacleAnalysis(_navigation.ledgeProbeOrigin.position, isLedge: true);

        yield return new WaitForSeconds(ledgeAnalysisDuration);

        var query = _navigation.QueryEnvironment();
        if (!query.canSafelyDrop)
        {
            _motor.Flip();
            StartCoroutine(FlipCooldownRoutine());
        }
        else
        {
            // Lógica futura para descer
            _motor.Flip();
            StartCoroutine(FlipCooldownRoutine());
        }

        _perception.StopObstacleAnalysis();
        _isAnalyzingLedge = false;
    }

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