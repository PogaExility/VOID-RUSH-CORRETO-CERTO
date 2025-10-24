using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AIPerceptionSystem), typeof(AIPlatformerMotor), typeof(AINavigationSystem))]
public class AIController_Stalker : MonoBehaviour
{
    #region REFERENCES & STATE
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

    void Start()
    {
        _perception = GetComponent<AIPerceptionSystem>();
        _motor = GetComponent<AIPlatformerMotor>();
        _navigation = GetComponentInChildren<AINavigationSystem>();
    }

    void Update()
    {
        if (_motor.IsTransitioningState || _isAnalyzing) return;
        // Descomente a linha abaixo para reativar a lógica de combate
        // if (_perception != null && _perception.IsAwareOfPlayer) { ProcessCombatLogic(); } else { ProcessPatrolLogic(); }
        ProcessPatrolLogic();
    }

    private void ProcessPatrolLogic()
    {
        if (_motor.IsCrouching)
        {
            if (_navigation.CanStandUp())
            {
                _motor.StopCrouch();
                return;
            }

            var crouchQuery = _navigation.QueryEnvironment();
            switch (crouchQuery.detectedObstacle)
            {
                case AINavigationSystem.ObstacleType.FullWall: StartCoroutine(AnalyzeWallRoutine()); break;
                case AINavigationSystem.ObstacleType.Ledge: StartCoroutine(AnalyzeLedgeRoutine()); break;
                default: _motor.Move(patrolTopSpeed / 2); break;
            }
        }
        else
        {
            var query = _navigation.QueryEnvironment();
            switch (query.detectedObstacle)
            {
                case AINavigationSystem.ObstacleType.CrouchTunnel:
                    _motor.StartCrouch();
                    _motor.Move(patrolTopSpeed / 2);
                    break;
                case AINavigationSystem.ObstacleType.JumpablePlatform:
                    _motor.HardStop();
                    _motor.StartVault();
                    break;
                case AINavigationSystem.ObstacleType.FullWall:
                    StartCoroutine(AnalyzeWallRoutine());
                    break;
                case AINavigationSystem.ObstacleType.DroppableLedge:
                    _motor.Move(patrolTopSpeed);
                    break;
                case AINavigationSystem.ObstacleType.Ledge:
                    StartCoroutine(AnalyzeLedgeRoutine());
                    break;
                case AINavigationSystem.ObstacleType.None:
                default:
                    _motor.Move(patrolTopSpeed);
                    break;
            }
        }
    }

    #region Helper Routines and Combat
    private void ProcessCombatLogic()
    {
        if (_player == null) return;
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
                case AINavigationSystem.ObstacleType.FullWall: _motor.HardStop(); break;
                case AINavigationSystem.ObstacleType.JumpablePlatform: _motor.StartVault(); break; // Deve escalar em combate também
                case AINavigationSystem.ObstacleType.Ledge: _motor.HardStop(); break;
                default: _motor.Move(huntTopSpeed); break;
            }
        }
    }

    private IEnumerator AnalyzeWallRoutine()
    {
        if (_isAnalyzing) yield break;
        _isAnalyzing = true;
        _motor.HardStop();
        if (_perception != null) _perception.StartObstacleAnalysis(_navigation.Probe_Height_1_Base.position, isLedge: false);
        yield return new WaitForSeconds(wallAnalysisDuration);
        if (_perception != null) _perception.StopObstacleAnalysis();
        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());
        _isAnalyzing = false;
    }

    private IEnumerator AnalyzeLedgeRoutine()
    {
        if (_isAnalyzing) yield break;
        _isAnalyzing = true;
        _motor.HardStop();
        if (_perception != null) _perception.StartObstacleAnalysis(_navigation.Probe_Ledge_Check.position, isLedge: true);
        yield return new WaitForSeconds(ledgeAnalysisDuration);
        if (_perception != null) _perception.StopObstacleAnalysis();
        _motor.Flip();
        StartCoroutine(FlipCooldownRoutine());
        _isAnalyzing = false;
    }

    // ==================================================================
    // FUNÇÕES COMPLETAS E CORRIGIDAS
    // ==================================================================
    private void FaceTarget(Vector3 targetPosition)
    {
        if (_player == null) return;
        float dotProduct = Vector2.Dot((targetPosition - transform.position).normalized, transform.right);
        if (_canFlip && dotProduct < -0.1f)
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