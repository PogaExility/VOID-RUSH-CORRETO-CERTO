using UnityEngine;
using System.Collections.Generic;

public class SmartWaypoint : MonoBehaviour
{
    [Header("Conexões")]
    [Tooltip("Arraste aqui os outros Waypoints que o Stalker pode alcançar a partir deste.")]
    public List<SmartWaypoint> neighbors;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (neighbors != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null)
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}