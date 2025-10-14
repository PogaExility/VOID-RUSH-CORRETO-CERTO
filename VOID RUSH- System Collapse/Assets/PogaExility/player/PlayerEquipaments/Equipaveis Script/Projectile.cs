// Projectile.cs - VERS�O COMPLETA E CORRIGIDA COM PERFURA��O E GIZMO
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // Vari�veis de estado do proj�til, definidas pela arma.
    private float damage;
    private int pierceCount;
    private float damageFalloff;
    private float knockbackPower;
    private RangedKnockbackDirection knockbackDirection; // <-- VARI�VEL ADICIONADA

    private Rigidbody2D rb;

    // Tags e Layers para otimiza��o. Configure-as no Inspector do Unity.
    private const string ENEMY_TAG = "Enemy";
    private const string GROUND_LAYER_NAME = "Chao";
    private int groundLayer;

    [Header("Debug")]
    [SerializeField] private float gizmoLength = 1f; // <-- VARI�VEL ADICIONADA PARA O GIZMO

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer(GROUND_LAYER_NAME);
    }

    /// <summary>
    /// A fun��o Initialize agora aceita todos os dados da "ficha t�cnica", incluindo a dire��o do knockback.
    /// </summary>
    public void Initialize(float damage, float speed, float lifetime, int pierceCount, float damageFalloff, float knockback, RangedKnockbackDirection knockbackDir)
    {
        this.damage = damage;
        this.pierceCount = pierceCount;
        this.damageFalloff = damageFalloff;
        this.knockbackPower = knockback;
        this.knockbackDirection = knockbackDir; // <-- ARMAZENA A DIRE��O RECEBIDA
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    // DENTRO DO SCRIPT: Projectile.cs

    // DENTRO DO SCRIPT: Projectile.cs

    void OnTriggerEnter2D(Collider2D other)
    {
        // A checagem de colis�o com o ch�o permanece a mesma.
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
            // --- IN�CIO DA MUDAN�A ---
            // Chamamos a nova fun��o 'ReceberHit' e passamos o tipo de ataque do proj�til.
            objetoInterativo.ReceberHit(TipoDeAtaqueAceito.ApenasRanged);
            // --- FIM DA MUDAN�A ---
            hitValidTarget = true;
        }
        // 2. Se n�o era um objeto interativo, a l�gica para inimigos continua.
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

        // 3. A l�gica de perfura��o funciona para qualquer alvo v�lido (inimigo OU objeto).
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
    /// Desenha um Gizmo no Editor para visualizar a dire��o "para frente" do proj�til.
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