using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    private EnemyBrain _brain;
    private EnemyAnimationLink _animLink;
    private AudioSource _audioSource; // Referência ao AudioSource do Health
    private float _nextAttackTime;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _animLink = GetComponent<EnemyAnimationLink>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void TryAttack(Transform target)
    {
        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + _brain.stats.attackCooldown;
            StartCoroutine(PerformAttack(target));
        }
    }

    IEnumerator PerformAttack(Transform target)
    {
        if (!_brain.stats.canMoveWhileAttacking) _brain.motor.Freeze(true);

        // --- LÓGICA DO KAMIKAZE ---
        if (_brain.stats.isExploder)
        {
            // FASE 1: AVISO + SOM DO PAVIO
            if (_animLink) _animLink.SetKamikazePrepare(true);

            // Toca som do pavio/frequência subindo
            if (_brain.stats.fuseSound && _audioSource)
                _audioSource.PlayOneShot(_brain.stats.fuseSound);

            float totalTime = _brain.stats.explosionFuseTime;

            yield return new WaitForSeconds(totalTime * 0.7f);

            // FASE 2: INFLAR
            if (_animLink) _animLink.TriggerFinalPhase();

            yield return new WaitForSeconds(totalTime * 0.3f);

            // FASE 3: EXPLOSÃO
            Collider2D[] explosionHits = Physics2D.OverlapCircleAll(transform.position, _brain.stats.explosionRadius, _brain.stats.targetLayer);
            foreach (var hit in explosionHits)
            {
                ApplyDamageToTarget(hit);
            }

            if (_brain.stats.explosionVFX)
                Instantiate(_brain.stats.explosionVFX, transform.position, Quaternion.identity);

            // Som da explosão (PlayClipAtPoint pois o objeto será destruído)
            if (_brain.stats.explosionSound)
                AudioSource.PlayClipAtPoint(_brain.stats.explosionSound, transform.position, 1f);

            Destroy(gameObject);
            yield break;
        }

        // --- LÓGICA PADRÃO (MELEE / RANGED) ---
        _brain.motor.FacePoint(target.position);

        if (_animLink) _animLink.TriggerAttackAnim();

        // Som de Ataque (Soco ou Disparo)
        if (_brain.stats.attackSound && _audioSource)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.PlayOneShot(_brain.stats.attackSound);
        }

        yield return new WaitForSeconds(0.3f); // Delay do impacto

        if (_brain.stats.isRanged)
        {
            if (_brain.stats.projectilePrefab)
            {
                GameObject p = Instantiate(_brain.stats.projectilePrefab, _brain.attackPoint.position, Quaternion.identity);
                Vector2 dir = (target.position - _brain.attackPoint.position).normalized;

                if (p.TryGetComponent<EnemyProjectile>(out var script))
                {
                    script.Initialize(_brain.stats.damage, _brain.stats.knockbackPower, dir, _brain.stats.projectileSpeed);
                }
            }
        }
        else // Melee
        {
            if (_brain.stats.meleeAttackVFX != null)
                Instantiate(_brain.stats.meleeAttackVFX, _brain.attackPoint.position, _brain.attackPoint.rotation);

            Vector2 center = (Vector2)transform.position + new Vector2(_brain.stats.hitboxOffset.x * transform.localScale.x, _brain.stats.hitboxOffset.y);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, _brain.stats.hitboxSize, 0, _brain.stats.targetLayer);

            foreach (var hit in hits)
            {
                ApplyDamageToTarget(hit);
            }
        }

        float waitTime = Mathf.Max(0.2f, _brain.stats.postAttackDelay);
        yield return new WaitForSeconds(waitTime);

        if (!_brain.stats.canMoveWhileAttacking) _brain.motor.Freeze(false);
    }

    void ApplyDamageToTarget(Collider2D hit)
    {
        if (hit.TryGetComponent<PlayerStats>(out var player))
        {
            Vector2 knockDir = (hit.transform.position - transform.position).normalized;
            player.TakeDamage(_brain.stats.damage, knockDir, _brain.stats.knockbackPower);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        if (_brain && _brain.stats)
        {
            Gizmos.color = Color.red;
            if (_brain.stats.isExploder)
            {
                Gizmos.DrawWireSphere(transform.position, _brain.stats.explosionRadius);
            }
            else if (!_brain.stats.isRanged)
            {
                Vector2 center = (Vector2)transform.position + new Vector2(_brain.stats.hitboxOffset.x * transform.localScale.x, _brain.stats.hitboxOffset.y);
                Gizmos.DrawWireCube(center, _brain.stats.hitboxSize);
            }
        }
    }
}