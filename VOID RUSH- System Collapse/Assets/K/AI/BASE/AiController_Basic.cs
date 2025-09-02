// --- SCRIPT: AIController_Basic.cs ---
// Versão: 1.0 "Placeholder"

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AIMotor_Basic))]
public class AIController_Basic : MonoBehaviour
{
    private enum State { Patrolling, Hunting, Attacking, Climbing, Dead }
    private State currentState;

    private AIMotor_Basic motor;
    private Transform playerTarget;
    private bool isFacingRight = true;
    public Transform eyes; // <-- ADICIONE ESTA LINHA
    private bool canAttack = true;

    [Header("▶ STATUS E COMBATE")]
    public float maxHealth = 100f;
    private float currentHealth;
    public float moveSpeed = 3f;
    public float climbSpeed = 3f;
    public float attackDamage = 15f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("▶ PERCEPÇÃO")]
    public float visionRange = 10f;
    [Range(0, 180)] public float visionAngle = 90f; // <-- ADICIONE ESTA LINHA
    public LayerMask playerLayer;
    public LayerMask visionBlockers;

    void Awake()
    {
        motor = GetComponent<AIMotor_Basic>();
    }

    void Start()
    {
        // Encontra o jogador usando AIManager, se existir, senão por tag.
        if (AIManager.Instance != null) {
            playerTarget = AIManager.Instance.playerTarget;
        } else {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerTarget = playerObject.transform;
        }

        isFacingRight = transform.localScale.x > 0;
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
        currentHealth = maxHealth;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        // 1. PERCEPÇÃO
        bool canSeePlayer = CanSeePlayer();
        float distanceToPlayer = (playerTarget != null) ? Vector2.Distance(transform.position, playerTarget.position) : float.MaxValue;

        // 2. TOMADA DE DECISÃO
        if (canSeePlayer) {
            if (distanceToPlayer <= attackRange) {
                ChangeState(State.Attacking);
            } else {
                ChangeState(State.Hunting);
            }
        } else {
            if (currentState != State.Patrolling && currentState != State.Climbing) {
                ChangeState(State.Patrolling);
            }
        }
        
        // 3. EXECUÇÃO
        switch (currentState)
        {
            case State.Patrolling:
                if (motor.IsObstacleAhead() || !motor.IsGroundAhead()) {
                    Flip();
                } else {
                    motor.Move(isFacingRight ? 1 : -1, moveSpeed);
                }
                break;
            case State.Hunting:
                FaceTarget(playerTarget.position);
                if (motor.IsObstacleAhead()) {
                    ChangeState(State.Climbing);
                } else {
                    motor.Move(isFacingRight ? 1 : -1, moveSpeed);
                }
                break;
            case State.Attacking:
                FaceTarget(playerTarget.position);
                motor.Stop();
                if (canAttack) StartCoroutine(AttackCoroutine());
                break;
            case State.Climbing:
                if (!motor.IsObstacleAhead()) {
                    ChangeState(State.Patrolling); // Chegou ao topo, volta a patrulhar/caçar
                } else {
                    motor.Climb(climbSpeed);
                }
                break;
        }
    }

    #region Funções de Suporte
    private bool CanSeePlayer()
    {
        if (playerTarget == null || eyes == null) return false;

        float distanceToPlayer = Vector2.Distance(eyes.position, playerTarget.position);
        if (distanceToPlayer > visionRange) return false;

        Vector2 directionToPlayer = (playerTarget.position - eyes.position).normalized;
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;

        // --- LÓGICA DO CONE DE VISÃO ---
        // 1. Verifica se o jogador está dentro do ângulo de visão.
        if (Vector2.Angle(forward, directionToPlayer) > visionAngle / 2f)
        {
            return false;
        }

        // --- LÓGICA DE LINHA DE VISÃO (RAYCAST) ---
        // 2. Verifica se não há obstáculos bloqueando a visão.
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToPlayer, distanceToPlayer, visionBlockers);

        // Retorna TRUE apenas se o raio não atingiu nada, ou se a primeira coisa que atingiu foi o jogador.
        return hit.collider == null || ((1 << hit.collider.gameObject.layer) & playerLayer) != 0;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        if ((targetPosition.x > transform.position.x && !isFacingRight) || (targetPosition.x < transform.position.x && isFacingRight))
        {
            Flip();
        }
    }

    private IEnumerator AttackCoroutine()
    {
        canAttack = false;
        // Lógica de ataque aqui
        Debug.Log("Atacando!");
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }
    #endregion
    #region Gizmos de Percepção
    void OnDrawGizmosSelected()
    {
        if (eyes == null) return;

        // Desenha o Cone de Visão
        Gizmos.color = Color.yellow;
        Vector3 forward = (Application.isPlaying ? isFacingRight : transform.localScale.x > 0) ? Vector3.right : Vector3.left;
        Vector3 up = Quaternion.Euler(0, 0, visionAngle / 2) * forward;
        Vector3 down = Quaternion.Euler(0, 0, -visionAngle / 2) * forward;

        Gizmos.DrawLine(eyes.position, eyes.position + up * visionRange);
        Gizmos.DrawLine(eyes.position, eyes.position + down * visionRange);

        // Se estiver vendo o jogador, desenha uma linha vermelha até ele.
        if (Application.isPlaying && CanSeePlayer())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyes.position, playerTarget.position);
        }
    }
    #endregion
}