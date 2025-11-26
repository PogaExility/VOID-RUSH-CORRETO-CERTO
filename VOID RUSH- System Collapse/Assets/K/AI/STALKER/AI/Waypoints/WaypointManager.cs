using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;
    private SmartWaypoint[] allWaypoints;

    void Awake()
    {
        Instance = this;
        allWaypoints = FindObjectsByType<SmartWaypoint>(FindObjectsSortMode.None);
    }

    public List<Vector3> GetPath(Vector3 startPos, Vector3 targetPos)
    {
        SmartWaypoint startNode = GetClosestWaypoint(startPos);
        SmartWaypoint endNode = GetClosestWaypoint(targetPos);

        if (startNode == null || endNode == null || startNode == endNode) return null;

        // Algoritmo BFS (Busca em Largura) - Simples e robusto
        Queue<SmartWaypoint> queue = new Queue<SmartWaypoint>();
        Dictionary<SmartWaypoint, SmartWaypoint> cameFrom = new Dictionary<SmartWaypoint, SmartWaypoint>();

        queue.Enqueue(startNode);
        cameFrom[startNode] = null;

        while (queue.Count > 0)
        {
            SmartWaypoint current = queue.Dequeue();
            if (current == endNode) break;

            foreach (var neighbor in current.neighbors)
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(endNode)) return null;

        // Reconstrói o caminho
        List<Vector3> path = new List<Vector3>();
        SmartWaypoint curr = endNode;
        while (curr != null)
        {
            path.Add(curr.transform.position);
            curr = cameFrom[curr];
        }
        path.Reverse();
        return path;
    }

    SmartWaypoint GetClosestWaypoint(Vector3 pos)
    {
        SmartWaypoint best = null;
        float minDst = float.MaxValue;
        foreach (var wp in allWaypoints)
        {
            float dst = Vector3.Distance(pos, wp.transform.position);
            if (dst < minDst) { minDst = dst; best = wp; }
        }
        return best;
    }
}