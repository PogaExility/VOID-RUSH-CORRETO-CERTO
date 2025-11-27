using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ExplosiveEnemyController_VD : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private ExplosiveEnemyDataSO_VD enemyData;

    [Header("Referências Visuais")]
    [SerializeField] private CustomSpriteAnimatorVD spriteAnimator;

    [Header("Pontos de Referência (Física)")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private Transform ledgeCheckPoint;
    [SerializeField] private float environmentCheckDistance = 0.2f;

    // Componentes e referências
    private Rigidbody2D rb;
    private Transform playerTransform;

    // Controle de estado
    private enum State { Patrolling, Alert, Priming, Exploding }
    private State currentState;
    private int facingDirection = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteAnimator == null)
        {
            spriteAnimator = GetComponentInChildren<CustomSpriteAnimatorVD>();
        }
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (spriteAnimator == null)
        {
            // O teste da cor vermelha permanece, pois é útil
            Debug.LogError("Referência para o CustomSpriteAnimatorVD está faltando!", this);
            var renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null) renderer.color = Color.red;
            enabled = false;
            return;
        }

        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (playerTransform == null || currentState == State.Exploding) return;

        // A lógica de decisão de estado acontece primeiro
        DecideState();

        // A lógica de ação (movimento e animação) acontece com base no estado decidido
        ActOnState();
    }

    private void DecideState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case State.Patrolling:
                if (distanceToPlayer <= enemyData.alertRadius) ChangeState(State.Alert);
                break;
            case State.Alert:
                if (distanceToPlayer <= enemyData.explosionRadius) ChangeState(State.Priming);
                else if (distanceToPlayer > enemyData.alertRadius) ChangeState(State.Patrolling);
                break;
        }
    }

    private void ActOnState()
    {
        switch (currentState)
        {
            case State.Patrolling:
                rb.linearVelocity = new Vector2(enemyData.patrolSpeed * facingDirection, rb.linearVelocity.y);
                if (IsNearWall() || !IsGroundAhead())
                {
                    Flip();
                }
                break;
            case State.Alert:
            case State.Priming:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                FacePlayer();
                break;
        }
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        if (spriteAnimator == null) return;

        switch (currentState)
        {
            case State.Patrolling:
                spriteAnimator.Play(enemyData.patrolStateName);
                break;
            case State.Alert:
                spriteAnimator.Play(enemyData.alertStateName);
                break;
            case State.Priming:
                // O FuseCoroutine agora só se preocupa com a animação e o tempo
                StartCoroutine(FuseCoroutine());
                break;
        }
    }

    private IEnumerator FuseCoroutine()
    {
        spriteAnimator.Play(enemyData.alertStateName);
        yield return new WaitForSeconds(enemyData.fuseTime);
        if (currentState == State.Priming)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Esta função agora só se preocupa em iniciar a sequência de explosão
        currentState = State.Exploding;
        rb.linearVelocity = Vector2.zero;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
        StartCoroutine(ExplosionSequenceCoroutine());
    }

    private IEnumerator ExplosionSequenceCoroutine()
    {
        spriteAnimator.Play(enemyData.explodeStateName);

        SpriteAnimationVD explodeAnim = spriteAnimator.GetAnimationByName(enemyData.explodeStateName);
        float animationDuration = 0f;
        if (explodeAnim != null && explodeAnim.framesPerSecond > 0)
        {
            animationDuration = (float)explodeAnim.frames.Count / explodeAnim.framesPerSecond;
        }
        yield return new WaitForSeconds(animationDuration);

        // O dano e a destruição acontecem depois da animação
        if (enemyData.explosionPrefab != null)
        {
            GameObject explosion = Instantiate(enemyData.explosionPrefab, transform.position, Quaternion.identity);
            if (explosion.TryGetComponent<ExplosionEffect_VD>(out var effect))
            {
                effect.Initialize(enemyData.explosionDamage, enemyData.explosionRadius, enemyData.explosionKnockback);
            }
        }
        Destroy(gameObject);
    }

    // --- FUNÇÕES AUXILIARES ---

    private void FacePlayer()
    {
        if (playerTransform == null) return;
        if (playerTransform.position.x > transform.position.x && facingDirection == -1) Flip();
        else if (playerTransform.position.x < transform.position.x && facingDirection == 1) Flip();
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(0, 180, 0);
    }

    private bool IsNearWall()
    {
        if (wallCheckPoint == null) return false;
        return Physics2D.Raycast(wallCheckPoint.position, Vector2.right * facingDirection, environmentCheckDistance, LayerMask.GetMask("Chao"));
    }

    private bool IsGroundAhead()
    {
        if (ledgeCheckPoint == null) return false;
        return Physics2D.Raycast(ledgeCheckPoint.position, Vector2.down, environmentCheckDistance, LayerMask.GetMask("Chao"));
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        if (wallCheckPoint != null) Gizmos.DrawRay(wallCheckPoint.position, (transform.right * facingDirection) * environmentCheckDistance);
        if (ledgeCheckPoint != null) Gizmos.DrawRay(ledgeCheckPoint.position, Vector2.down * environmentCheckDistance);

        if (enemyData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.alertRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.explosionRadius);
        }
    }
#endif
}