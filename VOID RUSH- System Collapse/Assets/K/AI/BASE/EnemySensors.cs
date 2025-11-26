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
        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > _brain.stats.visionRange) return false;

        Vector2 dirToTarget = (target.position - transform.position).normalized;
        float dirX = transform.localScale.x;

        // Verifica ângulo (está na frente?)
        if (Vector2.Angle(new Vector2(dirX, 0), dirToTarget) > _brain.stats.visionAngle / 2f) return false;

        // Raycast para ver se tem parede
        if (Physics2D.Raycast(transform.position, dirToTarget, dist, _brain.stats.obstacleLayer)) return false;

        return true;
    }

    void OnDrawGizmosSelected() { /* (Igual ao anterior, desenha cone) */ }
}