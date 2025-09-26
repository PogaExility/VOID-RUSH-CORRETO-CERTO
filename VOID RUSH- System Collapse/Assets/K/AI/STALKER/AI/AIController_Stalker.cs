using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AIPerceptionSystem), typeof(AIPlatformerMotor), typeof(AINavigationSystem))]
public class AIController_Stalker : MonoBehaviour
{
    #region REFERÊNCIAS E ESTADO
    private AIPerceptionSystem _perception;
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private Transform _player;
    private bool _isAnalyzing = false;
    #endregion

    #region CONFIGURAÇÃO DE COMPORTAMENTO
    [Header("▶ Atributos Gerais")]
    public float patrolTopSpeed = 4f;
    public float jumpForce = 15f;

    [Header("▶ LÓGICA DE PROXIMIDADE")]
    [Tooltip("Distância em que o Stalker começa a desacelerar.")]
    public float dangerDistance = 2.5f;
    [Tooltip("Distância em que o Stalker para totalmente e analisa (ex: 1 tile = 1.0f).")]
    public float contactDistance = 1.0f;

    [Header("▶ Lógica de Análise")]
    public float analysisDuration = 1.5f;
    #endregion

    void Start()
    {
        _perception = GetComponent<AIPerceptionSystem>();
        _motor = GetComponent<AIPlatformerMotor>();
        _navigation = GetComponent<AINavigationSystem>();
        // _player = AIManager.Instance.playerTarget; // Descomente se tiver um AIManager
    }

    void Update()
    {
        if (_isAnalyzing || _motor.IsTransitioningState) return;
        ProcessPatrolLogic();
    }

    private void ProcessPatrolLogic()
    {
        var query = _navigation.QueryEnvironment();

        switch (query.detectedObstacle)
        {
            case AINavigationSystem.ObstacleType.None:
                if (_motor.IsCrouching && _navigation.CanStandUp())
                {
                    _motor.StopCrouch();
                }
                _motor.Move(patrolTopSpeed);
                break;

            case AINavigationSystem.ObstacleType.CrouchTunnel:
                _motor.StartCrouch();
                _motor.Move(patrolTopSpeed / 2);
                break;

            case AINavigationSystem.ObstacleType.JumpableWall:
                if (query.distanceToObstacle <= contactDistance * 1.5f)
                {
                    _motor.Jump(jumpForce);
                }
                else
                {
                    _motor.Move(patrolTopSpeed);
                }
                break;

            case AINavigationSystem.ObstacleType.FullWall:
            case AINavigationSystem.ObstacleType.Ledge:
                HandleProximityObstacle(query);
                break;
        }
    }

    private void HandleProximityObstacle(AINavigationSystem.NavQueryResult query)
    {
        if (query.distanceToObstacle <= contactDistance)
        {
            StartCoroutine(AnalyzeObstacleRoutine(query.detectedObstacle == AINavigationSystem.ObstacleType.Ledge));
        }
        else if (query.distanceToObstacle <= dangerDistance)
        {
            _motor.Brake();
        }
        else
        {
            _motor.Move(patrolTopSpeed);
        }
    }

    private IEnumerator AnalyzeObstacleRoutine(bool isLedge)
    {
        if (_isAnalyzing) yield break;
        _isAnalyzing = true;

        while (_motor._currentSpeed > 0.01f)
        {
            _motor.Brake();
            yield return null;
        }
        _motor.HardStop();

        Debug.Log("Analisando " + (isLedge ? "Borda" : "Parede"));
        yield return new WaitForSeconds(analysisDuration);

        _motor.Flip();
        _isAnalyzing = false;
    }
}