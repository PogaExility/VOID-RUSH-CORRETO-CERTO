using UnityEngine;
using System.Collections;

/// <summary>
/// O "Corpo" da IA (v4.0 - Motor de Plataforma Tático).
/// Este componente é responsável por TODA a interação com a física e o movimento.
/// Possui movimento baseado em aceleração e manobras de plataforma como escalar (Vault).
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class EnemyMotor : MonoBehaviour
{
    // =================================================================================================
    // CONFIGURAÇÃO
    // =================================================================================================

    #region Configuração

    [Header("▶ Atributos de Movimento")]
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 60f;

    [Header("▶ Atributos de Pulo e Escalada")]
    [Tooltip("Força máxima de um pulo normal.")]
    [SerializeField] private float jumpForce = 15f;
    [Tooltip("Velocidade de subida ao escalar um obstáculo.")]
    [SerializeField] private float vaultClimbSpeed = 5f;
    [Tooltip("Velocidade para frente ao completar uma escalada.")]
    [SerializeField] private float vaultForwardSpeed = 3f;
    [Tooltip("Distância horizontal percorrida ao subir em uma plataforma.")]
    [SerializeField] private float vaultForwardDistance = 0.5f;

    [Header("▶ Referências dos Sensores")]
    [Tooltip("Ponto na base do inimigo para checar se está no chão.")]
    [SerializeField] private Transform groundProbe;

    #endregion

    // =================================================================================================
    // ESTADO INTERNO
    // =================================================================================================

    #region Estado Interno

    private Rigidbody2D rb;
    private LayerMask obstacleLayer;

    private float currentSpeedTarget = 0f;
    private bool isFacingRight = true;
    private float originalGravityScale;

    // Propriedades Públicas para o Cérebro
    public bool IsFacingRight => isFacingRight;
    public bool IsTransitioningAction { get; private set; } // Trava o cérebro durante manobras
    public Vector2 Velocity => rb.linearVelocity;

    #endregion

    #region Inicialização e Ciclo de Vida

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
    }

    public void Initialize(LayerMask obstacles)
    {
        this.obstacleLayer = obstacles;
    }

    private void FixedUpdate()
    {
        // Se o motor está executando uma ação especial (como escalar), a física é controlada pela corrotina.
        if (IsTransitioningAction) return;

        // Se não, aplica o movimento horizontal normal com aceleração.
        ApplyHorizontalMovement();
    }

    private void ApplyHorizontalMovement()
    {
        float targetXVelocity = currentSpeedTarget * (isFacingRight ? 1 : -1);
        float accelRate = (Mathf.Abs(targetXVelocity) > 0.01f) ? acceleration : deceleration;
        float speedDifference = targetXVelocity - rb.linearVelocity.x;
        rb.AddForce(speedDifference * accelRate * Vector2.right);
    }
    #endregion

    #region Comandos do Cérebro

    public void Move(float targetSpeed) { currentSpeedTarget = targetSpeed; }
    public void Stop() { currentSpeedTarget = 0f; }
    public void Flip() { isFacingRight = !isFacingRight; transform.Rotate(0f, 180f, 0f); }

    public void Jump(float strength = 1.0f)
    {
        if (IsGrounded() && !IsTransitioningAction)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * (jumpForce * Mathf.Clamp01(strength)), ForceMode2D.Impulse);
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        currentSpeedTarget = 0;
        if (rb.bodyType != RigidbodyType2D.Dynamic) return;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Inicia a manobra de escalar um obstáculo (Vault).
    /// </summary>
    /// <param name="ledgePoint">O ponto no topo da beirada que a IA deve alcançar.</param>
    public void StartVault(Vector2 ledgePoint)
    {
        if (IsTransitioningAction) return;
        StartCoroutine(VaultRoutine(ledgePoint));
    }

    private IEnumerator VaultRoutine(Vector2 ledgePoint)
    {
        IsTransitioningAction = true; // Trava o cérebro
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        // Fase 1: Movimento Vertical
        Vector2 startPos = transform.position;
        Vector2 verticalTarget = new Vector2(startPos.x, ledgePoint.y);
        float verticalDuration = Vector2.Distance(startPos, verticalTarget) / vaultClimbSpeed;

        float timer = 0f;
        while (timer < verticalDuration)
        {
            rb.MovePosition(Vector2.Lerp(startPos, verticalTarget, timer / verticalDuration));
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Fase 2: Movimento Horizontal para subir na plataforma
        Vector2 forwardStart = transform.position;
        Vector2 forwardTarget = (Vector2)transform.position + new Vector2(vaultForwardDistance * (isFacingRight ? 1 : -1), 0);
        float forwardDuration = Vector2.Distance(forwardStart, forwardTarget) / vaultForwardSpeed;

        timer = 0f;
        while (timer < forwardDuration)
        {
            rb.MovePosition(Vector2.Lerp(forwardStart, forwardTarget, timer / forwardDuration));
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Finaliza a manobra
        rb.gravityScale = originalGravityScale;
        IsTransitioningAction = false; // Destrava o cérebro
    }

    #endregion

    #region Sensores

    public bool IsGrounded()
    {
        if (groundProbe == null) return false;
        return Physics2D.Raycast(groundProbe.position, Vector2.down, 0.1f, obstacleLayer);
    }

    #endregion

    #region Gizmos

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundProbe != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawLine(groundProbe.position, (Vector2)groundProbe.position + Vector2.down * 0.1f);
        }
    }
#endif

    #endregion
}