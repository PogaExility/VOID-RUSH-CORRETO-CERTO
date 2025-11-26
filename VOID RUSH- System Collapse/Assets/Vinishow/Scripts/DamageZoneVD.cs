using UnityEngine;

public class DamageZoneVD : MonoBehaviour
{
    [Header("Configuração de Dano")]
    [Tooltip("A quantidade de dano que esta zona causa ao jogador a cada toque.")]
    [SerializeField] private float danoAoContato = 10f;

    [Header("Configuração de Knockback")]
    [Tooltip("A força com que o jogador é empurrado para trás ao tocar na parede.")]
    [SerializeField] private float forcaDoKnockback = 5f;

    // Esta função é chamada automaticamente quando uma colisão 2D começa.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Verifica se o objeto que colidiu tem a tag "Player".
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. Tenta pegar o componente de status do jogador.
            if (collision.gameObject.TryGetComponent<PlayerStats>(out var playerStats))
            {
                // --- MODIFICAÇÃO PRINCIPAL AQUI ---

                // 3. Calcula a direção do knockback.
                // A direção será para longe do ponto de contato.
                // A 'normal' do contato é um vetor que aponta para fora da superfície,
                // que é exatamente a direção que queremos para o knockback.
                Vector2 knockbackDirection = collision.contacts[0].normal;

                // 4. Chama a função TakeDamage com TODOS os parâmetros corretos.
                playerStats.TakeDamage(danoAoContato, knockbackDirection, forcaDoKnockback);
            }
        }
    }

    // --- VERSÃO PARA TRIGGERS (se a parede não for sólida) ---
    // Se a sua zona de dano for um Trigger (como um campo de força), use esta versão.
    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerStats>(out var playerStats))
            {
                // Para triggers, não temos um ponto de contato, então calculamos a direção
                // do centro da zona de dano para o centro do jogador.
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;

                playerStats.TakeDamage(danoAoContato, knockbackDirection, forcaDoKnockback);
            }
        }
    }
    */
}