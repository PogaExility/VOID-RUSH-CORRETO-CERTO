using UnityEngine;

namespace K.Pathfinding
{
    public enum ActionType { Walk, Crouch, Jump, Climb, Fall }

    public class Node
    {
        public bool walkable;       // É um bloco vazio? (Não é parede)
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;

        public int verticalClearance; // Quantos blocos livres acima deste (incluindo este)
        public ActionType action;     // Ação para chegar aqui

        // A* Costs
        public int gCost;
        public int hCost;
        public Node parent;

        public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
        {
            walkable = _walkable;
            worldPosition = _worldPos;
            gridX = _gridX;
            gridY = _gridY;
        }

        public int fCost { get { return gCost + hCost; } }
    }
}