using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    private EnemyBrain _brain;
    private EnemyAnimationLink _animLink;
    private float _nextAttackTime;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _animLink = GetComponent<EnemyAnimationLink>();
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

        // --- LÓGICA KAMIKAZE ---
        if (_brain.stats.isExploder)
        {
            if (_animLink) _animLink.SetKamikazePrepare(true);

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            Color originalColor = (sr != null) ? sr.color : Color.white;

            float timer = 0;
            while (timer < _brain.stats.explosionFuseTime)
            {
                float interval = Mathf.Lerp(0.5f, 0.1f, timer / _brain.stats.explosionFuseTime);
                timer += interval;
                if (sr) sr.color = (sr.color == originalColor) ? Color.red : originalColor;
                yield return new WaitForSeconds(interval);
            }

            if (sr) sr.color = originalColor;

            // Explosão em área
            Collider2D[] explosionHits = Physics2D.OverlapCircleAll(transform.position, _brain.stats.explosionRadius, _brain.stats.targetLayer);
            foreach (var hit in explosionHits)
            {
                ApplyDamageToTarget(hit);
            }

            if (_brain.stats.explosionVFX) Instantiate(_brain.stats.explosionVFX, transform.position, Quaternion.identity);

            Destroy(gameObject);
            yield break;
        }

        // --- ATAQUE MELEE / RANGED ---
        _brain.motor.FacePoint(target.position);

        if (_animLink) _animLink.TriggerAttackAnim();

        yield return new WaitForSeconds(0.3f); // Delay do impacto

        if (_brain.stats.isRanged)
        {
            // PROJÉTIL
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
        else
        {
            // MELEE
            if (_brain.stats.meleeAttackVFX != null)
            {
                Instantiate(_brain.stats.meleeAttackVFX, _brain.attackPoint.position, _brain.attackPoint.rotation);
            }

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

    // --- AQUI ESTÁ A MÁGICA ---
    void ApplyDamageToTarget(Collider2D hit)
    {
        // Pega o PlayerStats
        if (hit.TryGetComponent<PlayerStats>(out var player))
        {
            // Calcula direção do empurrão
            Vector2 knockDir = (hit.transform.position - transform.position).normalized;

            // CHAMA A VERSÃO DE 3 ARGUMENTOS: Dano, Direção, Força
            player.TakeDamage(_brain.stats.damage, knockDir, _brain.stats.knockbackPower);
        }
    }

    void OnDrawGizmosSelected()
    {
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