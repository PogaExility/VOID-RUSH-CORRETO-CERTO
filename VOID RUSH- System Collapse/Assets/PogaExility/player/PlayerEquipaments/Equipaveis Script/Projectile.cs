// Projectile.cs - MODIFICADO
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 3f;

    // O dano agora é privado e definido pela arma.
    private float damage;

    // NOVA FUNÇÃO: A arma vai chamar isso para dar a "identidade" ao projétil.
    public void Initialize(float damageFromWeapon)
    {
        this.damage = damageFromWeapon;
    }

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Agora o dano usado é o que a arma mandou.
        Debug.Log($"Bala atingiu {other.name} causando {damage} de dano.");

        // TODO: Lógica de dano real aqui.
        // var enemyHealth = other.GetComponent<EnemyHealth>();
        // if (enemyHealth != null) {
        //     enemyHealth.TakeDamage(this.damage);
        // }

        Destroy(gameObject);
    }
}