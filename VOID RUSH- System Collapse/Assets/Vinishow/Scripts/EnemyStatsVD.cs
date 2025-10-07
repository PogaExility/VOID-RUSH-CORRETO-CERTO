using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStatsVD : MonoBehaviour
{
    [Tooltip("Arraste aqui o Scriptable Object com os dados base deste inimigo.")]
    [SerializeField] private EnemyDataSO_VD enemyData;

    public EnemyDataSO_VD EnemyData => enemyData;

    // --- Variáveis de Estado Atual ---
    private float currentHealth;
    private bool isStunned = false;

    // --- Componentes & Referências ---
    private Rigidbody2D rb;

    // --- EVENTOS PÚBLICOS ---
    // Evento para notificar que o inimigo morreu.
    public event Action OnEnemyDied;
    // NOVO EVENTO: Notifica que o inimigo tomou dano e envia a direção do ataque.
    public event Action<Vector2> OnDamageTaken;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (enemyData == null)
        {
            Debug.LogError("O Scriptable Object 'enemyData' não foi atribuído no EnemyStatsVD de " + gameObject.name);
            this.enabled = false;
            return;
        }

        currentHealth = enemyData.maxHealth;
    }

    public void TakeDamage(float amount, Vector2 attackDirection, float incomingKnockbackPower)
    {
        if (currentHealth <= 0) return;

        // Dispara o evento de dano ANTES de aplicar o dano,
        // para que o cérebro (IA) possa reagir imediatamente.
        OnDamageTaken?.Invoke(attackDirection);

        currentHealth -= amount;
        Debug.Log(gameObject.name + " tomou " + amount + " de dano. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            float finalForce = incomingKnockbackPower - enemyData.knockbackResistance;
            if (finalForce > 0)
            {
                ApplyKnockback(finalForce, attackDirection);
            }
        }
    }

    private void ApplyKnockback(float force, Vector2 direction)
    {
        // Correção de Bug: Rigidbody2D usa .velocity.
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " morreu!");
        OnEnemyDied?.Invoke();
        Destroy(gameObject, 0.1f);
    }
}