using UnityEngine;

public class PathNode
{
    public int gridX, gridY;
    public bool isWalkable;
    public Vector3 worldPosition;

    public int gCost;
    public int hCost;
    public PathNode cameFromNode;

    public PathNode(int _gridX, int _gridY, Vector3 _worldPos, bool _isWalkable)
    {
        gridX = _gridX;
        gridY = _gridY;
        worldPosition = _worldPos;
        isWalkable = _isWalkable;
    }

    public int fCost { get { return gCost + hCost; } }
}