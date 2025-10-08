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

    void OnTriggerEnter2D(Collider2D other)
    {
        // A checagem de colis�o com o ch�o permanece a mesma, � a prioridade m�xima.
        if (other.gameObject.layer == groundLayer)
        {
            Destroy(gameObject);
            return;
        }

        // --- IN�CIO DA MUDAN�A ---

        // Esta flag nos ajudar� a saber se devemos aplicar a l�gica de perfura��o no final.
        bool hitValidTarget = false;

        // 1. Tenta encontrar o componente do objeto interativo.
        var objetoInterativo = other.GetComponent<ObjetoInterativo>();
        if (objetoInterativo != null)
        {
            // Se encontrou, chama a fun��o de dano do objeto.
            objetoInterativo.ReceberDano(TipoDeAtaqueAceito.ApenasRanged);
            hitValidTarget = true;
        }
        // 2. Se n�o era um objeto interativo, continua a l�gica para inimigos.
        // Usamos 'else if' para garantir que n�o tentaremos atingir um inimigo se j� atingimos um objeto.
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

        // 3. Se atingimos um alvo v�lido (inimigo OU objeto), aplicamos a l�gica de perfura��o.
        if (hitValidTarget)
        {
            if (pierceCount > 0)
            {
                pierceCount--;
                damage *= (1 - damageFalloff); // Reduz o dano para o pr�ximo alvo.
            }
            else
            {
                Destroy(gameObject); // Destr�i o proj�til se n�o pode mais perfurar.
            }
        }

        // --- FIM DA MUDAN�A ---
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