// Projectile.cs - VERSÃO COMPLETA E CORRIGIDA COM PERFURAÇÃO E GIZMO
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // Variáveis de estado do projétil, definidas pela arma.
    private float damage;
    private int pierceCount;
    private float damageFalloff;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection; // <-- VARIÁVEL ADICIONADA

    private Rigidbody2D rb;

    // Tags e Layers para otimização. Configure-as no Inspector do Unity.
    private const string ENEMY_TAG = "Enemy";
    private const string GROUND_LAYER_NAME = "Chao";
    private int groundLayer;

    [Header("Debug")]
    [SerializeField] private float gizmoLength = 1f; // <-- VARIÁVEL ADICIONADA PARA O GIZMO

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer(GROUND_LAYER_NAME);
    }

    /// <summary>
    /// A função Initialize agora aceita todos os dados da "ficha técnica", incluindo a direção do knockback.
    /// </summary>
    public void Initialize(float damage, float speed, float lifetime, int pierceCount, float damageFalloff, float knockback, RangedKnockbackDirection knockbackDir)
    {
        this.damage = damage;
        this.pierceCount = pierceCount;
        this.damageFalloff = damageFalloff;
        this.knockbackPower = knockback;
        this.knockbackDirection = knockbackDir; // <-- ARMAZENA A DIREÇÃO RECEBIDA
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == groundLayer)
        {
            Destroy(gameObject);
            return;
        }

        if (other.TryGetComponent<AIController_Basic>(out var enemyAI))
        {
            Vector2 attackDirection;

            // Decide qual vetor de direção usar com base na instrução recebida.
            switch (knockbackDirection)
            {
                case RangedKnockbackDirection.Frente:
                    attackDirection = rb.linearVelocity.normalized;
                    break;
                // Futuramente, outros 'cases' podem ser adicionados aqui.
                default:
                    attackDirection = rb.linearVelocity.normalized;
                    break;
            }

            enemyAI.TakeDamage(this.damage, attackDirection, this.knockbackPower);

            if (pierceCount > 0)
            {
                pierceCount--;
                damage *= (1 - damageFalloff);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Desenha um Gizmo no Editor para visualizar a direção "para frente" do projétil.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + (transform.right * gizmoLength);
        Gizmos.DrawLine(startPosition, endPosition);
    }
#endif
}