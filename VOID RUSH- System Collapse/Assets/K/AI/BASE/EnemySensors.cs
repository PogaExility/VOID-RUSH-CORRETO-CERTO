using UnityEngine;

public class EnemySensors : MonoBehaviour
{
    private EnemyBrain _brain;
    void Start() => _brain = GetComponent<EnemyBrain>();

    public Collider2D ScanForPlayer()
    {
        return Physics2D.OverlapCircle(transform.position, _brain.stats.visionRange, _brain.stats.targetLayer);
    }

    public bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;
        float dist = Vector2.Distance(_brain.eyes.position, target.position);
        if (dist > _brain.stats.visionRange) return false;

        Vector2 dirToTarget = (target.position - _brain.eyes.position).normalized;
        Vector2 facingDir = _brain.motor.IsFacingRight ? Vector2.right : Vector2.left;

        if (Vector2.Angle(facingDir, dirToTarget) > _brain.stats.visionAngle / 2f) return false;

        Vector2 startPos = _brain.eyes.position + (Vector3)(dirToTarget * 0.2f);
        if (Physics2D.Raycast(startPos, dirToTarget, dist, _brain.stats.obstacleLayer)) return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return; // Esconde no Play

        if (_brain == null || _brain.stats == null || _brain.eyes == null || !_brain.showGizmos) return;

        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawSphere(_brain.eyes.position, _brain.stats.visionRange);

        Gizmos.color = Color.yellow;
        Vector3 forward = _brain.motor.IsFacingRight ? Vector3.right : Vector3.left;
        Vector3 leftRay = Quaternion.AngleAxis(-_brain.stats.visionAngle / 2, Vector3.forward) * forward;
        Vector3 rightRay = Quaternion.AngleAxis(_brain.stats.visionAngle / 2, Vector3.forward) * forward;

        Gizmos.DrawRay(_brain.eyes.position, leftRay * _brain.stats.visionRange);
        Gizmos.DrawRay(_brain.eyes.position, rightRay * _brain.stats.visionRange);
    }
}