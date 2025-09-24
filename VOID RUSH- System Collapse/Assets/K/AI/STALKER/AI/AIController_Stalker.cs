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
    private bool _isAnalyzing = false;
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
    [Header("▶ Lógica de Escalada")]
    public float entrySpeed = 3f;
    public float entryDuration = 0.5f;
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
            new ActionNode(ProcessAnalysisTriggers),
            new ActionNode(ProcessClimbingLogic),
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
        if (!_isAnalyzing && !_motor.IsTransitioningState)
        {
            _rootNode?.Evaluate();
        }
    }
    #endregion

    #region BEHAVIOR TREE NODES
    private NodeState CheckIfAwareOfPlayer() => _perception.IsAwareOfPlayer ? NodeState.SUCCESS : NodeState.FAILURE;

    private NodeState ProcessClimbingLogic()
    {
        if (!_motor.IsClimbing) return NodeState.FAILURE;
        var query = _navigation.QueryEnvironment();

        if (query.canEnterCrouchTunnel)
        {
            _motor.StartCrouch();
            _motor.EnterTunnel(entrySpeed, entryDuration);
            return NodeState.SUCCESS;
        }

        _motor.Climb(1f);
        return NodeState.RUNNING;
    }

    private NodeState ProcessCombatLogic()
    {
        FaceTarget(_player.position);
        if (Vector2.Distance(transform.position, _player.position) <= attackRange) { _motor.Stop(); }
        else
        {
            var query = _navigation.QueryEnvironment();
            if (query.isAtLedge || query.dangerLedgeAhead) { _motor.HardStop(); }
            else if (query.anticipationLedgeAhead) { _motor.Brake(); }
            else
            {
                if (query.climbableWallAhead) _motor.StartClimb();
                else if (query.contactWallAhead) _motor.Jump(jumpForce);
                else _motor.Move(huntTopSpeed);
            }
        }
        return NodeState.RUNNING;
    }

    private NodeState ProcessAnalysisTriggers()
    {
        if (_motor.IsTransitioningState) return NodeState.FAILURE;
        var query = _navigation.QueryEnvironment();
        if (query.isAtLedge) { StartCoroutine(AnalyzeLedgeRoutine()); return NodeState.SUCCESS; }
        if (query.contactWallAhead && !query.climbableWallAhead && query.isGrounded) { StartCoroutine(AnalyzeWallRoutine()); return NodeState.SUCCESS; }
        return NodeState.FAILURE;
    }

    private NodeState ProcessPatrolLogic()
    {
        var query = _navigation.QueryEnvironment();

        // PRIORIDADE MÁXIMA: OPORTUNIDADE TÁTICA (SPELUNKER)
        if (query.climbableWallAhead && query.canEnterCrouchTunnel && query.isGrounded)
        {
            _motor.StartClimb();
            return NodeState.RUNNING;
        }

        // AMEAÇA PRIORIDADE 1: Paredes e Bordas
        if (query.dangerWallAhead || query.dangerLedgeAhead)
        {
            _motor.Move(patrolTopSpeed * 0.25f);
            return NodeState.RUNNING;
        }

        if (query.anticipationWallAhead || query.anticipationLedgeAhead)
        {
            _motor.Move(patrolTopSpeed * 0.5f);
            return NodeState.RUNNING;
        }

        // AMEAÇA PRIORIDADE 2: Tetos
        bool shouldCrouchDueToCeilingAhead = query.ceilingAhead;
        bool isClearToStand = query.isClearToStandUp;

        if (shouldCrouchDueToCeilingAhead && !_motor.IsCrouching)
        {
            _motor.StartCrouch();
        }
        else if (_motor.IsCrouching && !shouldCrouchDueToCeilingAhead && isClearToStand)
        {
            _motor.StopCrouch();
        }

        // AÇÃO PADRÃO: Movimento
        if (_motor.IsCrouching)
        {
            _motor.Move(patrolTopSpeed * 0.5f);
        }
        else
        {
            _motor.Move(patrolTopSpeed);
        }

        return NodeState.RUNNING;
    }
    #endregion

    #region HELPER ROUTINES & LOGIC
    // --- CONTEÚDO DAS ROTINAS RESTAURADO ---
    private IEnumerator AnalyzeWallRoutine()
    {
        _isAnalyzing = true;
        _motor.HardStop();
        _perception.StartObstacleAnalysis(_navigation.contactWallProbeOrigin.position, isLedge: false);
        yield return new WaitForSeconds(wallAnalysisDuration);
        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());
        _perception.StopObstacleAnalysis();
        _isAnalyzing = false;
    }

    private IEnumerator AnalyzeLedgeRoutine()
    {
        _isAnalyzing = true;
        _motor.HardStop();
        _perception.StartObstacleAnalysis(_navigation.ledgeProbeOrigin.position, isLedge: true);
        yield return new WaitForSeconds(ledgeAnalysisDuration);
        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());
        _perception.StopObstacleAnalysis();
        _isAnalyzing = false;
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