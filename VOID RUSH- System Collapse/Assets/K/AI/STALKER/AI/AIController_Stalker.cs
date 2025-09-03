using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;

// CORREÇÃO: Um atributo [RequireComponent] para cada tipo.
[RequireComponent(typeof(AIPerceptionSystem))]
[RequireComponent(typeof(AIPlatformerMotor))]
[RequireComponent(typeof(AINavigationSystem))]
[RequireComponent(typeof(AIThreatAdaptationModule))]
public class AIController_Stalker : MonoBehaviour
{
    #region REFERENCES
    private Node _rootNode;
    private AIPerceptionSystem _perception;
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private AIThreatAdaptationModule _adaptation;
    #endregion

    #region CONFIGURATION
    [Header("▶ Configuração de Combate")]
    public float moveSpeed = 4f;
    public float huntSpeed = 6f;
    public float jumpForce = 15f;
    public float attackRange = 1.5f;
    #endregion

    void Start()
    {
        _perception = GetComponent<AIPerceptionSystem>();
        _motor = GetComponent<AIPlatformerMotor>();
        _navigation = GetComponent<AINavigationSystem>();
        _adaptation = GetComponent<AIThreatAdaptationModule>();

        // --- CONSTRUÇÃO DA ÁRVORE DE COMPORTAMENTO MESTRA ---
        _rootNode = new Selector(new List<Node>
        {
            // --- Ramo de Combate Tático (Prioridade Máxima) ---
            new Sequence(new List<Node>
            {
                new ActionNode(CheckIfAwareOfPlayer), // A IA só combate se estiver ciente do jogador
                new Selector(new List<Node> // A IA escolherá UMA destas ações de combate
                {
                    // Lógica de ataque corpo a corpo
                    new Sequence(new List<Node>
                    {
                        new ActionNode(CheckIfInAttackRange),
                        new ActionNode(PerformAttack)
                    }),
                    // Lógica de caça
                    new ActionNode(HuntPlayer)
                })
            }),

            // --- Ramo de Investigação (Prioridade Média) ---
            new Sequence(new List<Node>
            {
                new ActionNode(CheckIfSuspicious),
                new ActionNode(InvestigateLocation)
            }),

            // --- Comportamento Padrão de Patrulha (Prioridade Mínima) ---
            new ActionNode(Patrol)
        });
    }

    void Update()
    {
        if (_rootNode != null)
        {
            _rootNode.Evaluate();
        }
    }

    #region BEHAVIOR TREE NODES (Checks / Conditionals)

    private NodeState CheckIfAwareOfPlayer()
    {
        return _perception.IsAwareOfPlayer ? NodeState.SUCCESS : NodeState.FAILURE;
    }

    private NodeState CheckIfInAttackRange()
    {
        // Usa a posição real do jogador se estiver caçando, para um ataque mais preciso
        Vector3 targetPosition = AIManager.Instance.playerTarget.position;
        float distance = Vector2.Distance(transform.position, targetPosition);
        return distance <= attackRange ? NodeState.SUCCESS : NodeState.FAILURE;
    }

    private NodeState CheckIfSuspicious()
    {
        return _perception.currentAwareness == AIPerceptionSystem.AwarenessState.SUSPICIOUS ? NodeState.SUCCESS : NodeState.FAILURE;
    }

    #endregion

    #region BEHAVIOR TREE NODES (Actions)

    private NodeState PerformAttack()
    {
        _motor.Stop();
        _motor.FaceTarget(AIManager.Instance.playerTarget.position);
        Debug.Log("STALKER: Atacando!");
        // --- Chame aqui a sua lógica de ataque ---
        return NodeState.SUCCESS;
    }

    // --- MÉTODO ATUALIZADO ---
    private NodeState HuntPlayer()
    {
        // ADICIONADO: Garante que o Stalker sempre encare o alvo enquanto caça.
        _motor.FaceTarget(_perception.LastKnownPlayerPosition);

        // A lógica de navegação continua a mesma.
        _navigation.NavigateTowards(_perception.LastKnownPlayerPosition, huntSpeed, jumpForce);

        return NodeState.RUNNING; // A caçada é uma ação contínua.
    }

    private NodeState InvestigateLocation()
    {
        float distanceToLKP = Vector2.Distance(transform.position, _perception.LastKnownPlayerPosition);

        // Se chegou perto o suficiente do ponto de suspeita
        if (distanceToLKP < 1f)
        {
            _motor.Stop();
            // --- Lógica de "procurar" no local ---
            // Ex: animator.SetTrigger("LookAround");
            // Por enquanto, ele apenas para e a awareness vai diminuir com o tempo.
            return NodeState.SUCCESS; // A ação de "investigar o ponto" foi concluída
        }

        // Se ainda não chegou, continua se movendo
        _navigation.NavigateTowards(_perception.LastKnownPlayerPosition, moveSpeed, jumpForce);
        return NodeState.RUNNING; // RUNNING porque a ação de se mover leva tempo
    }

    private NodeState Patrol()
    {
        // Lógica de patrulha simples baseada em "bater e virar"
        if (_navigation.IsPathBlocked() || _navigation.IsFacingEdge())
        {
            _motor.Flip();
        }
        _motor.Move(moveSpeed);
        return NodeState.RUNNING; // RUNNING porque patrulhar é uma ação contínua
    }

    #endregion
}