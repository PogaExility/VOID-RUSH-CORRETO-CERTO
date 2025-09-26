using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    public enum ObstacleType { None, CrouchTunnel, JumpablePlatform, FullWall, Ledge }

    #region REFERENCES & STATE
    private AIPlatformerMotor _motor;
    #endregion

    #region CONFIGURATION
    [Header("▶ Pontos de Origem das Sondas (Transforms)")]
    [Tooltip("Sensor baixo (altura dos pés) para detectar a base da parede.")]
    public Transform lowerWallProbe;
    [Tooltip("Sensor alto (altura da cabeça) para detectar a parte de cima da parede.")]
    public Transform upperWallProbe;
    [Tooltip("Sensor frontal (altura da cabeça) para detectar tetos baixos.")]
    public Transform ceilingProbe;
    [Tooltip("Sensor para verificar espaço livre na altura da cabeça.")]
    public Transform headSpaceProbe;
    [Tooltip("Sensor de contato com borda (apenas para quedas).")]
    public Transform ledgeProbe;
    [Header("▶ Configuração das Sondas")]
    public LayerMask groundLayer;
    public LayerMask climbableLayer;
    public float wallProbeDistance = 0.2f;
    public float ceilingProbeDistance = 0.2f; // Nova variável para controle da sonda de teto
    [Header("▶ Depuração Visual")]
    public bool showDebugGizmos = true;
    #endregion

    #region UNITY LIFECYCLE
    void Awake()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        if (_motor == null) { Debug.LogError("AINavigationSystem: AIPlatformerMotor não encontrado!", this); this.enabled = false; }
    }
    #endregion

    #region PUBLIC API
    public struct NavQueryResult
    {
        public ObstacleType detectedObstacle;
        public bool isGrounded;
    }

    public NavQueryResult QueryEnvironment()
    {
        var result = new NavQueryResult
        {
            isGrounded = _motor.IsGrounded(),
            detectedObstacle = ObstacleType.None
        };

        bool seesLowerWall = ProbeForWall(lowerWallProbe);

        if (seesLowerWall)
        {
            bool seesUpperWall = ProbeForWall(upperWallProbe);

            if (!seesUpperWall)
            {
                result.detectedObstacle = ObstacleType.JumpablePlatform;
            }
            else
            {
                // CORREÇÃO CRÍTICA: Utiliza a nova sonda vertical para o teto.
                bool seesCeiling = ProbeForCeiling(ceilingProbe);
                bool headSpaceIsClear = !ProbeForWall(headSpaceProbe);

                if (seesCeiling && headSpaceIsClear)
                {
                    result.detectedObstacle = ObstacleType.CrouchTunnel;
                }
                else
                {
                    result.detectedObstacle = ObstacleType.FullWall;
                }
            }
        }

        if (result.detectedObstacle == ObstacleType.None && ProbeForLedge(ledgeProbe))
        {
            result.detectedObstacle = ObstacleType.Ledge;
        }

        return result;
    }
    #endregion

    #region SONDAS INTERNAS
    private bool ProbeForWall(Transform origin) => origin != null && Physics2D.Raycast(origin.position, transform.right, wallProbeDistance, groundLayer);

    // NOVO MÉTODO: Sonda dedicada para o teto, que lança o raio para CIMA.
    private bool ProbeForCeiling(Transform origin) => origin != null && Physics2D.Raycast(origin.position, Vector2.up, ceilingProbeDistance, groundLayer);

    private bool ProbeForLedge(Transform origin) => origin != null && !Physics2D.Raycast(origin.position, Vector2.down, 1.0f, groundLayer);
    #endregion

    #region VISUALIZAÇÃO
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying || _motor == null) return;
        DrawProbeGizmo(lowerWallProbe, ProbeForWall(lowerWallProbe), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(upperWallProbe, ProbeForWall(upperWallProbe), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(headSpaceProbe, !ProbeForWall(headSpaceProbe), Color.cyan, transform.right, wallProbeDistance);

        // CORREÇÃO CRÍTICA: Atualiza a visualização da sonda de teto para apontar para CIMA.
        DrawProbeGizmo(ceilingProbe, ProbeForCeiling(ceilingProbe), Color.yellow, Vector3.up, ceilingProbeDistance);

        DrawProbeGizmo(ledgeProbe, ProbeForLedge(ledgeProbe), Color.magenta, Vector3.down, 1.0f);
    }

    // Método de desenho do Gizmo atualizado para aceitar direção e distância.
    private void DrawProbeGizmo(Transform origin, bool isActive, Color activeColor, Vector3 direction, float distance)
    {
        if (origin == null) return;
        Color inactiveColor = new Color(activeColor.r * 0.3f, activeColor.g * 0.3f, activeColor.b * 0.3f, 0.5f);
        Gizmos.color = isActive ? activeColor : inactiveColor;
        Gizmos.DrawLine(origin.position, origin.position + direction * distance);
        if (isActive) Gizmos.DrawCube(origin.position + direction * distance, Vector3.one * 0.1f);
    }
    #endregion
}