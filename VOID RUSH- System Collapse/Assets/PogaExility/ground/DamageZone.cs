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
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou tem o script PlayerStats (ou seja, é o nosso jogador)
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            // Se o dano não for contínuo, causa dano uma única vez, na entrada.
            if (!continuousDamage)
            {
                player.TakeDamage(damageAmount);
                // Opcional: Adicionar um efeito visual ou sonoro de dano aqui
            }

            // Reseta o timer do dano contínuo para que o primeiro tick seja imediato.
            lastDamageTime = Time.time;
        }
    }

    // Chamado A CADA FRAME enquanto o jogador PERMANECE dentro da zona de dano.
    private void OnTriggerStay2D(Collider2D other)
    {
        // Se o dano não for contínuo, não faz nada.
        if (!continuousDamage) return;

        // Verifica se o objeto é o jogador
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            // Verifica se já passou tempo suficiente desde o último 'tick' de dano.
            if (Time.time >= lastDamageTime + damageInterval)
            {
                player.TakeDamage(damageAmount);
                lastDamageTime = Time.time; // Atualiza o timer
                // Opcional: Adicionar um efeito visual ou sonoro de dano aqui
            }
        }
    }
}