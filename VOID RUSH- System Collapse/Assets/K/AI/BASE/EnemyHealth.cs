using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    private EnemyBrain _brain;
    private EnemyAIController _ai;
    private float _currentHealth;
    private bool _isDead = false;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _ai = GetComponent<EnemyAIController>();

        if (_brain != null && _brain.stats != null)
            _currentHealth = _brain.stats.maxHealth;
    }

    // CORREÇÃO: Sobrecarga com 3 argumentos para o SlashEffect e Projectile funcionarem
    public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
    {
        if (_isDead) return;

        _currentHealth -= damage;

        // 1. Aplica Knockback
        if (_brain.motor != null)
            _brain.motor.ApplyKnockback(knockbackDir, knockbackForce);

        // 2. Avisa a IA (Reação)
        if (_ai != null)
            _ai.OnSuspiciousActivityDetected(transform.position - (Vector3)knockbackDir);

        // 3. VFX de Dano (Sangue/Faísca)
        if (_brain.stats.hitVFX != null)
            Instantiate(_brain.stats.hitVFX, transform.position, Quaternion.identity);

        StartCoroutine(FlashRed());

        if (_currentHealth <= 0) Die();
    }

    // Sobrecarga simples caso algum script antigo chame só com dano
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, Vector2.zero, 0);
    }

    void Die()
    {
        _isDead = true;
        _brain.motor.Stop();
        _brain.motor.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        if (_ai) _ai.enabled = false;

        // LÓGICA DE VFX DE MORTE
        // Se for Kamikaze (isExploder), usa a explosão grande.
        // Se for normal, usa o deathVFX (fumaça/esqueleto).
        if (_brain.stats.isExploder && _brain.stats.explosionVFX != null)
        {
            Instantiate(_brain.stats.explosionVFX, transform.position, Quaternion.identity);
        }
        else if (_brain.stats.deathVFX != null)
        {
            Instantiate(_brain.stats.deathVFX, transform.position, Quaternion.identity);
        }

        Destroy(gameObject); // Remove imediatamente ou ajusta tempo se tiver anim
    }

    IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }
}