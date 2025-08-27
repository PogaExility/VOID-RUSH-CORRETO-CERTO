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
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou tem o script PlayerStats (ou seja, � o nosso jogador)
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            // Se o dano n�o for cont�nuo, causa dano uma �nica vez, na entrada.
            if (!continuousDamage)
            {
                player.TakeDamage(damageAmount);
                // Opcional: Adicionar um efeito visual ou sonoro de dano aqui
            }

            // Reseta o timer do dano cont�nuo para que o primeiro tick seja imediato.
            lastDamageTime = Time.time;
        }
    }

    // Chamado A CADA FRAME enquanto o jogador PERMANECE dentro da zona de dano.
    private void OnTriggerStay2D(Collider2D other)
    {
        // Se o dano n�o for cont�nuo, n�o faz nada.
        if (!continuousDamage) return;

        // Verifica se o objeto � o jogador
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            // Verifica se j� passou tempo suficiente desde o �ltimo 'tick' de dano.
            if (Time.time >= lastDamageTime + damageInterval)
            {
                player.TakeDamage(damageAmount);
                lastDamageTime = Time.time; // Atualiza o timer
                // Opcional: Adicionar um efeito visual ou sonoro de dano aqui
            }
        }
    }
}