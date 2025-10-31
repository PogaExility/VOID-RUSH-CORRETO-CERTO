using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// SINTAXE CORRIGIDA e NOME DA CLASSE CORRIGIDO
[RequireComponent(typeof(AIPerceptionSystem))]
[RequireComponent(typeof(AIPlatformerMotor))]
[RequireComponent(typeof(AINavigationSystem))]
[RequireComponent(typeof(AIStalkerWallSensor))] // <-- NOME CORRIGIDO
public class AIController_Stalker : MonoBehaviour
{
    #region REFERENCES & STATE
    private AIPerceptionSystem _perception;
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private AIStalkerWallSensor _wallSensor; // <-- NOME CORRIGIDO
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
        _wallSensor = GetComponent<AIStalkerWallSensor>(); // <-- NOME CORRIGIDO
    }

    void Update()
    {
        if (_motor.IsTransitioningState || _isAnalyzing) return;
        ProcessPatrolLogic();
    }

    private void ProcessPatrolLogic()
    {
        if (_motor.IsCrouching)
        {
            if (_navigation.CanStandUp()) { _motor.StopCrouch(); return; }
            var crouchQuery = _navigation.QueryEnvironment();
            switch (crouchQuery.detectedObstacle)
            {
                case AINavigationSystem.ObstacleType.FullWall: StartCoroutine(AnalyzeAndFlipRoutine()); break;
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
                case AINavigationSystem.ObstacleType.FullWall:
                    _motor.HardStop();
                    StartCoroutine(AnalyzeWallAndDecideRoutine());
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

    private IEnumerator AnalyzeWallAndDecideRoutine()
    {
        if (_isAnalyzing) yield break;
        _isAnalyzing = true;
        _motor.HardStop();
        yield return new WaitForSeconds(0.25f);
        var analysis = _wallSensor.AnalyzeWallInFront();
        switch (analysis.TypeOfLedge)
        {
            case LedgeType.HighOpening:
                Debug.Log("SENSOR: Abertura alta detectada. Executando escalada padrão.");
                _motor.StartVault();
                break;
            case LedgeType.LowOpening:
                Debug.Log("SENSOR: Abertura baixa detectada. Executando escalada com agachamento.");
                _motor.StartVaultAndCrouch();
                break;
            case LedgeType.SolidWall:
            default:
                Debug.Log("SENSOR: Parede sólida detectada. Desistindo.");
                _motor.Flip();
                StartCoroutine(FlipCooldownRoutine());
                break;
        }
        _isAnalyzing = false;
    }

    #region Helper Routines and Combat
    private void ProcessCombatLogic()
    {
        // Lógica de combate (pode ser expandida no futuro)
    }

    private IEnumerator AnalyzeAndFlipRoutine()
    {
        if (_isAnalyzing) yield break;
        _isAnalyzing = true;
        _motor.HardStop();
        yield return new WaitForSeconds(wallAnalysisDuration);
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