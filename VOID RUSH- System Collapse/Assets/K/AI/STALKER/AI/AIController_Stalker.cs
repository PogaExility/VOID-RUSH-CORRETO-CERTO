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
    [Tooltip("A distância que ele recua da parede após a análise.")]
    public float wallRetreatDistance = 1.0f;
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
            // A ORDEM É IMPORTANTE: Primeiro ele verifica se precisa de analisar, depois patrulha.
            new ActionNode(ProcessAnalysisTriggers),
            new ActionNode(ProcessPatrolLogic)
        });
    }

    void Update()
    {
        // Se uma rotina de análise está a decorrer, ela tem controlo exclusivo.
        // A Árvore de Comportamento está efetivamente em pausa.
        if (_isAnalyzingLedge || _isAnalyzingWall) return;

        _rootNode?.Evaluate();
    }
    #endregion

    #region BEHAVIOR TREE NODES
    private NodeState CheckIfAwareOfPlayer() => _perception.IsAwareOfPlayer ? NodeState.SUCCESS : NodeState.FAILURE;

    private NodeState ProcessClimbingLogic() { /* ...código inalterado... */ return NodeState.FAILURE; }

    private NodeState ProcessCombatLogic() { /* ...código inalterado... */ return NodeState.RUNNING; }

    // NÓ DE GATILHO UNIFICADO PARA ANÁLISE
    private NodeState ProcessAnalysisTriggers()
    {
        // Se já está a analisar, este nó não faz nada
        if (_isAnalyzingLedge || _isAnalyzingWall) return NodeState.FAILURE;

        var query = _navigation.QueryEnvironment();

        // Gatilho para análise de precipício
        if (query.isAtLedge)
        {
            StartCoroutine(AnalyzeLedgeRoutine());
            return NodeState.SUCCESS; // A análise foi iniciada
        }

        // Gatilho para análise de parede
        if (query.contactWallAhead && !query.climbableWallAhead && query.isGrounded)
        {
            StartCoroutine(AnalyzeWallRoutine());
            return NodeState.SUCCESS; // A análise foi iniciada
        }

        return NodeState.FAILURE; // Nenhum gatilho de análise foi ativado
    }

    private NodeState ProcessPatrolLogic()
    {
        var query = _navigation.QueryEnvironment();

        // A PATRULHA REAGE AO AMBIENTE, MAS NÃO INICIA A ANÁLISE
        if (query.isAtLedge || query.contactWallAhead) { _motor.HardStop(); }
        else if (query.dangerLedgeAhead || query.dangerWallAhead) { _motor.Brake(); }
        else if (query.anticipationLedgeAhead || query.anticipationWallAhead) { _motor.Move(patrolTopSpeed / 2); }
        else
        {
            if (query.ceilingAhead && !_motor.IsCrouching) _motor.StartCrouch();
            else if (!query.ceilingAhead && _motor.IsCrouching) _motor.StopCrouch();
            _motor.Move(patrolTopSpeed);
        }
        return NodeState.RUNNING;
    }
    #endregion

    #region HELPER ROUTINES & LOGIC
    private IEnumerator AnalyzeWallRoutine()
    {
        _isAnalyzingWall = true;
        _motor.HardStop();

        _perception.StartObstacleAnalysis(_navigation.contactWallProbeOrigin.position, isLedge: false);
        yield return new WaitForSeconds(wallAnalysisDuration);
        _perception.StopObstacleAnalysis();

        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());
        yield return new WaitForSeconds(0.2f); // Pausa

        // Fase de Recuo Tático Preciso
        float retreatEndTime = Time.time + (wallRetreatDistance / patrolTopSpeed);
        while (Time.time < retreatEndTime)
        {
            _motor.Move(patrolTopSpeed);
            yield return null;
        }
        _motor.HardStop();

        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());

        _isAnalyzingWall = false;
    }

    private IEnumerator AnalyzeLedgeRoutine()
    {
        _isAnalyzingLedge = true;
        _motor.HardStop();
        _perception.StartObstacleAnalysis(_navigation.ledgeProbeOrigin.position, isLedge: true);
        yield return new WaitForSeconds(ledgeAnalysisDuration);

        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());

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