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

    // DENTRO DO SCRIPT: Projectile.cs

    void OnTriggerEnter2D(Collider2D other)
    {
        // A checagem de colisão com o chão permanece a mesma, é a prioridade máxima.
        if (other.gameObject.layer == groundLayer)
        {
            Destroy(gameObject);
            return;
        }

        // --- INÍCIO DA MUDANÇA ---

        // Esta flag nos ajudará a saber se devemos aplicar a lógica de perfuração no final.
        bool hitValidTarget = false;

        // 1. Tenta encontrar o componente do objeto interativo.
        var objetoInterativo = other.GetComponent<ObjetoInterativo>();
        if (objetoInterativo != null)
        {
            // Se encontrou, chama a função de dano do objeto.
            objetoInterativo.ReceberDano(TipoDeAtaqueAceito.ApenasRanged);
            hitValidTarget = true;
        }
        // 2. Se não era um objeto interativo, continua a lógica para inimigos.
        // Usamos 'else if' para garantir que não tentaremos atingir um inimigo se já atingimos um objeto.
        else if (other.TryGetComponent<AIController_Basic>(out var enemyAI))
        {
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

            enemyAI.TakeDamage(this.damage, attackDirection, this.knockbackPower);
            hitValidTarget = true;
        }

        // 3. Se atingimos um alvo válido (inimigo OU objeto), aplicamos a lógica de perfuração.
        if (hitValidTarget)
        {
            if (pierceCount > 0)
            {
                pierceCount--;
                damage *= (1 - damageFalloff); // Reduz o dano para o próximo alvo.
            }
            else
            {
                Destroy(gameObject); // Destrói o projétil se não pode mais perfurar.
            }
        }

        // --- FIM DA MUDANÇA ---
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