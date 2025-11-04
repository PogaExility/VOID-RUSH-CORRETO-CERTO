using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AIPlatformerMotor), typeof(AINavigationSystem), typeof(AIStalkerWallSensor))]
public class AIController_Stalker : MonoBehaviour
{
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private AIStalkerWallSensor _wallSensor;
    private bool _isAnalyzing = false;

    [Header("▶ Configuração de Patrulha")]
    public float patrolTopSpeed = 4f;

    void Start()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        _navigation = GetComponentInChildren<AINavigationSystem>();
        _wallSensor = GetComponent<AIStalkerWallSensor>();
    }

    void Update()
    {
        if (_motor.IsTransitioningState || _isAnalyzing) return;

        // A lógica de percepção básica: se vir uma parede, inicie a ANÁLISE.
        var query = _navigation.QueryEnvironment();
        if (query.detectedObstacle == AINavigationSystem.ObstacleType.Wall)
        {
            StartCoroutine(AnalyzeWallAndDecideRoutine());
        }
        else
        {
            _motor.Move(patrolTopSpeed);
        }
    }

    private IEnumerator AnalyzeWallAndDecideRoutine()
    {
        if (_isAnalyzing) yield break;
        _isAnalyzing = true;
        _motor.HardStop();

        Debug.Log("Iniciando análise de parede...");
        yield return new WaitForSeconds(0.2f); // Tempo de "pensamento"

        // PASSO 1: O CÉREBRO PEDE O RELATÓRIO AO SENSOR
        WallAnalysisReport report = _wallSensor.AnalyzeWallInFront();

        // PASSO 2: O CÉREBRO LÊ O RELATÓRIO E TOMA UMA DECISÃO
        if (!report.IsWallDetected)
        {
            Debug.Log("Análise cancelada. Parede desapareceu.");
        }
        else if (report.Opportunities.Count > 0)
        {
            // DECISÃO 1: A parede tem buracos. Use o primeiro que encontrar.
            Debug.Log($"DECISÃO: Parede analisada. {report.Opportunities.Count} buracos encontrados. Escolhendo o primeiro.");
            var chosenHole = report.Opportunities[0];
            bool shouldCrouch = chosenHole.HeightInTiles < 3;
            _motor.ClimbToPosition(chosenHole.EntryPosition, shouldCrouch);
        }
        else if (report.WallHeight < 3.0f) // Parede baixa sem buracos
        {
            // DECISÃO 2: A parede é baixa e não tem buracos. Escale por cima.
            Debug.Log($"DECISÃO: Parede baixa ({report.WallHeight}m) sem buracos. Escalando por cima.");
            _motor.StartVault(report.WallHeight);
        }
        else if (report.WallHeight >= 7.0f) // Parede alta sem buracos
        {
            // DECISÃO 3: A parede é alta e não tem buracos. Inicie o Perch.
            Debug.Log($"DECISÃO: Parede alta ({report.WallHeight}m) sem buracos. Iniciando Perch.");
            float perchHeight = transform.position.y + (report.WallHeight * 0.75f);
            Vector2 perchPosition = new Vector2(transform.position.x, perchHeight);
            _motor.StartPerch(perchPosition);
        }
        else // Parede de altura média sem buracos
        {
            // DECISÃO 4: A parede é intransponível. Vire e desista.
            Debug.Log($"DECISÃO: Parede de altura média ({report.WallHeight}m) sem buracos. Desistindo.");
            _motor.Flip();
        }

        yield return new WaitForSeconds(1.5f); // Cooldown para evitar re-análise imediata
        _isAnalyzing = false;
    }
}