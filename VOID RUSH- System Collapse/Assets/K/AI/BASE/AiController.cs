using UnityEngine;
using System.Collections;

// Versão FINAL com lógica de navegação segura e Gizmos restaurados.
[RequireComponent(typeof(AIPlatformerMotor))]
public class AIController : MonoBehaviour
{
    #region State Machine and References
    private enum State { Patrolling, Hunting, Attacking, Searching }
    private State currentState;
    private AIPlatformerMotor motor;
    private Transform playerTarget;
    public Transform eyes;
    #endregion

    #region Vision and Memory Parameters
    [Header("Parâmetros de Visão")]
    public float visionRange = 15f;
    [Range(0, 180)] public float visionAngle = 90f;
    public LayerMask visionBlockers;
    public LayerMask playerLayer;
    private Vector2 lookDirection;
    public float lookSpeed = 4f;

    [Header("Consciência Situacional")]
    [Range(0, 90)] public float upwardLookAngle = 45f;
    [Range(0, 90)] public float downwardLookAngle = 45f;
    public float platformCheckHeight = 5f;

    [Header("Memória e Busca")]
    public float searchTime = 5f;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;
    #endregion

    #region Combat Parameters
    [Header("Combate Corpo a Corpo")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;
    #endregion

    #region Navigation Parameters
    [Header("Navegação")]
    public float jumpForce = 14f;
    public float airControlForce = 5f;
    public float maxJumpDistance = 5f;
    private bool isFacingRight = true;
    private bool isExecutingAction = false;
    #endregion

    #region Unity Lifecycle Methods
    void Awake() { motor = GetComponent<AIPlatformerMotor>(); }

    void Start()
    {
        playerTarget = AIManager.Instance?.playerTarget;
        isFacingRight = transform.localScale.x > 0;
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
        lookDirection = isFacingRight ? Vector2.right : Vector2.left;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (playerTarget == null) { FindPlayer(); return; }
        HandleHeadLook();
        DecideState();
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;
        ExecuteBrain();
    }
    #endregion

    #region Core AI Logic (Separated Brains)
    void DecideState()
    {
        if (isExecutingAction || !motor.IsGrounded()) return;

        bool canSeePlayer = CanSeePlayer();
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = playerTarget.position;
            if (currentState != State.Hunting && currentState != State.Attacking) ChangeState(State.Hunting);
        }

        switch (currentState)
        {
            case State.Hunting:
                if (canSeePlayer) { if (IsPlayerInAttackRange()) ChangeState(State.Attacking); }
                else { ChangeState(State.Searching); }
                break;
            case State.Attacking:
                if (canSeePlayer) { if (!IsPlayerInAttackRange()) ChangeState(State.Hunting); }
                else { ChangeState(State.Searching); }
                break;
            case State.Searching:
                if (Vector2.Distance(transform.position, lastKnownPlayerPosition) < 1f)
                {
                    searchTimer -= Time.deltaTime;
                    if (searchTimer <= 0) ChangeState(State.Patrolling);
                }
                break;
        }
    }

    void ExecuteBrain()
    {
        if (isExecutingAction) { motor.Stop(); return; }

        // Se estiver no ar, aplica controle aéreo e encerra
        if (!motor.IsGrounded())
        {
            float airMove = 0f;
            if (currentState == State.Hunting || currentState == State.Searching)
            {
                airMove = Mathf.Sign(playerTarget.position.x - transform.position.x);
            }
            motor.Move(airMove * 0.5f);
            return;
        }

        Vector3 targetPosition = transform.position;
        bool doAttack = false;

        // 1. Cérebro Tático: Decide ONDE quer estar.
        switch (currentState)
        {
            case State.Hunting: targetPosition = playerTarget.position; break;
            case State.Patrolling: targetPosition = transform.position + (transform.right * (isFacingRight ? 1 : -1) * 5f); break;
            case State.Searching: targetPosition = lastKnownPlayerPosition; break;
            case State.Attacking: doAttack = true; break;
        }

        // 2. Cérebro de Navegação: Calcula o próximo passo seguro.
        float moveInput = 0;
        if (currentState != State.Attacking && Mathf.Abs(targetPosition.x - transform.position.x) > 0.5f)
        {
            moveInput = Mathf.Sign(targetPosition.x - transform.position.x);
            FaceDirection(moveInput);
        }

        if (moveInput != 0)
        {
            if (motor.IsLedgeAhead())
            {
                if (CanMakeTheJump()) { Jump(moveInput); }
                else { moveInput = 0; } // Pulo impossível, cancela o movimento
            }
            else if (motor.IsObstacleAhead())
            {
                if (CanJumpOverWall()) { Jump(moveInput); }
                else { moveInput = 0; } // Parede intransponível, cancela o movimento
            }
        }

        // 3. EXECUÇÃO: Executa a ação final.
        if (doAttack)
        {
            if (canAttack) StartCoroutine(AttackCoroutine());
        }
        else
        {
            motor.Move(moveInput);
        }
    }
    #endregion

