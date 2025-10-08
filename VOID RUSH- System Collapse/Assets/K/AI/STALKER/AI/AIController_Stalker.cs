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
        _navigation = GetComponent<AINavigationSystem>();
        // _player = AIManager.Instance.playerTarget;
    }

    void Update()
    {
        if (_motor.IsTransitioningState || _isAnalyzing) return;
        ProcessPatrolLogic();
    }

    private void ProcessPatrolLogic()
    {
        // =====================================================================================
        // SUA LÓGICA DO "INTERRUPTOR" (SWITCH) IMPLEMENTADA
        // =====================================================================================

        // --- NÍVEL 2: O PILOTO AUTOMÁTICO (SE JÁ ESTIVER AGACHADO) ---
        if (_motor.IsCrouching)
        {
            // A única pergunta é: O interruptor de segurança desligou? (CS == off?)
            if (_navigation.CanStandUp())
            {
                // SIM: O interruptor "Agachar" é desligado. A IA se levanta.
                _motor.StopCrouch();
            }
            else
            {
                // NÃO: O interruptor continua ligado. Continue no modo túnel.
                // Verificamos se há paredes dentro do túnel.
                var crouchQuery = _navigation.QueryEnvironment();
                if (crouchQuery.detectedObstacle == AINavigationSystem.ObstacleType.FullWall)
                {
                    StartCoroutine(AnalyzeWallRoutine());
                }
                else
                {
                    _motor.Move(patrolTopSpeed / 2);
                }
            }
            return; // A decisão para este frame está tomada.
        }

        // --- NÍVEL 1: O GATILHO DE ENTRADA (SE ESTIVER EM PÉ) ---
        // Este código só é executado se a IA não estiver agachada.
        var query = _navigation.QueryEnvironment();

        // O veredito "CrouchTunnel" do sistema nervoso representa a sua condição:
        // (wall top e ceiling == on)
        if (query.detectedObstacle == AINavigationSystem.ObstacleType.CrouchTunnel)
        {
            // A condição implícita (CS == off) é que _motor.IsCrouching é falso.
            // O interruptor "Agachar" é ligado. A IA começa a agachar.
            _motor.StartCrouch();
            _motor.Move(patrolTopSpeed / 2);
        }
        else
        {
            // Se o gatilho de agachar não for ativado, execute a lógica de patrulha normal.
            switch (query.detectedObstacle)
            {
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
                    _motor.Move(patrolTopSpeed);
                    break;
            }
        }
    }

    #region Helper Routines and Combat
    // ... (As outras funções, como ProcessCombatLogic, AnalyzeWallRoutine, etc., permanecem as mesmas) ...
    private void ProcessCombatLogic() {/*...*/}
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