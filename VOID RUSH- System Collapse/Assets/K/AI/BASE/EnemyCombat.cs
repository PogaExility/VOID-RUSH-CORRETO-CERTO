using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    private EnemyBrain _brain;
    private EnemyAnimationLink _animLink;
    private AudioSource _audioSource;
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
            // --- NOVO: Verificação de Linha de Visão para Ranged ---
            if (_brain.stats.isRanged)
            {
                if (!HasLineOfSight(target))
                {
                    // Tem parede na frente, não atira.
                    return;
                }
            }
            // -------------------------------------------------------

            _nextAttackTime = Time.time + _brain.stats.attackCooldown;
            StartCoroutine(PerformAttack(target));
        }
    }

    // Função auxiliar para verificar se tem parede no caminho
    bool HasLineOfSight(Transform target)
    {
        if (target == null) return false;

        Vector2 origin = _brain.attackPoint != null ? _brain.attackPoint.position : transform.position;
        Vector2 direction = target.position - (Vector3)origin;
        float distance = direction.magnitude;

        // Lança raio que colide com Target (Player) OU Obstacle (Parede/Chão)
        // Certifique-se de configurar a obstacleLayer no SO_EnemyStats
        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, _brain.stats.visionRange, _brain.stats.targetLayer | _brain.stats.obstacleLayer);

        if (hit.collider != null)
        {
            // Se bater em algo, verificamos se é o Player (TargetLayer)
            // Se a layer do objeto batido faz parte da TargetLayer, então temos visão.
            if (((1 << hit.collider.gameObject.layer) & _brain.stats.targetLayer) != 0)
            {
                return true;
            }
            // Se bateu em qualquer outra coisa (parede), retorna falso.
            return false;
        }

        return false;
    }

    IEnumerator PerformAttack(Transform target)
    {
        if (!_brain.stats.canMoveWhileAttacking) _brain.motor.Freeze(true);

        // --- LÓGICA DO KAMIKAZE ---
        if (_brain.stats.isExploder)
        {
            if (_animLink) _animLink.SetKamikazePrepare(true);

            if (_brain.stats.fuseSound && _audioSource)
                _audioSource.PlayOneShot(_brain.stats.fuseSound);

            float totalTime = _brain.stats.explosionFuseTime;

            yield return new WaitForSeconds(totalTime * 0.7f);

            if (_animLink) _animLink.TriggerFinalPhase();

            yield return new WaitForSeconds(totalTime * 0.3f);

            Collider2D[] explosionHits = Physics2D.OverlapCircleAll(transform.position, _brain.stats.explosionRadius, _brain.stats.targetLayer);
            foreach (var hit in explosionHits)
            {
                // Kamikaze usa explosionDamage
                ApplyDamageToTarget(hit, _brain.stats.explosionDamage);
            }

            if (_brain.stats.explosionVFX)
                Instantiate(_brain.stats.explosionVFX, transform.position, Quaternion.identity);

            if (_brain.stats.explosionSound)
                AudioSource.PlayClipAtPoint(_brain.stats.explosionSound, transform.position, 1f);

            Destroy(gameObject);
            yield break;
        }

        // --- LÓGICA PADRÃO (MELEE / RANGED) ---
        _brain.motor.FacePoint(target.position);

        if (_animLink) _animLink.TriggerAttackAnim();

        if (_brain.stats.attackSound && _audioSource)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.PlayOneShot(_brain.stats.attackSound);
        }

        yield return new WaitForSeconds(0.3f);

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
                // Melee usa damage normal
                ApplyDamageToTarget(hit, _brain.stats.damage);
            }
        }

        float waitTime = Mathf.Max(0.2f, _brain.stats.postAttackDelay);
        yield return new WaitForSeconds(waitTime);

        if (!_brain.stats.canMoveWhileAttacking) _brain.motor.Freeze(false);
    }

    // Alterei para aceitar valor de dano customizado (útil pro Kamikaze)
    void ApplyDamageToTarget(Collider2D hit, float damageAmount)
    {
        if (hit.TryGetComponent<PlayerStats>(out var player))
        {
            Vector2 knockDir = (hit.transform.position - transform.position).normalized;
            player.TakeDamage(damageAmount, knockDir, _brain.stats.knockbackPower);
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