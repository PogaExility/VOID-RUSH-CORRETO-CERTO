using UnityEngine;

public class ExplosionEffect_VD : MonoBehaviour
{
    private float damage;
    private float radius;
    private float knockback;

    public void Initialize(float damage, float radius, float knockback)
    {
        this.damage = damage;
        this.radius = radius;
        this.knockback = knockback;
    }

    void Start()
    {
        // Encontra todos os colliders dentro do raio da explosão que estão na camada do jogador
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Player"));

        foreach (var hit in hits)
        {
            // Tenta pegar o script de status do jogador e aplicar o dano
            if (hit.TryGetComponent<PlayerStats>(out var playerStats))
            {
                Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;
                playerStats.TakeDamage(damage, knockbackDirection, knockback);
            }
        }

        // Destrói o objeto de efeito visual após um tempo (ex: 1 segundo)
        Destroy(gameObject, 1f);
    }
}