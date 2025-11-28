using UnityEngine;

public class EnemyPatrolPoint : MonoBehaviour
{
    [Tooltip("Tempo parado aqui.")]
    public float waitTime = 2f;
    [Tooltip("Olhar para Direita?")]
    public bool faceRight = true;

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return; // Esconde no Play

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Vector3 dir = faceRight ? Vector3.right : Vector3.left;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, dir * 1f);
    }
}