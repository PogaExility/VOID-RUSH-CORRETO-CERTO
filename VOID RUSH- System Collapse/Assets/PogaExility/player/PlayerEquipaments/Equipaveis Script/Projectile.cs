// Projectile.cs - VERS�O COMPLETA E CORRIGIDA COM PERFURA��O
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // Vari�veis de estado do proj�til, definidas pela arma.
    private float damage;
    private int pierceCount;
    private float damageFalloff;
    private float knockbackPower;

    private Rigidbody2D rb;

    // Tags e Layers para otimiza��o. Configure-as no Inspector do Unity.
    private const string ENEMY_TAG = "Enemy";
    private const string GROUND_LAYER_NAME = "Chao";
    private int groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Converte o nome da layer para um n�mero inteiro no in�cio para otimizar.
        groundLayer = LayerMask.NameToLayer(GROUND_LAYER_NAME);
    }

    // A fun��o Initialize agora aceita todos os dados da "ficha t�cnica".

    public void Initialize(float damage, float speed, float lifetime, int pierceCount, float damageFalloff, float knockback)
    {
        this.damage = damage;
        this.pierceCount = pierceCount;
        this.damageFalloff = damageFalloff;
        this.knockbackPower = knockback;
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Projectile] Colidi com: {other.gameObject.name}");
        if (other.gameObject.layer == groundLayer)
        {
            Destroy(gameObject);
            return;
        }

        // Tenta pegar o componente do inimigo.
        if (other.TryGetComponent<AIController_Basic>(out var enemyAI))
        {
            Debug.Log($"[Projectile] Inimigo '{other.gameObject.name}' detectado! Enviando comando de dano.");
            Vector2 attackDirection = rb.linearVelocity.normalized;
            Debug.Log($"[Projectile] Enviando Knockback: {this.knockbackPower} na dire��o {attackDirection}");
     
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
}