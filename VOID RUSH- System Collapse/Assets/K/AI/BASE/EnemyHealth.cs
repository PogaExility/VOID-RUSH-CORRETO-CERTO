using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))] // Garante que tem AudioSource
public class EnemyHealth : MonoBehaviour
{
    private EnemyBrain _brain;
    private EnemyAIController _ai;
    private AudioSource _audioSource; // Nosso alto-falante
    private float _currentHealth;
    private bool _isDead = false;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _ai = GetComponent<EnemyAIController>();
        _audioSource = GetComponent<AudioSource>();

        // Configuração básica do áudio para não ficar 2D (som na cabeça)
        _audioSource.spatialBlend = 1f; // 1 = 3D (som diminui com a distância)
        _audioSource.minDistance = 2f;
        _audioSource.maxDistance = 20f;

        if (_brain != null && _brain.stats != null)
            _currentHealth = _brain.stats.maxHealth;
    }

    public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
    {
        if (_isDead) return;

        _currentHealth -= damage;

        // --- SOM DE DANO ---
        if (_brain.stats.damageSound && _audioSource)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f); // Variação leve
            _audioSource.PlayOneShot(_brain.stats.damageSound);
        }

        // 1. Aplica Knockback
        if (_brain.motor != null)
            _brain.motor.ApplyKnockback(knockbackDir, knockbackForce);

        // 2. Avisa a IA (Reação)
        if (_ai != null)
            _ai.OnSuspiciousActivityDetected(transform.position - (Vector3)knockbackDir);

        // 3. VFX de Dano
        if (_brain.stats.hitVFX != null)
            Instantiate(_brain.stats.hitVFX, transform.position, Quaternion.identity);

        StartCoroutine(FlashRed());

        if (_currentHealth <= 0) Die();
    }

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

        // LÓGICA DE SOM E VFX DE MORTE
        if (_brain.stats.isExploder)
        {
            // Se morreu antes de explodir, explode igual (ou mude para deathSound se preferir)
            if (_brain.stats.explosionVFX)
                Instantiate(_brain.stats.explosionVFX, transform.position, Quaternion.identity);

            // Som da explosão (PlayClipAtPoint cria um áudio temporário na cena)
            if (_brain.stats.explosionSound)
                AudioSource.PlayClipAtPoint(_brain.stats.explosionSound, transform.position, 1f);
        }
        else
        {
            // Morte normal
            if (_brain.stats.deathVFX)
                Instantiate(_brain.stats.deathVFX, transform.position, Quaternion.identity);

            // Som de morte
            if (_brain.stats.deathSound)
                AudioSource.PlayClipAtPoint(_brain.stats.deathSound, transform.position, 1f);
        }

        Destroy(gameObject);
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