using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviorTree; // Namespace dos seus nós (Selector, ActionNode, etc)

[RequireComponent(typeof(AIPlatformerMotor), typeof(AINavigationSystem), typeof(AIStalkerWallSensor))]
public class AIController_Stalker : MonoBehaviour
{
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private AIStalkerWallSensor _wallSensor;

    // --- BT & Pathfinding ---
    private Node _rootNode;
    private List<PathNode> _path;
    private int _pathIndex;

    public Transform targetPlayer; // ARRASTE O PLAYER AQUI
    public bool _isAnalyzing = false;

    [Header("▶ Configuração")]
    public float patrolTopSpeed = 4f;
    public float chaseTopSpeed = 6f;

    void Start()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        _navigation = GetComponentInChildren<AINavigationSystem>();
        _wallSensor = GetComponent<AIStalkerWallSensor>();

        // Tenta achar player automaticamente se não atribuído
        if (targetPlayer == null && GameObject.FindGameObjectWithTag("Player"))
            targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;

        ConstructBehaviorTree();
    }

    void Update()
    {
        if (_rootNode != null)
            _rootNode.Evaluate();
    }

    void ConstructBehaviorTree()
    {
        // A Árvore de Decisão:
        // 1. Tenta resolver obstáculos imediatos (Paredes/Parkour)
        // 2. Se não tiver obstáculos, Tenta perseguir o jogador (Pathfinding)
        // 3. Se não tiver caminho, Patrulha (Idle/Fallback)

        _rootNode = new Selector(new List<Node>
        {
            new ActionNode(HandleImmediateObstacles),
            new ActionNode(ChasePlayerWithPathfinding),
            new ActionNode(PatrolFallback)
        });
    }

    // --- AÇÃO 1: PARKOUR & ANÁLISE TÁTICA ---
    NodeState HandleImmediateObstacles()
    {
        if (_motor.IsTransitioningState || _isAnalyzing) return NodeState.RUNNING;

        var query = _navigation.QueryEnvironment();

        // Se encontrar uma parede, pare tudo e analise
        if (query.detectedObstacle == AINavigationSystem.ObstacleType.Wall)
        {
            StartCoroutine(AnalyzeWallAndDecideRoutine());
            return NodeState.RUNNING; // Está lidando com isso
        }

        return NodeState.FAILURE; // Sem obstáculos, continua para perseguição
    }

    // --- AÇÃO 2: PERSEGUIÇÃO INTELIGENTE (A*) ---
    NodeState ChasePlayerWithPathfinding()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (_isAnalyzing || _motor.IsTransitioningState) return NodeState.RUNNING;

        // Recalcula caminho se: Não existe OU Jogador se moveu muito longe do destino final
        if (_path == null || _path.Count == 0 || Vector3.Distance(_path[_path.Count - 1].worldPosition, targetPlayer.position) > 2.0f)
        {
            _path = Pathfinder.FindPath(transform.position, targetPlayer.position);
            _pathIndex = 0;
        }

        if (_path == null || _path.Count == 0)
        {
            // Caminho bloqueado ou impossível
            return NodeState.FAILURE;
        }

        // Segue o Caminho
        if (_pathIndex < _path.Count)
        {
            PathNode currentNode = _path[_pathIndex];

            // Debug Visual do Caminho
            for (int i = 0; i < _path.Count - 1; i++)
                Debug.DrawLine(_path[i].worldPosition, _path[i + 1].worldPosition, Color.cyan);

            // Comanda o motor para ir ao nó
            _motor.MoveTo(currentNode.worldPosition, chaseTopSpeed);

            // Checa proximidade para avançar índice
            if (Vector3.Distance(transform.position, currentNode.worldPosition) < 0.5f)
            {
                _pathIndex++;
            }
        }
        else
        {
            // Chegou ao fim do caminho
            _motor.HardStop();
        }

        return NodeState.RUNNING;
    }

    // --- AÇÃO 3: FALLBACK ---
    NodeState PatrolFallback()
    {
        // Comportamento simples se não puder ir até o jogador
        _motor.HardStop();
        return NodeState.SUCCESS;
    }

    // --- SUA LÓGICA ORIGINAL DE ANÁLISE (MANTIDA) ---
    private IEnumerator AnalyzeWallAndDecideRoutine()
    {
        if (_isAnalyzing) yield break;
        _isAnalyzing = true;
        _motor.HardStop();

        Debug.Log("Iniciando análise de parede...");
        yield return new WaitForSeconds(0.2f); // Tempo de "pensamento"

        WallAnalysisReport report = _wallSensor.AnalyzeWallInFront();

        if (!report.IsWallDetected)
        {
            Debug.Log("Análise cancelada. Parede desapareceu.");
        }
        else if (report.Opportunities.Count > 0)
        {
            Debug.Log($"DECISÃO: Usando buraco na parede.");
            var chosenHole = report.Opportunities[0];
            bool shouldCrouch = chosenHole.HeightInTiles < 3;
            _motor.ClimbToPosition(chosenHole.EntryPosition, shouldCrouch);
        }
        else if (report.WallHeight < 3.0f)
        {
            Debug.Log($"DECISÃO: Escalando parede baixa.");
            _motor.StartVault(report.WallHeight);
        }
        else if (report.WallHeight >= 7.0f)
        {
            Debug.Log($"DECISÃO: Perch em parede alta.");
            float perchHeight = transform.position.y + (report.WallHeight * 0.75f);
            Vector2 perchPosition = new Vector2(transform.position.x, perchHeight);
            _motor.StartPerch(perchPosition);
        }
        else
        {
            Debug.Log($"DECISÃO: Desistindo.");
            _motor.Flip();
        }

        yield return new WaitForSeconds(1.5f);
        _isAnalyzing = false;
    }
}