using UnityEngine;

// Adiciona um Collider2D automaticamente, garantindo que a detecção de colisão funcione.
[RequireComponent(typeof(Collider2D))]
public class DamageZone : MonoBehaviour
{
    [Header("Configurações de Dano")]
    [Tooltip("A quantidade de dano que esta zona causa ao jogador.")]
    public float damageAmount = 10f;

    [Tooltip("Se marcado, causa dano continuamente enquanto o jogador estiver em contato.")]
    public bool continuousDamage = false;

    [Tooltip("O intervalo em segundos entre cada 'tick' de dano contínuo.")]
    public float damageInterval = 1f;

    // Variável para controlar o tempo do dano contínuo
    private float lastDamageTime;

    // Garante que o Collider seja um 'Trigger' para que o jogador possa passar por ele,
    // em vez de colidir e parar.
    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    // Chamado quando o jogador ENTRA na zona de dano.
    // Dentro de DamageZone.cs
    // Dentro do seu DamageZone.cs

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            // Calcula a direção do ataque (do espinho para o jogador)
            Vector2 attackDirection = (player.transform.position - transform.position).normalized;

            if (!continuousDamage)
            {
                // Passa a direção do ataque para a função TakeDamage
                player.TakeDamage(damageAmount, attackDirection);
            }
            // ...
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!continuousDamage) return;

        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                // Calcula a direção do ataque também para o dano contínuo.
                Vector2 attackDirection = (other.transform.position - transform.position).normalized;

                // --- E AQUI TAMBÉM ---
                player.TakeDamage(damageAmount, attackDirection);

                lastDamageTime = Time.time;
            }
        }
    }
}