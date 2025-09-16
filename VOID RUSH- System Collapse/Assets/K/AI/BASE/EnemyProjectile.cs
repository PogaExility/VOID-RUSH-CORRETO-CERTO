using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    // Voc� pode pegar esses valores do AIController se quiser
    public float damage = 10f;
    public float knockbackForce = 20f;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Tenta encontrar o componente de stats do jogador
        if (other.TryGetComponent<PlayerStats>(out PlayerStats player))
        {
            // Calcula a dire��o do knockback
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;

            // Aplica dano e knockback ao jogador
            player.TakeDamage(damage, knockbackDirection, knockbackForce);
        }

        // Destroi o proj�til ao colidir com qualquer coisa (exceto o pr�prio inimigo, se necess�rio)
        // Adicione uma verifica��o de tag se n�o quiser que ele se destrua em certos objetos.
        Destroy(gameObject);
    }
}