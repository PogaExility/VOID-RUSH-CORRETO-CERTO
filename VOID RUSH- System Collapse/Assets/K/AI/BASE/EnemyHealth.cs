using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    private EnemyBrain _brain;
    private EnemyAIController _ai; // Referência para forçar mudança de estado
    private float _currentHealth;
    private bool _isDead = false;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _ai = GetComponent<EnemyAIController>();
        _currentHealth = _brain.stats.maxHealth;
    }

    public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
    {
        if (_isDead) return;

        _currentHealth -= damage;

        // 1. FÍSICA (Knockback)
        if (_brain.motor != null) _brain.motor.ApplyKnockback(knockbackDir, knockbackForce);

        // 2. REAÇÃO DE INTELIGÊNCIA (A NOVIDADE)
        // Calcula de onde veio o tiro (inverso do knockback)
        Vector3 attackOrigin = transform.position - (Vector3)(knockbackDir * 2f);

        // Avisa a IA: "Ouvi/Senti algo vindo daquela direção!"
        if (_ai != null)
        {
            _ai.OnSuspiciousActivityDetected(attackOrigin);
        }

        // 3. FEEDBACK VISUAL
        StartCoroutine(FlashRed());

        if (_currentHealth <= 0) Die();
    }

    void Die()
    {
        _isDead = true;
        _brain.motor.Stop();
        _brain.motor.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        if (_ai) _ai.enabled = false; // Desliga o cérebro
        Destroy(gameObject, 1f);
    }

    IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }
}