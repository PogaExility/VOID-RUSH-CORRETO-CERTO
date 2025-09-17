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
            new ActionNode(ProcessAnalysisTriggers),
            new ActionNode(ProcessClimbingLogic),
            new Sequence(new List<Node>
            {
                new ActionNode(CheckIfAwareOfPlayer),
                new ActionNode(ProcessCombatLogic)
            }),
            new ActionNode(ProcessPatrolLogic)
        });
        // --- DEBUG ---
        Debug.Log("[CONTROLLER] Start: Cérebro inicializado e Árvore de Comportamento construída.");
    }

    void Update()
    {
        // --- DEBUG ---
        Debug.Log($"--- FRAME START (IsAnalyzing: {_isAnalyzing}) ---");
        if (!_isAnalyzing)
        {
            _rootNode?.Evaluate();
        }
        else
        {
            // --- DEBUG ---
            Debug.LogWarning("[CONTROLLER] Update SKIPPED: Análise em progresso.");
        }
    }
    #endregion

    #region BEHAVIOR TREE NODES
    private NodeState CheckIfAwareOfPlayer()
    {
        // --- DEBUG ---
        bool isAware = _perception.IsAwareOfPlayer;
        Debug.Log($"[CONTROLLER] Evaluating Node: CheckIfAwareOfPlayer. Result: {isAware}");
        return isAware ? NodeState.SUCCESS : NodeState.FAILURE;
    }

    private NodeState ProcessClimbingLogic() { /* Implementação futura */ return NodeState.FAILURE; }

    private NodeState ProcessCombatLogic()
    {
        // --- DEBUG ---
        Debug.Log("[CONTROLLER] Evaluating Node: ProcessCombatLogic");
        FaceTarget(_player.position);
        if (Vector2.Distance(transform.position, _player.position) <= attackRange)
        {
            // --- DEBUG ---
            Debug.Log("[CONTROLLER-Combat] In attack range. Stopping.");
            _motor.Stop();
        }
        else
        {
            var query = _navigation.QueryEnvironment();
            if (query.isAtLedge || query.dangerLedgeAhead)
            {
                // --- DEBUG ---
                Debug.Log("[CONTROLLER-Combat] Ledge ahead while hunting. Hard Stopping.");
                _motor.HardStop();
            }
            else if (query.anticipationLedgeAhead)
            {
                // --- DEBUG ---
                Debug.Log("[CONTROLLER-Combat] Anticipating ledge while hunting. Braking.");
                _motor.Brake();
            }
            else
            {
                if (query.climbableWallAhead)
                {
                    // --- DEBUG ---
                    Debug.Log("[CONTROLLER-Combat] Climbable wall detected. Starting climb.");
                    _motor.StartClimb();
                }
                else if (query.contactWallAhead)
                {
                    // --- DEBUG ---
                    Debug.Log("[CONTROLLER-Combat] Wall ahead. Jumping.");
                    _motor.Jump(jumpForce);
                }
                else
                {
                    _motor.Move(huntTopSpeed);
                }
            }
        }
        return NodeState.RUNNING;
    }

    private NodeState ProcessAnalysisTriggers()
    {
        // --- DEBUG ---
        Debug.Log("[CONTROLLER] Evaluating Node: ProcessAnalysisTriggers");
        var query = _navigation.QueryEnvironment();
        if (query.isAtLedge)
        {
            // --- DEBUG ---
            Debug.LogWarning("[CONTROLLER-Analysis] TRIGGERED Ledge Analysis.");
            StartCoroutine(AnalyzeLedgeRoutine());
            return NodeState.SUCCESS;
        }
        if (query.contactWallAhead && !query.climbableWallAhead && query.isGrounded)
        {
            // --- DEBUG ---
            Debug.LogWarning("[CONTROLLER-Analysis] TRIGGERED Wall Analysis.");
            StartCoroutine(AnalyzeWallRoutine());
            return NodeState.SUCCESS;
        }
        return NodeState.FAILURE;
    }

    private NodeState ProcessPatrolLogic()
    {
        // --- DEBUG ---
        Debug.Log("[CONTROLLER] Evaluating Node: ProcessPatrolLogic");
        var query = _navigation.QueryEnvironment();
        if (query.dangerLedgeAhead || query.dangerWallAhead)
        {
            // --- DEBUG ---
            Debug.Log("[CONTROLLER-Patrol] Danger ahead. Braking.");
            _motor.Brake();
        }
        else if (query.anticipationLedgeAhead || query.anticipationWallAhead)
        {
            // --- DEBUG ---
            Debug.Log("[CONTROLLER-Patrol] Anticipation ahead. Slowing down.");
            _motor.Move(patrolTopSpeed / 2);
        }
        else
        {
            if (query.ceilingAhead && !_motor.IsCrouching)
            {
                // --- DEBUG ---
                Debug.Log("[CONTROLLER-Patrol] Ceiling ahead. Crouching.");
                _motor.StartCrouch();
            }
            else if (!query.ceilingAhead && _motor.IsCrouching)
            {
                // --- DEBUG ---
                Debug.Log("[CONTROLLER-Patrol] Ceiling clear. Stopping crouch.");
                _motor.StopCrouch();
            }
            _motor.Move(patrolTopSpeed);
        }
        return NodeState.RUNNING;
    }
    #endregion

    #region HELPER ROUTINES & LOGIC
    private IEnumerator AnalyzeWallRoutine()
    {
        // --- DEBUG ---
        Debug.LogWarning("[CONTROLLER-Analysis] Starting Wall Analysis Coroutine. Setting _isAnalyzing = true.");
        _isAnalyzing = true;
        _motor.HardStop();
        _perception.StartObstacleAnalysis(_navigation.contactWallProbeOrigin.position, isLedge: false);
        yield return new WaitForSeconds(wallAnalysisDuration);

        // --- DEBUG ---
        Debug.Log("[CONTROLLER-Analysis] Wall analysis time ended. Flipping.");
        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());
        yield return new WaitForSeconds(0.2f);

        float retreatEndTime = Time.time + (wallRetreatDistance / patrolTopSpeed);
        while (Time.time < retreatEndTime)
        {
            _motor.Move(patrolTopSpeed);
            yield return null;
        }
        _motor.HardStop();

        // --- DEBUG ---
        Debug.Log("[CONTROLLER-Analysis] Retreat finished. Flipping back.");
        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());
        _perception.StopObstacleAnalysis();
        // --- DEBUG ---
        Debug.LogWarning("[CONTROLLER-Analysis] FINISHED Wall Analysis Coroutine. Setting _isAnalyzing = false.");
        _isAnalyzing = false;
    }

    private IEnumerator AnalyzeLedgeRoutine()
    {
        // --- DEBUG ---
        Debug.LogWarning("[CONTROLLER-Analysis] Starting Ledge Analysis Coroutine. Setting _isAnalyzing = true.");
        _isAnalyzing = true;
        _motor.HardStop();
        _perception.StartObstacleAnalysis(_navigation.ledgeProbeOrigin.position, isLedge: true);
        yield return new WaitForSeconds(ledgeAnalysisDuration);

        // --- DEBUG ---
        Debug.Log("[CONTROLLER-Analysis] Ledge analysis time ended. Flipping.");
        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());
        _perception.StopObstacleAnalysis();

        // --- DEBUG ---
        Debug.LogWarning("[CONTROLLER-Analysis] FINISHED Ledge Analysis Coroutine. Setting _isAnalyzing = false.");
        _isAnalyzing = false;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        float dotProduct = Vector2.Dot((targetPosition - transform.position).normalized, transform.right);
        if (_canFlip && dotProduct < -0.5f)
        {
            // --- DEBUG ---
            Debug.Log("[CONTROLLER-Helper] FaceTarget triggered a flip.");
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