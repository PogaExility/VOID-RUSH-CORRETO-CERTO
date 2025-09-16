using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    // Você pode pegar esses valores do AIController se quiser
    public float damage = 10f;
    public float knockbackForce = 20f;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Tenta encontrar o componente de stats do jogador
        if (other.TryGetComponent<PlayerStats>(out PlayerStats player))
        {
            // Calcula a direção do knockback
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;

            // Aplica dano e knockback ao jogador
            player.TakeDamage(damage, knockbackDirection, knockbackForce);
        }

        // Destroi o projétil ao colidir com qualquer coisa (exceto o próprio inimigo, se necessário)
        // Adicione uma verificação de tag se não quiser que ele se destrua em certos objetos.
        Destroy(gameObject);
    }
}