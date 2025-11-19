using UnityEngine;
using System.Collections.Generic;

public class PathfindingGrid : MonoBehaviour
{
    public static PathfindingGrid Instance { get; private set; }

    [Header("Configuração da Grade")]
    public LayerMask unwalkableMask; // Defina como a Layer do Chão/Paredes
    public Vector2 gridWorldSize;
    public float nodeRadius = 0.25f;

    PathNode[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        Instance = this;
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new PathNode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);

                // Lógica 2D: Um nó é "Andável" se NÃO estiver sobrepondo uma parede/chão.
                // O nó representa o ESPAÇO VAZIO onde o personagem pisa/passa.
                bool walkable = !(Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.8f, unwalkableMask));
                grid[x, y] = new PathNode(x, y, worldPoint, walkable);
            }
        }
    }

    public PathNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y - transform.position.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public List<PathNode> GetNeighbours(PathNode node)
    {
        List<PathNode> neighbours = new List<PathNode>();

        // Verifica vizinhos em volta (incluindo diagonais para facilitar detecção de pulos)
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));
        if (grid != null && Application.isPlaying) // Só desenha nós individuais se estiver jogando para não pesar o editor
        {
            foreach (PathNode n in grid)
            {
                if (!n.isWalkable) // Desenha apenas os obstáculos para visualização limpa
                {
                    Gizmos.color = new Color(1, 0, 0, 0.3f);
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .05f));
                }
            }
        }
    }
}