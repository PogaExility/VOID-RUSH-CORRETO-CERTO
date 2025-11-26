using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    private EnemyBrain _brain;
    private float _nextAttackTime;

    void Start() => _brain = GetComponent<EnemyBrain>();

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

        _brain.motor.FacePoint(target.position);

        yield return new WaitForSeconds(0.3f);

        if (_brain.stats.isRanged)
        {
            if (_brain.stats.projectilePrefab)
            {
                GameObject p = Instantiate(_brain.stats.projectilePrefab, _brain.attackPoint.position, Quaternion.identity);
                Vector2 dir = (target.position - _brain.attackPoint.position).normalized;

                if (p.TryGetComponent<EnemyProjectile>(out var script))
                {
                    // CORREÇÃO: Usando knockbackPower
                    script.Initialize(_brain.stats.damage, _brain.stats.knockbackPower, dir, _brain.stats.projectileSpeed);
                }
            }
        }
        else
        {
            Vector2 center = (Vector2)transform.position + new Vector2(_brain.stats.hitboxOffset.x * transform.localScale.x, _brain.stats.hitboxOffset.y);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, _brain.stats.hitboxSize, 0, _brain.stats.targetLayer);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<PlayerStats>(out var player))
                {
                    Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                    // CORREÇÃO: Usando knockbackPower
                    player.TakeDamage(_brain.stats.damage, knockDir, _brain.stats.knockbackPower);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
        if (!_brain.stats.canMoveWhileAttacking) _brain.motor.Freeze(false);
    }

    void OnDrawGizmosSelected()
    {
        if (_brain && _brain.stats && !_brain.stats.isRanged)
        {
            Gizmos.color = Color.red;
            Vector2 center = (Vector2)transform.position + new Vector2(_brain.stats.hitboxOffset.x * transform.localScale.x, _brain.stats.hitboxOffset.y);
            Gizmos.DrawWireCube(center, _brain.stats.hitboxSize);
        }
    }
}