// Projectile.cs - VERS�O COMPLETA E CORRIGIDA COM PERFURA��O
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // Vari�veis de estado do proj�til, definidas pela arma.
    private float damage;
    private int pierceCount;
    private float damageFalloff;

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
    public void Initialize(float damage, float speed, float lifetime, int pierceCount, float damageFalloff)
    {
        this.damage = damage;
        this.pierceCount = pierceCount;
        this.damageFalloff = damageFalloff;

        // A l�gica de movimento e tempo de vida foi movida para c�.
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se a bala colidir com o ch�o, ela se destr�i.
        if (other.gameObject.layer == groundLayer)
        {
            Destroy(gameObject);
            return; // Para a execu��o da fun��o aqui.
        }

        // Se a bala colidir com um inimigo...
        if (other.CompareTag(ENEMY_TAG))
        {
            // Tenta pegar o componente de vida do inimigo.
            // TODO: Substitua 'EnemyHealth' pelo nome real do seu script de vida do inimigo.
            // var enemyHealth = other.GetComponent<EnemyHealth>();
            // if (enemyHealth != null)
            // {
            //     enemyHealth.TakeDamage(this.damage);
            // }
            Debug.Log($"Atingiu {other.name} com {this.damage} de dano.");

            // Verifica se ainda pode perfurar.
            if (pierceCount > 0)
            {
                pierceCount--; // Gasta uma perfura��o.
                damage *= (1 - damageFalloff); // Aplica a redu��o (ou aumento) de dano.

                // N�o se destr�i e continua voando.
            }
            else
            {
                // Se n�o pode mais perfurar, se destr�i.
                Destroy(gameObject);
            }
        }

        // Se colidir com qualquer outra coisa (outro proj�til, item, etc.), n�o faz nada.
    }
}