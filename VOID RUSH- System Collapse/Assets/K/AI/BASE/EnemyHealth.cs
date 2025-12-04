using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class EnemyHealth : MonoBehaviour
{
    private EnemyBrain _brain;
    private EnemyAIController _ai;
    private AudioSource _audioSource;
    private float _currentHealth;
    private bool _isDead = false;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _ai = GetComponent<EnemyAIController>();
        _audioSource = GetComponent<AudioSource>();

        _audioSource.spatialBlend = 1f;
        _audioSource.minDistance = 2f;
        _audioSource.maxDistance = 20f;

        if (_brain != null && _brain.stats != null)
            _currentHealth = _brain.stats.maxHealth;
    }

    public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
    {
        if (_isDead) return;

        _currentHealth -= damage;

        // Som de dano
        if (_brain.stats.damageSound && _audioSource)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.PlayOneShot(_brain.stats.damageSound);
        }

        // --- LÓGICA DE KNOCKBACK COM RESISTÊNCIA ---
        if (_brain.motor != null)
        {
            // 1. Checa se é totalmente imune
            if (!_brain.stats.isImmuneToKnockback)
            {
                // 2. Calcula: Força Recebida - Resistência do Inimigo
                float finalForce = knockbackForce - _brain.stats.knockbackResistance;

                // 3. Só aplica se sobrar força positiva (Mathf.Max garante que não fique negativo)
                if (finalForce > 0)
                {
                    _brain.motor.ApplyKnockback(knockbackDir, finalForce);
                }
            }
        }
        // -------------------------------------------

        // Avisa a IA (Reação)
        if (_ai != null)
            _ai.OnSuspiciousActivityDetected(transform.position - (Vector3)knockbackDir);

        // VFX de Dano
        if (_brain.stats.hitVFX != null)
            Instantiate(_brain.stats.hitVFX, transform.position, Quaternion.identity);

        StartCoroutine(FlashRed());

        if (_currentHealth <= 0) Die();
    }

    // (O resto do script permanece igual ao anterior)
    public void TakeDamage(float damage) => TakeDamage(damage, Vector2.zero, 0);

    void Die()
    {

        if (SceneGoalManagerVD.Instance != null)
        {
            SceneGoalManagerVD.Instance.OnEnemyDefeated();
        }

        _isDead = true;
        _brain.motor.Stop();
        _brain.motor.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        if (_ai) _ai.enabled = false;

        if (_brain.stats.isExploder)
        {
            if (_brain.stats.explosionVFX)
                Instantiate(_brain.stats.explosionVFX, transform.position, Quaternion.identity);
            if (_brain.stats.explosionSound)
                AudioSource.PlayClipAtPoint(_brain.stats.explosionSound, transform.position, 1f);
        }
        else
        {
            if (_brain.stats.deathVFX)
                Instantiate(_brain.stats.deathVFX, transform.position, Quaternion.identity);
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