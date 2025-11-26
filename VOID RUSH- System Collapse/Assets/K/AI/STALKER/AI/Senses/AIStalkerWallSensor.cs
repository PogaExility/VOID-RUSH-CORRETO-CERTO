using UnityEngine;

public struct WallIntel
{
    public bool FoundWall;
    public bool IsPassableHole;
    public Vector2 TargetClimbPos;
    public bool RequiresCrouch;
}

public class AIStalkerWallSensor : MonoBehaviour
{
    [Header("Configuração de Scan")]
    public Transform eyeLevelProbe; // Coloque na altura dos olhos
    public float scanDistance = 0.8f;
    public float maxClimbHeight = 4.0f;
    public LayerMask obstacleLayer;

    // Executa um scan rápido (Raycast) para não travar o jogo
    public WallIntel QuickScan()
    {
        WallIntel intel = new WallIntel();
        float dir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 origin = eyeLevelProbe.position;

        // 1. Verifica Parede na frente
        RaycastHit2D wallHit = Physics2D.Raycast(origin, Vector2.right * dir, scanDistance, obstacleLayer);

        if (!wallHit)
        {
            intel.FoundWall = false;
            return intel;
        }

        intel.FoundWall = true;
        float wallDistance = wallHit.distance;

        // 2. Verifica Buraco (Logic do "Legacy" simplificada)
        // Verifica 1 metro acima e 1 metro abaixo
        bool blockedUp = Physics2D.Raycast(origin + Vector2.up, Vector2.right * dir, scanDistance, obstacleLayer);
        bool blockedDown = Physics2D.Raycast(origin + Vector2.down, Vector2.right * dir, scanDistance, obstacleLayer);

        if (!blockedUp && !blockedDown)
        {
            // Situação estranha, mas possível. Prioriza passar pelo meio.
            intel.IsPassableHole = true;
            intel.TargetClimbPos = (Vector2)origin + (Vector2.right * dir * (wallDistance + 0.5f));
            return intel;
        }

        // 3. Verifica Topo da Parede (Para Vault/Escalada)
        // Dispara raios de cima para baixo para achar a altura da parede
        for (float h = 1f; h <= maxClimbHeight; h += 0.5f)
        {
            Vector2 checkPos = origin + (Vector2.up * h) + (Vector2.right * dir * (wallDistance + 0.2f));
            if (!Physics2D.OverlapPoint(checkPos, obstacleLayer))
            {
                // Achamos um espaço vazio acima da parede!
                // Agora verifica se cabe o personagem (Crouch check)
                bool ceilingCheck = Physics2D.Raycast(checkPos, Vector2.up, 1.0f, obstacleLayer);

                intel.TargetClimbPos = checkPos;
                intel.IsPassableHole = false; // É uma escalada de topo
                intel.RequiresCrouch = ceilingCheck;
                return intel;
            }
        }

        // Se chegou aqui, é uma parede muito alta (High Wall)
        intel.FoundWall = true;
        intel.TargetClimbPos = Vector2.zero; // Sem solução imediata
        return intel;
    }

    void OnDrawGizmos()
    {
        if (eyeLevelProbe != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyeLevelProbe.position, eyeLevelProbe.position + transform.right * transform.localScale.x * scanDistance);
        }
    }
}