    #region Movement and Navigation Helpers
    void Jump(float direction)
    {
        motor.AddJumpForce(new Vector2(direction * airControlForce, jumpForce));
    }

    void FindPlayer() { playerTarget = AIManager.Instance?.playerTarget; }

    void FaceDirection(float direction)
    {
        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
        motor.currentFacingDirection = isFacingRight ? 1 : -1;
    }
    #endregion

    #region Perception Helpers (Vision, etc.)
    void HandleHeadLook()
    {
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 targetLookDirection = forward;
        switch (currentState)
        {
            case State.Hunting:
            case State.Attacking:
                targetLookDirection = (playerTarget.position - eyes.position).normalized; break;
            case State.Searching:
                targetLookDirection = (lastKnownPlayerPosition - eyes.position).normalized; break;
            case State.Patrolling:
                if (motor.IsLedgeAhead()) { targetLookDirection = Quaternion.Euler(0, 0, -downwardLookAngle * (isFacingRight ? 1 : -1)) * forward; }
                else if (HasPlatformAbove()) { targetLookDirection = Quaternion.Euler(0, 0, upwardLookAngle * (isFacingRight ? 1 : -1)) * forward; }
                break;
        }
        if (Vector2.Dot(targetLookDirection, forward) < 0) { targetLookDirection = forward; }
        lookDirection = Vector2.Lerp(lookDirection, targetLookDirection, Time.deltaTime * lookSpeed);
    }

    private bool CanSeePlayer() { /* ... */ return false; } // Omitido por brevidade
    private bool HasPlatformAbove() { return Physics2D.Raycast(eyes.position, Vector2.up, platformCheckHeight, motor.groundLayer); }
    private bool CanJumpOverWall() { return !Physics2D.Raycast(eyes.position, Vector2.up, 3f, visionBlockers); }
    private bool CanMakeTheJump() { RaycastHit2D hit = Physics2D.Raycast(motor.ledgeCheck.position, Vector2.right * motor.currentFacingDirection, maxJumpDistance, motor.groundLayer); return hit.collider != null; }
    private bool IsPlayerInAttackRange() { if (playerTarget == null) return false; return Vector2.Distance(transform.position, playerTarget.position) <= attackRange; }
    #endregion

    #region State Machine and Coroutines
    private void ChangeState(State newState) { /* ... */ }
    private IEnumerator AttackCoroutine() { /* ... */ yield return null; }
    #endregion

    #region Gizmos
    // GIZMO RESTAURADO
    void OnDrawGizmosSelected()
    {
        if (eyes == null) return;

        Vector3 forward = Application.isPlaying ? (Vector3)lookDirection.normalized : (transform.right * (transform.localScale.x > 0 ? 1 : -1));

        Gizmos.color = Color.yellow;
        Vector3 p1 = eyes.position + (Quaternion.Euler(0, 0, visionAngle / 2) * forward) * visionRange;
        Vector3 p2 = eyes.position + (Quaternion.Euler(0, 0, -visionAngle / 2) * forward) * visionRange;
        Gizmos.DrawLine(eyes.position, p1);
        Gizmos.DrawLine(eyes.position, p2);

        if (Application.isPlaying && CanSeePlayer())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyes.position, playerTarget.position);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(eyes.position, eyes.position + Vector3.up * platformCheckHeight);
    }
    #endregion
}