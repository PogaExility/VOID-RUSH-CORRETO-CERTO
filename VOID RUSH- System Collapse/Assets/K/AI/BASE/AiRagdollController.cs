using UnityEngine;

public class AIRagdollController : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Attacking }
    private State currentState;

    [Header("Referências")]
    public Transform playerTarget;
    public AIProceduralAnimator proceduralAnimator;
    [Tooltip("O Rigidbody2D do corpo principal, geralmente o Torso.")]
    public Rigidbody2D mainBodyRb; // Usado para medir distância

    [Header("Parâmetros")]
    public float moveSpeed = 3f;
    public float detectionRange = 15f;
    public float attackRange = 4f;

    void Start()
    {
        // Tenta encontrar as referências automaticamente
        if (playerTarget == null) playerTarget = AIManager.Instance?.playerTarget;
        if (proceduralAnimator == null) proceduralAnimator = GetComponent<AIProceduralAnimator>();

        // Verificações de segurança
        if (playerTarget == null) Debug.LogError("Jogador (Player Target) não encontrado! Verifique o AIManager e a tag do Player.", this);
        if (proceduralAnimator == null) Debug.LogError("Animator Procedural não encontrado! Verifique se o script está no mesmo objeto.", this);
        if (mainBodyRb == null) Debug.LogError("Rigidbody Principal (Main Body Rb) não foi atribuído no Inspector!", this);
    }

    void Update()
    {
        if (playerTarget == null || mainBodyRb == null) return;
        UpdateState();
    }

    void FixedUpdate()
    {
        if (playerTarget == null || proceduralAnimator == null || mainBodyRb == null) return;

        Vector2 moveDirection = Vector2.zero;
        switch (currentState)
        {
            case State.Patrolling:
                // Em patrulha, ele se move para a direita por padrão
                moveDirection = new Vector2(1, 0) * moveSpeed;
                break;
            case State.Chasing:
                // Persegue o jogador
                float direction = (playerTarget.position.x > mainBodyRb.position.x) ? 1 : -1;
                moveDirection = new Vector2(direction, 0) * moveSpeed;
                break;
            case State.Attacking:
                // Para de se mover quando está atacando
                moveDirection = Vector2.zero;
                break;
        }
        proceduralAnimator.SetMoveIntention(moveDirection);
    }

    void UpdateState()
    {
        if (IsPlayerInAttackRange()) currentState = State.Attacking;
        else if (IsPlayerInDetectionRange()) currentState = State.Chasing;
        else currentState = State.Patrolling;
    }

    private bool IsPlayerInDetectionRange() => Vector2.Distance(mainBodyRb.position, playerTarget.position) < detectionRange;
    private bool IsPlayerInAttackRange() => Vector2.Distance(mainBodyRb.position, playerTarget.position) < attackRange;
}