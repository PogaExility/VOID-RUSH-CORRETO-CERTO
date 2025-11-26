using UnityEngine;
using System.Collections.Generic;

namespace K.Pathfinding
{
    public static class Pathfinding
    {
        public static List<Node> FindPath(Vector3 startPos, Vector3 targetPos, Grid grid)
        {
            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node targetNode = grid.NodeFromWorldPoint(targetPos);

            // --- CORREÇÃO DE TOLERÂNCIA ---
            // Se o nó exato não for caminhável (parede ou limite), procura vizinhos
            // Isso resolve o problema de "Player pulando" ou "Clipando na parede"
            if (!IsNodeValid(startNode)) startNode = FindClosestWalkableNode(startNode, grid);
            if (!IsNodeValid(targetNode)) targetNode = FindClosestWalkableNode(targetNode, grid);

            if (startNode == null || targetNode == null) return null;

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (closedSet.Contains(neighbour)) continue;

                    int moveCost = GetDistance(currentNode, neighbour);

                    // Penalidades
                    if (neighbour.action == ActionType.Climb) moveCost += 10;
                    if (neighbour.action == ActionType.Crouch) moveCost += 15;

                    int newMovementCostToNeighbour = currentNode.gCost + moveCost;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
            return null;
        }

        static bool IsNodeValid(Node n)
        {
            // É válido se não for nulo e for "walkable" (espaço vazio)
            return n != null && n.walkable;
        }

        // Procura em espiral por um nó válido próximo
        static Node FindClosestWalkableNode(Node origin, Grid grid)
        {
            if (IsNodeValid(origin)) return origin;

            // Busca profundidade 2
            foreach (Node n in grid.GetNeighbours(origin))
            {
                if (IsNodeValid(n)) return n;
                foreach (Node n2 in grid.GetNeighbours(n))
                {
                    if (IsNodeValid(n2)) return n2;
                }
            }
            return null;
        }

        static List<Node> RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Reverse();
            return path;
        }

        static int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
            return (dstX > dstY) ? 14 * dstY + 10 * (dstX - dstY) : 14 * dstX + 10 * (dstY - dstX);
        }
    }
}