using UnityEngine;

public class DamageZoneVD : MonoBehaviour
{
    [Header("Configuração de Dano")]
    [Tooltip("A quantidade de dano que esta zona causa ao jogador a cada toque.")]
    [SerializeField] private float danoAoContato = 10f;

    [Header("Configuração de Knockback")]
    [Tooltip("A força com que o jogador é empurrado para trás ao tocar na zona.")]
    [SerializeField] private float forcaDoKnockback = 5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<PlayerStats>(out var playerStats))
            {
                // MUDANÇA: Em vez de usar collision.contacts (que pode bugar em quinas),
                // calculamos a direção baseada no centro dos objetos.
                // Isso garante que o player seja sempre empurrado PARA LONGE do objeto de dano.
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;

                // Se o cálculo acima der zero (posições idênticas), usa um valor padrão (cima)
                if (knockbackDirection == Vector2.zero) knockbackDirection = Vector2.up;

                playerStats.TakeDamage(danoAoContato, knockbackDirection, forcaDoKnockback);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerStats>(out var playerStats))
            {
                // Mesma lógica para Triggers
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;

                if (knockbackDirection == Vector2.zero) knockbackDirection = Vector2.up;

                playerStats.TakeDamage(danoAoContato, knockbackDirection, forcaDoKnockback);
            }
        }
    }
}