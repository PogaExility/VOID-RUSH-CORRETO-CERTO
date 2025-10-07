using UnityEngine;

// Agora, o prefab de ataque OBRIGATORIAMENTE ter� um Collider2D e um Rigidbody2D.
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class EnemyAttackPrefabVD : MonoBehaviour
{
    // --- Dados do Ataque (recebidos do AIController) ---
    private float damage;
    private float knockbackPower;

    // --- Componentes ---
    private Rigidbody2D rb;

    // --- Controle de L�gica ---
    private bool hasHit = false; // Garante que o ataque cause dano apenas uma vez.

    /// <summary>
    /// Fun��o de inicializa��o expandida. Agora configura o movimento tamb�m.
    /// </summary>
    /// <param name="damageAmount">O dano a ser causado.</param>
    /// <param name="knockbackForce">A for�a de repuls�o.</param>
    /// <param name="projectileSpeed">A velocidade do proj�til. Se for 0, ele n�o se mover�.</param>
    /// <param name="direction">A dire��o na qual o proj�til deve viajar.</param>
    public void Initialize(float damageAmount, float knockbackForce, float projectileSpeed, Vector2 direction)
    {
        this.damage = damageAmount;
        this.knockbackPower = knockbackForce;

        // Se uma velocidade foi fornecida, aplica o movimento.
        if (projectileSpeed > 0)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Garante que o proj�til n�o seja afetado pela gravidade.
        rb.gravityScale = 0;

        GetComponent<Collider2D>().isTrigger = true;

        // Medida de seguran�a: se o proj�til se perder, ele se autodestr�i ap�s 5 segundos.
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se j� atingimos algo, ignora.
        if (hasHit) return;

        // Se atingiu o jogador...
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerStats>(out var playerStats))
            {
                hasHit = true;
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                playerStats.TakeDamage(damage, knockbackDirection, knockbackPower);
                Destroy(gameObject);
            }
        }
        // Se atingiu qualquer outra coisa que N�O seja um trigger (como uma parede ou o ch�o)...
        // Isso impede que o proj�til atravesse o cen�rio.
        else if (!other.isTrigger)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}