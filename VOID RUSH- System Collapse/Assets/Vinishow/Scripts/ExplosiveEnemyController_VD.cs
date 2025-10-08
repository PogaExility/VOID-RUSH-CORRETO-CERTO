using System.Collections;
using UnityEngine;

// O Animator não é mais requerido
[RequireComponent(typeof(Rigidbody2D))]
public class ExplosiveEnemyController_VD : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private ExplosiveEnemyDataSO_VD enemyData;

    [Header("Pontos de Referência")]
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
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        ChangeState(State.Patrolling);
    }

    void Update()
    {
        if (playerTransform == null || currentState == State.Exploding) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case State.Patrolling:
                if (distanceToPlayer <= enemyData.alertRadius)
                    ChangeState(State.Alert);
                break;
            case State.Alert:
                FacePlayer();
                if (distanceToPlayer <= enemyData.explosionRadius)
                    ChangeState(State.Priming);
                else if (distanceToPlayer > enemyData.alertRadius)
                    ChangeState(State.Patrolling);
                break;
            case State.Priming:
                FacePlayer();
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == State.Patrolling)
        {
            rb.linearVelocity = new Vector2(enemyData.patrolSpeed * facingDirection, rb.linearVelocity.y);
            if (IsNearWall() || !IsGroundAhead())
            {
                Flip();
            }
        }
        else
        {
            // Para o movimento horizontal nos outros estados
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        // Se o estado for Priming, inicia a contagem regressiva
        if (currentState == State.Priming)
        {
            StartCoroutine(FuseCoroutine());
        }
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(enemyData.fuseTime);
        // Checagem de segurança para garantir que o estado não mudou enquanto esperava
        if (currentState == State.Priming)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Garante que o estado seja 'Exploding' para parar todos os Updates
        currentState = State.Exploding;

        rb.linearVelocity = Vector2.zero;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // --- A LÓGICA DA EXPLOSÃO ACONTECE AQUI IMEDIATAMENTE ---

        if (enemyData.explosionPrefab != null)
        {
            GameObject explosion = Instantiate(enemyData.explosionPrefab, transform.position, Quaternion.identity);
            if (explosion.TryGetComponent<ExplosionEffect_VD>(out var effect))
                effect.Initialize(enemyData.explosionDamage, enemyData.explosionRadius, enemyData.explosionKnockback);
        }

        // Destrói o inimigo
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
        // Gizmos de Detecção de Ambiente
        Gizmos.color = Color.blue;
        if (wallCheckPoint != null) Gizmos.DrawRay(wallCheckPoint.position, (transform.right * facingDirection) * environmentCheckDistance);
        if (ledgeCheckPoint != null) Gizmos.DrawRay(ledgeCheckPoint.position, Vector2.down * environmentCheckDistance);

        // Gizmos de Alcance
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