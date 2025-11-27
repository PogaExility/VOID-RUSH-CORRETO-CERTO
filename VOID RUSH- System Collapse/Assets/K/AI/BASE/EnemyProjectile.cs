using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    private float _damage;
    private float _knockback;

    public void Initialize(float damage, float knockback, Vector2 dir, float speed)
    {
        _damage = damage;
        _knockback = knockback;
        GetComponent<Rigidbody2D>().linearVelocity = dir * speed;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerStats>(out var player))
        {
            Vector2 dir = GetComponent<Rigidbody2D>().linearVelocity.normalized;
            player.TakeDamage(_damage, dir, _knockback);
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Chao"))
        {
            Destroy(gameObject);
        }
    }
}