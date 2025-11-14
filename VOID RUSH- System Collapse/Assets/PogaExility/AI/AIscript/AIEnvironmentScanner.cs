using UnityEngine;

/// <summary>
/// Define os tipos de anomalias de terreno que o scanner pode detectar.
/// </summary>
public enum PathObstacleType
{
    None,               // Caminho está livre.
    Wall,               // Uma parede alta e intransponível.
    JumpableObstacle,   // Um obstáculo baixo que pode ser pulado.
    Ledge,              // Um buraco ou beirada à frente.
}

/// <summary>
/// Um "relatório" detalhado que o scanner envia ao cérebro (AIController)
/// descrevendo o que foi encontrado no caminho à frente.
/// </summary>
public struct NavigationQueryResult
{
    public PathObstacleType ObstacleType; // O tipo de obstáculo encontrado.
    public float ObstacleHeight;          // A altura do obstáculo (se for pulável).
    public float LedgeGapWidth;           // A largura do buraco (se for uma beirada).
}

/// <summary>
/// O módulo de "Scanner de Ambiente" da IA. Este componente é responsável por
/// analisar o terreno à frente da IA para detectar paredes, obstáculos e buracos,
/// permitindo uma navegação de plataforma mais inteligente.
/// </summary>
public class AIEnvironmentScanner : MonoBehaviour
{
    [Header("▶ Referências das Sondas")]
    [Tooltip("Sonda para detectar paredes altas.")]
    [SerializeField] private Transform wallProbe;
    [Tooltip("Sonda para detectar obstáculos baixos (na altura do joelho).")]
    [SerializeField] private Transform obstacleProbe;
    [Tooltip("Sonda para detectar o início de um buraco.")]
    [SerializeField] private Transform ledgeProbe;

    [Header("▶ Configuração das Sondas")]
    [Tooltip("Distância horizontal que as sondas de parede e obstáculo verificam.")]
    [SerializeField] private float horizontalProbeDistance = 0.5f;
    [Tooltip("Distância horizontal que a sonda de beirada verifica.")]
    [SerializeField] private float ledgeProbeDistance = 0.8f;
    [Tooltip("Altura máxima que a IA tentará medir para um obstáculo pulável.")]
    [SerializeField] private float maxObstacleHeightScan = 2f;

    // Módulos e referências internas
    private AIMovement motor;
    private LayerMask obstacleLayer;

    #region Inicialização

    private void Awake()
    {
        motor = GetComponentInParent<AIMovement>();
    }

    /// <summary>
    /// Inicializa o scanner com a LayerMask unificada de obstáculos.
    /// </summary>
    public void Initialize(LayerMask obstacles)
    {
        this.obstacleLayer = obstacles;
    }

    #endregion

    #region API Pública de Análise

    /// <summary>
    /// A função principal. Analisa o caminho à frente e retorna um relatório detalhado.
    /// </summary>
    /// <returns>Um struct NavigationQueryResult com os resultados da análise.</returns>
    public NavigationQueryResult AnalyzePathAhead()
    {
        // Verifica as anomalias em ordem de prioridade.

        // 1. Há um buraco à frente?
        if (IsLedgeAhead(out float gapWidth))
        {
            return new NavigationQueryResult { ObstacleType = PathObstacleType.Ledge, LedgeGapWidth = gapWidth };
        }

        // 2. Se não há buraco, há um obstáculo baixo que pode ser pulado?
        if (IsJumpableObstacleAhead(out float obstacleHeight))
        {
            return new NavigationQueryResult { ObstacleType = PathObstacleType.JumpableObstacle, ObstacleHeight = obstacleHeight };
        }

        // 3. Se não, há uma parede alta?
        if (IsWallAhead())
        {
            return new NavigationQueryResult { ObstacleType = PathObstacleType.Wall };
        }

        // 4. Se nada foi detectado, o caminho está livre.
        return new NavigationQueryResult { ObstacleType = PathObstacleType.None };
    }

    #endregion

    #region Lógica Interna dos Sensores

    private bool IsWallAhead()
    {
        if (wallProbe == null) return false;
        Vector2 direction = motor.IsFacingRight ? Vector2.right : Vector2.left;
        return Physics2D.Raycast(wallProbe.position, direction, horizontalProbeDistance, obstacleLayer);
    }

    private bool IsJumpableObstacleAhead(out float height)
    {
        height = 0f;
        if (obstacleProbe == null) return false;

        Vector2 direction = motor.IsFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(obstacleProbe.position, direction, horizontalProbeDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // Mede a altura do obstáculo a partir do ponto de colisão.
            RaycastHit2D topHit = Physics2D.Raycast(hit.point + Vector2.up * maxObstacleHeightScan, Vector2.down, maxObstacleHeightScan, obstacleLayer);
            if (topHit.collider != null)
            {
                height = topHit.point.y - transform.position.y; // Altura relativa à base da IA.
                return true;
            }
        }
        return false;
    }

    private bool IsLedgeAhead(out float gapWidth)
    {
        gapWidth = 0f;
        if (ledgeProbe == null) return false;

        Vector2 direction = motor.IsFacingRight ? Vector2.right : Vector2.left;

        // Sonda 1: Verifica se o espaço logo à frente está VAZIO.
        bool isGap = !Physics2D.Raycast(ledgeProbe.position, direction, ledgeProbeDistance, obstacleLayer);

        if (isGap)
        {
            // Sonda 2: Se está vazio, verifica a que distância está o próximo chão.
            Vector2 probeOrigin = (Vector2)ledgeProbe.position + (direction * ledgeProbeDistance);
            RaycastHit2D hit = Physics2D.Raycast(probeOrigin, Vector2.down, 10f, obstacleLayer);

            if (hit.collider != null)
            {
                // A "largura do buraco" é a distância horizontal até o próximo chão.
                gapWidth = Vector2.Distance(probeOrigin, hit.point);
            }
            else
            {
                // Se não encontrou chão, é um abismo.
                gapWidth = float.MaxValue;
            }
            return true;
        }

        return false;
    }

    #endregion

    #region Gizmos para Debug

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (motor == null) return;
        Vector2 direction = motor.IsFacingRight ? Vector2.right : Vector2.left;

        // Gizmo da sonda de parede
        if (wallProbe != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(wallProbe.position, (Vector2)wallProbe.position + (direction * horizontalProbeDistance));
        }

        // Gizmo da sonda de obstáculo
        if (obstacleProbe != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(obstacleProbe.position, (Vector2)obstacleProbe.position + (direction * horizontalProbeDistance));
        }

        // Gizmos da sonda de beirada/buraco
        if (ledgeProbe != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(ledgeProbe.position, (Vector2)ledgeProbe.position + (direction * ledgeProbeDistance));

            // Desenha a sonda secundária que mede a largura do buraco
            Vector2 secondaryProbeOrigin = (Vector2)ledgeProbe.position + (direction * ledgeProbeDistance);
            Gizmos.DrawLine(secondaryProbeOrigin, secondaryProbeOrigin + Vector2.down * 10f);
        }
    }
#endif

    #endregion
}