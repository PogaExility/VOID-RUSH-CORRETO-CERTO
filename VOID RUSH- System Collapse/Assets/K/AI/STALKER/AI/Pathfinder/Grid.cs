using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace K.Pathfinding
{
    public class Grid : MonoBehaviour
    {
        [Header("Layers")]
        public LayerMask unwalkableMask; // Paredes, Chão, Teto

        [Header("Mundo")]
        public Vector2 gridWorldSize;
        public float nodeRadius = 0.5f; // Tiles de 1x1 (Raio 0.5 = Diametro 1.0)

        [Header("Debug")]
        public bool showClearanceNumbers = false; // Ative para ver os números na tela

        Node[,] grid;
        float nodeDiameter;
        int gridSizeX, gridSizeY;

        void Awake()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
            CreateGrid();
        }

        public void CreateGrid()
        {
            grid = new Node[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

            // Desativa colisão com a própria IA durante o scan
            bool oldQueries = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = false;

            // 1. GERAÇÃO BASE (Detectar Paredes Sólidas)
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);

                    // CORREÇÃO: Usar Box em vez de Circle para Tiles Quadrados
                    // Tamanho 0.95 garante que não pegue vizinhos diagonais, mas pegue o teto cheio.
                    Vector2 checkSize = Vector2.one * (nodeDiameter * 0.95f);

                    bool walkable = !Physics2D.OverlapBox(worldPoint, checkSize, 0, unwalkableMask);

                    grid[x, y] = new Node(walkable, worldPoint, x, y);
                }
            }

            // 2. CÁLCULO DE CLEARANCE (Quantos blocos vazios existem ACIMA deste)
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (!grid[x, y].walkable)
                    {
                        grid[x, y].verticalClearance = 0;
                        continue;
                    }

                    int clearance = 0;
                    // Verifica para cima. Se o tile é 1x1, checamos os indices Y+1, Y+2...
                    // Loop começa em 0 para contar o próprio bloco (chão onde pisa) como 1 de espaço
                    for (int i = 0; i < 4; i++)
                    {
                        int checkY = y + i;
                        if (checkY < gridSizeY && grid[x, checkY].walkable)
                            clearance++;
                        else
                            break; // Encontrou teto, para de contar
                    }
                    grid[x, y].verticalClearance = clearance;
                }
            }

            Physics2D.queriesHitTriggers = oldQueries;
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            int[] xDir = { 0, 0, -1, 1 };
            int[] yDir = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int checkX = node.gridX + xDir[i];
                int checkY = node.gridY + yDir[i];

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    Node neighbour = grid[checkX, checkY];

                    // ESCALADA (Cima)
                    if (yDir[i] == 1)
                    {
                        // Para subir, precisa ser walkable (ar)
                        if (neighbour.walkable)
                        {
                            bool wallLeft = IsWall(node.gridX - 1, node.gridY);
                            bool wallRight = IsWall(node.gridX + 1, node.gridY);

                            if (wallLeft || wallRight)
                            {
                                neighbour.action = ActionType.Climb;
                                neighbours.Add(neighbour);
                            }
                            // Pulo: Só se tiver espaço para o corpo inteiro (3 tiles)
                            else if (neighbour.verticalClearance >= 3)
                            {
                                neighbour.action = ActionType.Jump;
                                neighbours.Add(neighbour);
                            }
                        }
                    }
                    // QUEDA (Baixo)
                    else if (yDir[i] == -1)
                    {
                        if (neighbour.walkable)
                        {
                            neighbour.action = ActionType.Fall;
                            neighbours.Add(neighbour);
                        }
                    }
                    // ANDAR (Lado)
                    else
                    {
                        if (!neighbour.walkable) continue;

                        // Se clearance for 3 ou mais -> Pode Andar (2.9m)
                        if (neighbour.verticalClearance >= 3)
                        {
                            neighbour.action = ActionType.Walk;
                            neighbours.Add(neighbour);
                        }
                        // Se clearance for 2 -> Pode Agachar (1.9m)
                        else if (neighbour.verticalClearance == 2)
                        {
                            neighbour.action = ActionType.Crouch;
                            neighbours.Add(neighbour);
                        }
                        // Se clearance for 1 -> NÃO PASSA. (Buraco de 1m)
                    }
                }
            }
            return neighbours;
        }

        bool IsWall(int x, int y)
        {
            if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY) return true;
            return !grid[x, y].walkable;
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
            return grid[x, y];
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));
            if (grid != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.black;
                style.fontSize = 10;
                style.alignment = TextAnchor.MiddleCenter;

                foreach (Node n in grid)
                {
                    if (!n.walkable)
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.3f); // Parede
                        Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                    }
                    else
                    {
                        // Cores baseadas no clearance
                        if (n.verticalClearance >= 3) Gizmos.color = new Color(0, 1, 0, 0.1f); // Livre (3+)
                        else if (n.verticalClearance == 2) Gizmos.color = new Color(0, 0, 1, 0.4f); // Agachar (2)
                        else Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // Bloqueado/Muito Baixo (1)

                        Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));

#if UNITY_EDITOR
                        if (showClearanceNumbers)
                        {
                            Handles.Label(n.worldPosition, n.verticalClearance.ToString(), style);
                        }
#endif
                    }
                }
            }
        }
    }
}