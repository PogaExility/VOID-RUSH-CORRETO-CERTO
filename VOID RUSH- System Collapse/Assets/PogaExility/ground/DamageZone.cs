using UnityEngine;

// Adiciona um Collider2D automaticamente, garantindo que a detec��o de colis�o funcione.
[RequireComponent(typeof(Collider2D))]
public class DamageZone : MonoBehaviour
{
    [Header("Configura��es de Dano")]
    [Tooltip("A quantidade de dano que esta zona causa ao jogador.")]
    public float damageAmount = 10f;

    [Tooltip("Se marcado, causa dano continuamente enquanto o jogador estiver em contato.")]
    public bool continuousDamage = false;

    [Tooltip("O intervalo em segundos entre cada 'tick' de dano cont�nuo.")]
    public float damageInterval = 1f;

    // Vari�vel para controlar o tempo do dano cont�nuo
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
            // Calcula a dire��o do ataque (do espinho para o jogador)
            Vector2 attackDirection = (player.transform.position - transform.position).normalized;

            if (!continuousDamage)
            {
                // Passa a dire��o do ataque para a fun��o TakeDamage
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
                // Calcula a dire��o do ataque tamb�m para o dano cont�nuo.
                Vector2 attackDirection = (other.transform.position - transform.position).normalized;

                // --- E AQUI TAMB�M ---
                player.TakeDamage(damageAmount, attackDirection);

                lastDamageTime = Time.time;
            }
        }
    }
}