using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIPlatformerMotor : MonoBehaviour
{
    private Rigidbody2D _rb;
    [HideInInspector] public float currentFacingDirection = 1f;
    [HideInInspector] public bool isFacingRight = true;

    [Header("▶ Configuração de Sensores Físicos")]
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // Garante que o estado inicial corresponde à orientação visual do prefab
        isFacingRight = transform.localScale.x > 0;
        currentFacingDirection = isFacingRight ? 1 : -1;
    }

    // --- API DE COMANDOS ---
    public void Move(float speed)
    {
        _rb.linearVelocity = new Vector2(currentFacingDirection * speed, _rb.linearVelocity.y);
    }

    public void Stop()
    {
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
    }

    public void Jump(float jumpForce)
    {
        if (IsGrounded())
        {
            _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    // A ÚNICA função que altera a orientação. É chamada pelo Controller.
    public void Flip()
    {
        isFacingRight = !isFacingRight;
        currentFacingDirection *= -1;
        // Usamos a rotação em vez da escala para compatibilidade com a rotação dos olhos
        transform.Rotate(0f, 180f, 0f);
    }

    public bool IsGrounded()
    {
        // Usa a posição do Rigidbody para ser mais preciso fisicamente
        return Physics2D.Raycast(_rb.position, Vector2.down, groundCheckDistance, groundLayer);
    }
}