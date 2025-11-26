using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // Variáveis de estado do projétil, definidas pela arma.
    private float damage;
    private int pierceCount;
    private float damageFalloff;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection;

    private Rigidbody2D rb;

    // Tags e Layers para otimização. Configure-as no Inspector do Unity.
    private const string ENEMY_TAG = "Enemy";
    private const string GROUND_LAYER_NAME = "Chao";
    private int groundLayer;

    [Header("Debug")]
    [SerializeField] private float gizmoLength = 1f;

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
        this.knockbackDirection = knockbackDir;
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // A checagem de colisão com o chão permanece a mesma.
        if (other.gameObject.layer == groundLayer)
        {
            Destroy(gameObject);
            return;
        }

        bool hitValidTarget = false;

        // 1. Tenta encontrar o componente do objeto interativo.
        var objetoInterativo = other.GetComponent<ObjetoInterativo>();
        if (objetoInterativo != null)
        {
            objetoInterativo.ReceberHit(TipoDeAtaqueAceito.ApenasRanged);
            hitValidTarget = true;
        }
        // 2. CORREÇÃO AQUI: Se não era um objeto interativo, procura pelo EnemyHealth (novo script do seu colega)
        else if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            // Mantivemos a sua lógica de direção de ataque, que estava melhor que a dele
            Vector2 attackDirection;
            switch (knockbackDirection)
            {
                case RangedKnockbackDirection.Frente:
                    attackDirection = rb.linearVelocity.normalized;
                    break;
                default:
                    attackDirection = rb.linearVelocity.normalized;
                    break;
            }

            // Aplica o dano no script novo
            enemyHealth.TakeDamage(this.damage, attackDirection, this.knockbackPower);
            hitValidTarget = true;
        }

        // 3. A lógica de perfuração funciona para qualquer alvo válido (inimigo OU objeto).
        if (hitValidTarget)
        {
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