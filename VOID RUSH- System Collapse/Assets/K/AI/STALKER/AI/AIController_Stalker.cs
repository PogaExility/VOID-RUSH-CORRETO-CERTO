using UnityEngine;
using BehaviorTree;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AIPerceptionSystem), typeof(AIPlatformerMotor), typeof(AINavigationSystem))]
public class AIController_Stalker : MonoBehaviour
{
    private Node _rootNode;
    private AIPerceptionSystem _perception;
    private AIPlatformerMotor _motor;
    private AINavigationSystem _navigation;
    private Transform _player;

    [Header("▶ Configuração de Combate")]
    public float moveSpeed = 4f;
    public float huntSpeed = 6f;
    public float jumpForce = 15f;
    public float attackRange = 1.5f;

    [Header("▶ Lógica de Patrulha")]
    public float flipCooldown = 0.5f;
    private bool _canFlip = true;

    void Start()
    {
        _perception = GetComponent<AIPerceptionSystem>();
        _motor = GetComponent<AIPlatformerMotor>();
        _navigation = GetComponent<AINavigationSystem>();
        _player = AIManager.Instance.playerTarget;

        _rootNode = new Selector(new List<Node>
        {
            new Sequence(new List<Node>
            {
                new ActionNode(CheckIfAwareOfPlayer),
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node>
                    {
                        new ActionNode(CheckIfInAttackRange),
                        new ActionNode(PerformAttack)
                    }),
                    new ActionNode(HuntPlayer)
                })
            }),
            new ActionNode(Patrol)
        });
    }

    void Update()
    {
        _rootNode?.Evaluate();
    }

    #region Nós da Árvore de Comportamento
    private NodeState CheckIfAwareOfPlayer() => _perception.IsAwareOfPlayer ? NodeState.SUCCESS : NodeState.FAILURE;
    private NodeState CheckIfInAttackRange() => Vector2.Distance(transform.position, _player.position) <= attackRange ? NodeState.SUCCESS : NodeState.FAILURE;

    private NodeState PerformAttack()
    {
        FaceTarget(_player.position);
        _motor.Stop();
        return NodeState.SUCCESS;
    }

    private NodeState HuntPlayer()
    {
        FaceTarget(_player.position);
        _navigation.Navigate(huntSpeed, jumpForce);
        return NodeState.RUNNING;
    }

    private NodeState Patrol()
    {
        // A CORREÇÃO CRÍTICA ESTÁ AQUI.
        // A decisão de virar-se e a de mover-se estão agora no mesmo fluxo.
        if (_canFlip && (_navigation.IsPathBlocked() || _navigation.IsFacingEdge()))
        {
            _motor.Flip();
            StartCoroutine(FlipCooldownRoutine());
            // Após se virar, ele não para de pensar. Ele prossegue.
        }

        // Independentemente de se ter virado ou não, a sua ação padrão é navegar.
        _navigation.Navigate(moveSpeed, jumpForce);
        return NodeState.RUNNING; // Patrulhar é sempre uma ação contínua.
    }
    #endregion

    #region Lógica de "Flip" e Cooldown
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