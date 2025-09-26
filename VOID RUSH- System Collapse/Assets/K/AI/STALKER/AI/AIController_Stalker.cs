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
            // A Árvore de Comportamento agora é muito mais simples.
            // A maior parte da lógica está no nó de Patrulha.
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
        // A lógica de análise agora é chamada de dentro da patrulha, então não precisamos
        // de uma verificação de _isAnalyzing aqui. O cooldown de transição é suficiente.
        if (!_motor.IsTransitioningState)
        {
            _rootNode?.Evaluate();
        }
    }
    #endregion

    #region BEHAVIOR TREE NODES
    private NodeState CheckIfAwareOfPlayer() => _perception.IsAwareOfPlayer ? NodeState.SUCCESS : NodeState.FAILURE;

    // Lógica de combate simplificada para usar o novo sistema
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
            switch (query.detectedObstacle)
            {
                case AINavigationSystem.ObstacleType.FullWall:
                case AINavigationSystem.ObstacleType.JumpablePlatform:
                    _motor.Jump(jumpForce);
                    break;
                case AINavigationSystem.ObstacleType.Ledge:
                    _motor.HardStop();
                    break;
                default:
                    _motor.Move(huntTopSpeed);
                    break;
            }
        }
        return NodeState.RUNNING;
    }

    // O nó principal que agora contém TODA a lógica de navegação.
    private NodeState ProcessPatrolLogic()
    {
        // Se já estivermos analisando, não faça nada.
        if (_isAnalyzing) return NodeState.RUNNING;

        var query = _navigation.QueryEnvironment();

        switch (query.detectedObstacle)
        {
            case AINavigationSystem.ObstacleType.CrouchTunnel:
                _motor.StartCrouch();
                _motor.Move(patrolTopSpeed / 2);
                break;

            case AINavigationSystem.ObstacleType.JumpablePlatform:
                _motor.Jump(jumpForce);
                break;

            case AINavigationSystem.ObstacleType.FullWall:
                StartCoroutine(AnalyzeWallRoutine());
                break;

            case AINavigationSystem.ObstacleType.Ledge:
                StartCoroutine(AnalyzeLedgeRoutine());
                break;

            case AINavigationSystem.ObstacleType.None:
            default:
                if (_motor.IsCrouching) _motor.StopCrouch();
                _motor.Move(patrolTopSpeed);
                break;
        }

        return NodeState.RUNNING;
    }
    #endregion

    #region HELPER ROUTINES & LOGIC
    private IEnumerator AnalyzeWallRoutine()
    {
        _isAnalyzing = true;
        _motor.HardStop();
        // Nota: A rotina de análise visual precisa de um ponto de origem. Usaremos o lowerWallProbe como padrão.
        _perception.StartObstacleAnalysis(_navigation.lowerWallProbe.position, isLedge: false);
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
        _perception.StartObstacleAnalysis(_navigation.ledgeProbe.position, isLedge: true);
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