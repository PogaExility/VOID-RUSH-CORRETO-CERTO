// Projectile.cs - VERSÃO COMPLETA E CORRIGIDA COM PERFURAÇÃO
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // Variáveis de estado do projétil, definidas pela arma.
    private float damage;
    private int pierceCount;
    private float damageFalloff;

    private Rigidbody2D rb;

    // Tags e Layers para otimização. Configure-as no Inspector do Unity.
    private const string ENEMY_TAG = "Enemy";
    private const string GROUND_LAYER_NAME = "Chao";
    private int groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Converte o nome da layer para um número inteiro no início para otimizar.
        groundLayer = LayerMask.NameToLayer(GROUND_LAYER_NAME);
    }

    // A função Initialize agora aceita todos os dados da "ficha técnica".
    public void Initialize(float damage, float speed, float lifetime, int pierceCount, float damageFalloff)
    {
        this.damage = damage;
        this.pierceCount = pierceCount;
        this.damageFalloff = damageFalloff;

        // A lógica de movimento e tempo de vida foi movida para cá.
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se a bala colidir com o chão, ela se destrói.
        if (other.gameObject.layer == groundLayer)
        {
            Destroy(gameObject);
            return; // Para a execução da função aqui.
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
                pierceCount--; // Gasta uma perfuração.
                damage *= (1 - damageFalloff); // Aplica a redução (ou aumento) de dano.

                // Não se destrói e continua voando.
            }
            else
            {
                // Se não pode mais perfurar, se destrói.
                Destroy(gameObject);
            }
        }

        // Se colidir com qualquer outra coisa (outro projétil, item, etc.), não faz nada.
    }
}