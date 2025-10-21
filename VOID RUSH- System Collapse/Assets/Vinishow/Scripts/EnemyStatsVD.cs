using UnityEngine;
using System;
using System.Collections; // Necessário para usar Corrotinas

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStatsVD : MonoBehaviour
{
    [Header("Configuração de Dados")]
    [Tooltip("Arraste aqui o Scriptable Object com os dados base deste inimigo.")]
    [SerializeField] private EnemyDataSO_VD enemyData;

    [Header("Referências Visuais")]
    [Tooltip("O SpriteRenderer do inimigo que piscará em vermelho ao tomar dano.")]
    [SerializeField] private SpriteRenderer enemySpriteRenderer;

    [Header("Configuração do Feedback de Dano")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    public EnemyDataSO_VD EnemyData => enemyData;

    // --- Variáveis de Estado Atual ---
    private float currentHealth;
    private Color originalColor; // Para guardar a cor original do sprite
    private Coroutine flashCoroutine; // Para garantir que apenas um flash ocorra por vez

    // --- Componentes & Referências ---
    private Rigidbody2D rb;

    // --- EVENTOS PÚBLICOS ---
    public event Action OnEnemyDied;
    public event Action<Vector2> OnDamageTaken;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tenta encontrar o SpriteRenderer automaticamente se não for arrastado
        if (enemySpriteRenderer == null)
        {
            enemySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        // Guarda a cor original do sprite para podermos restaurá-la
        if (enemySpriteRenderer != null)
        {
            originalColor = enemySpriteRenderer.color;
        }

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

        OnDamageTaken?.Invoke(attackDirection);

        currentHealth -= amount;
        Debug.Log(gameObject.name + " tomou " + amount + " de dano. Vida restante: " + currentHealth);

        // --- LÓGICA DO FLASH DE DANO ---
        // Se já houver um flash acontecendo, interrompe-o antes de iniciar um novo.
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        // Inicia a corrotina do flash
        flashCoroutine = StartCoroutine(DamageFlashCoroutine());
        // --- FIM DA LÓGICA DO FLASH ---

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

    private IEnumerator DamageFlashCoroutine()
    {
        if (enemySpriteRenderer == null) yield break; // Se não há sprite, não faz nada

        // Muda a cor para a cor do flash
        enemySpriteRenderer.color = flashColor;
        // Espera pela duração definida
        yield return new WaitForSeconds(flashDuration);
        // Restaura a cor original
        enemySpriteRenderer.color = originalColor;

        // Marca a corrotina como finalizada
        flashCoroutine = null;
    }

    private void ApplyKnockback(float force, Vector2 direction)
    {
        // Correção de Bug
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " morreu!");
        OnEnemyDied?.Invoke();
        Destroy(gameObject, 0.1f);
    }
}