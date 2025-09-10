using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    #region REFERENCES & STATE
    private AIPlatformerMotor _motor;
    #endregion

    #region CONFIGURATION
    [Header("▶ Pontos de Origem das Sondas (Transforms)")]
    public Transform wallProbeOrigin;
    public Transform ceilingProbeOrigin;
    // As sondas de borda de múltiplas camadas
    public Transform anticipationProbeOrigin;
    public Transform dangerProbeOrigin;
    public Transform ledgeProbeOrigin;
    // Sondas restantes
    public Transform jumpProbeOrigin;
    public Transform climbProbeOrigin;
    public Transform ledgeMantleProbeOrigin;

    [Header("▶ Configuração das Sondas")]
    public LayerMask groundLayer;
    public LayerMask climbableLayer;
    [Space]
    public float wallProbeDistance = 1.0f;
    public float ceilingProbeDistance = 1.0f;
    public float jumpProbeDistance = 2.0f;
    public float maxJumpHeight = 2.5f;
    public float ledgeMantleDistance = 1.0f;
    public float safeDropDepth = 3.0f;

    [Header("▶ Depuração Visual")]
    public bool showDebugGizmos = true;
    #endregion

    #region UNITY LIFECYCLE
    void Awake()
    {
        _motor = GetComponent<AIPlatformerMotor>();
    }
    #endregion

    #region PUBLIC API
    /// <summary>
    /// Contém os resultados de todas as sondas de navegação.
    /// </summary>
    public struct NavQueryResult
    {
        // A ESTRUTURA COMPLETA E CORRIGIDA
        public bool isGrounded;
        public bool canSafelyDrop;
        public bool wallAhead;
        public bool ceilingAhead;
        public bool anticipationLedgeAhead;
        public bool dangerLedgeAhead;
        public bool isAtLedge;
        public bool jumpablePlatformAhead;
        public bool climbableWallAhead;
        public bool canMantleLedge;
    }

    /// <summary>
    /// Executa todas as sondas e devolve os resultados.
    /// </summary>
    public NavQueryResult QueryEnvironment()
    {
        if (_motor == null) return new NavQueryResult();

        bool atLedge = ProbeForLedge(ledgeProbeOrigin);
        return new NavQueryResult
        {
            isGrounded = _motor.IsGrounded(),
            canSafelyDrop = atLedge && ProbeForSafeDrop(),
            wallAhead = ProbeForWall(),
            ceilingAhead = ProbeForCeiling(),
            // PREENCHIMENTO DOS CAMPOS QUE ESTAVAM EM FALTA
            anticipationLedgeAhead = ProbeForLedge(anticipationProbeOrigin),
            dangerLedgeAhead = ProbeForLedge(dangerProbeOrigin),
            isAtLedge = atLedge,
            jumpablePlatformAhead = ProbeForJumpablePlatform(),
            climbableWallAhead = ProbeForClimbableWall(),
            canMantleLedge = ProbeForLedgeMantle()
        };
    }
    #endregion

    #region SONDAS INTERNAS
    private bool ProbeForLedge(Transform origin)
    {
        if (origin == null) return false;
        return !Physics2D.Raycast(origin.position, Vector2.down, 1.0f, groundLayer);
    }

    private bool ProbeForWall() => wallProbeOrigin != null && Physics2D.Raycast(wallProbeOrigin.position, transform.right, wallProbeDistance, groundLayer);
    private bool ProbeForCeiling() => ceilingProbeOrigin != null && Physics2D.Raycast(ceilingProbeOrigin.position, transform.right, ceilingProbeDistance, groundLayer);
    private bool ProbeForJumpablePlatform() { if (jumpProbeOrigin == null) return false; RaycastHit2D hit = Physics2D.Raycast(jumpProbeOrigin.position, transform.right, jumpProbeDistance, groundLayer); return hit.collider && (hit.point.y - transform.position.y) < maxJumpHeight; }
    private bool ProbeForSafeDrop() { if (ledgeProbeOrigin == null) return false; return Physics2D.Raycast(ledgeProbeOrigin.position, Vector2.down, safeDropDepth, groundLayer); }
    private bool ProbeForClimbableWall() => climbProbeOrigin != null && Physics2D.Raycast(climbProbeOrigin.position, transform.right, wallProbeDistance, climbableLayer);
    private bool ProbeForLedgeMantle() => ledgeMantleProbeOrigin != null && _motor.IsClimbing && !Physics2D.Raycast(ledgeMantleProbeOrigin.position, transform.right, ledgeMantleDistance, groundLayer);
    #endregion

    #region VISUALIZAÇÃO
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        if (!Application.isPlaying) return;
        if (_motor == null) return;

        // Antecipação (Verde)
        if (anticipationProbeOrigin) { Gizmos.color = ProbeForLedge(anticipationProbeOrigin) ? Color.green : new Color(0, 0.5f, 0); Gizmos.DrawLine(anticipationProbeOrigin.position, anticipationProbeOrigin.position + Vector3.down * 1.0f); }
        // Perigo (Amarelo)
        if (dangerProbeOrigin) { Gizmos.color = ProbeForLedge(dangerProbeOrigin) ? Color.yellow : new Color(0.5f, 0.5f, 0); Gizmos.DrawLine(dangerProbeOrigin.position, dangerProbeOrigin.position + Vector3.down * 1.0f); }
        // Borda Imediata (Vermelho)
        if (ledgeProbeOrigin) { Gizmos.color = ProbeForLedge(ledgeProbeOrigin) ? Color.red : new Color(0.5f, 0, 0); Gizmos.DrawLine(ledgeProbeOrigin.position, ledgeProbeOrigin.position + Vector3.down * 1.0f); }

        if (wallProbeOrigin) { Gizmos.color = ProbeForWall() ? Color.red : new Color(0.5f, 0, 0); Gizmos.DrawLine(wallProbeOrigin.position, wallProbeOrigin.position + transform.right * wallProbeDistance); }
        if (ceilingProbeOrigin) { Gizmos.color = ProbeForCeiling() ? Color.yellow : new Color(0.5f, 0.5f, 0); Gizmos.DrawLine(ceilingProbeOrigin.position, ceilingProbeOrigin.position + transform.right * ceilingProbeDistance); }
        if (jumpProbeOrigin) { Gizmos.color = ProbeForJumpablePlatform() ? Color.magenta : new Color(0.5f, 0, 0.5f); Gizmos.DrawLine(jumpProbeOrigin.position, jumpProbeOrigin.position + transform.right * jumpProbeDistance); }
        if (climbProbeOrigin) { Gizmos.color = ProbeForClimbableWall() ? Color.green : new Color(0, 0.5f, 0); Gizmos.DrawLine(climbProbeOrigin.position, climbProbeOrigin.position + transform.right * wallProbeDistance); }
        if (ledgeMantleProbeOrigin) { Gizmos.color = Color.white; Gizmos.DrawLine(ledgeMantleProbeOrigin.position, ledgeMantleProbeOrigin.position + transform.right * ledgeMantleDistance); }
    }
    #endregion
}