using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    private Rigidbody2D _rb;
    private CapsuleCollider2D _collider;
    private float _originalGravity;

    [Header("▶ Referências Visuais")]
    public Transform bodyTransform;

    [Header("▶ Configuração Física")]
    public float maxSpeed = 5f; // Renomeado de walkSpeed para consistência
    public float climbSpeed = 4f;
    public float acceleration = 20f;
    public float jumpForce = 14f;
    public float stopDistance = 0.1f;
    public float crouchHeight = 1.9f;

    [Tooltip("Força vertical extra para entrar no duto.")]
    public float ventEntryLiftForce = 2.0f;

    [Header("▶ Sensores")]
    public LayerMask groundLayer;
    public float groundCheckDist = 0.1f;

    // Cache
    private Vector2 _standSize;
    private Vector2 _standOffset;
    private Vector3 _standBodyPos;
    private Vector2 _crouchSize;
    private Vector2 _crouchOffset;

    // Estados
    public bool IsCrouching { get; private set; }
    public bool IsClimbing { get; private set; }
    public bool IsGrounded { get; private set; } // Restaurado
    public bool IsBusy { get; private set; } // Mantido para compatibilidade

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CapsuleCollider2D>();
        _originalGravity = _rb.gravityScale;

        // Snapshot Inicial
        _standSize = _collider.size;
        _standOffset = _collider.offset;
        if (bodyTransform != null) _standBodyPos = bodyTransform.localPosition;

        // Cálculo Crouch
        _crouchSize = new Vector2(_standSize.x, crouchHeight);
        float diff = _standSize.y - crouchHeight;
        _crouchOffset = new Vector2(_standOffset.x, _standOffset.y - (diff / 2));
    }

    void FixedUpdate()
    {
        CheckGround(); // Restaurado

        if (IsClimbing)
        {
            _rb.gravityScale = 0;

            if (IsCrouching)
            {
                // Modo Híbrido (Duto): Mantém movimento X, força leve subida Y
                // Note: MoveTowards define a velocidade X no _rb.linearVelocity diretamente? 
                // Não, MoveTowards abaixo calcula e aplica. Aqui apenas garantimos o Y.
                // Mas como MoveTowards roda a cada frame, precisamos aplicar o Y lá ou aqui.
                // Vamos aplicar a lógica de movimento aqui baseada na velocidade atual.
            }
            else
            {
                // Escalada Padrão: Y sobe, X travado
                _rb.linearVelocity = new Vector2(0, climbSpeed);
            }
        }
        else
        {
            _rb.gravityScale = _originalGravity;
        }
    }

    // --- COMANDOS DE MOVIMENTO ---

    public void MoveTowards(float targetX)
    {
        float deltaX = targetX - transform.position.x;

        // Zona morta
        if (Mathf.Abs(deltaX) < stopDistance)
        {
            StopMoving();
            return;
        }

        float dir = Mathf.Sign(deltaX);
        // Flip visual
        if (dir > 0 && transform.localScale.x < 0) Flip();
        else if (dir < 0 && transform.localScale.x > 0) Flip();

        float targetSpeed = dir * maxSpeed;
        if (IsCrouching) targetSpeed *= 0.5f;

        // Aplica Aceleração
        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        float newY = _rb.linearVelocity.y;

        // OVERRIDE PARA DUTO (Híbrido)
        if (IsClimbing && IsCrouching)
        {
            newY = ventEntryLiftForce; // Força subir a quina
        }

        _rb.linearVelocity = new Vector2(newX, newY);
    }

    public void StopMoving()
    {
        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, 0, acceleration * Time.fixedDeltaTime);
        float newY = _rb.linearVelocity.y;

        if (IsClimbing && IsCrouching) newY = ventEntryLiftForce; // Mantém sustentação paralisado
        else if (IsClimbing) newY = 0; // Trava na parede

        _rb.linearVelocity = new Vector2(newX, newY);
    }

    public void Jump()
    {
        if (IsClimbing || !IsGrounded) return; // Não pula da parede (por enquanto) nem do ar

        // Se estiver agachado, tenta levantar antes, senão cancela
        if (IsCrouching) return;

        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // --- ESTADOS ---

    public void StartClimb()
    {
        if (IsClimbing) return;
        IsClimbing = true;
        if (IsCrouching) StopCrouch();
        _rb.linearVelocity = Vector2.zero;
    }

    public void StopClimb()
    {
        if (!IsClimbing) return;
        IsClimbing = false;
        _rb.gravityScale = _originalGravity;
    }

    public void StartCrouch()
    {
        if (IsCrouching) return;
        IsCrouching = true;
        _collider.size = _crouchSize;
        _collider.offset = _crouchOffset;
        if (bodyTransform != null)
        {
            float diff = _standSize.y - crouchHeight;
            bodyTransform.localPosition = new Vector3(_standBodyPos.x, _standBodyPos.y - (diff / 2), _standBodyPos.z);
        }
    }

    public void StopCrouch()
    {
        if (!IsCrouching) return;
        IsCrouching = false;
        _collider.size = _standSize;
        _collider.offset = _standOffset;
        if (bodyTransform != null) bodyTransform.localPosition = _standBodyPos;
    }

    // --- AUXILIARES ---

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void CheckGround()
    {
        Vector2 center = (Vector2)transform.position + _collider.offset;
        Vector2 size = new Vector2(_collider.size.x * 0.9f, 0.05f);
        float dist = (_collider.size.y / 2f) + groundCheckDist;
        IsGrounded = Physics2D.BoxCast(center, size, 0, Vector2.down, dist, groundLayer);
    }
}