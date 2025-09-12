using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    #region REFERENCES & STATE
    private AIPlatformerMotor _motor;
    #endregion

    #region CONFIGURATION
    [Header("▶ Pontos de Origem das Sondas (Transforms)")]
    public Transform anticipationWallProbeOrigin;
    public Transform dangerWallProbeOrigin;
    public Transform contactWallProbeOrigin;
    public Transform anticipationLedgeProbeOrigin;
    public Transform dangerLedgeProbeOrigin;
    public Transform ledgeProbeOrigin;
    public Transform ceilingProbeOrigin;
    public Transform jumpProbeOrigin;
    public Transform climbProbeOrigin;
    public Transform ledgeMantleProbeOrigin;

    [Header("▶ Configuração das Sondas")]
    public LayerMask groundLayer;
    public LayerMask climbableLayer;
    [Space]
    public float wallProbeDistance = 0.2f;
    public float ceilingProbeDistance = 1.0f;
    public float jumpProbeDistance = 2.0f;
    public float maxJumpHeight = 2.5f;
    public float ledgeMantleDistance = 1.0f;
    public float safeDropDepth = 3.0f; // Adicionado de volta

    [Header("▶ Depuração Visual")]
    public bool showDebugGizmos = true;
    #endregion

    #region UNITY LIFECYLCE
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
        public bool anticipationWallAhead;
        public bool dangerWallAhead;
        public bool contactWallAhead;
        public bool ceilingAhead;
        public bool anticipationLedgeAhead;
        public bool dangerLedgeAhead;
        public bool isAtLedge;
        public bool canSafelyDrop; // O CAMPO QUE ESTAVA EM FALTA
        public bool jumpablePlatformAhead;
        public bool climbableWallAhead;
        public bool canMantleLedge;
        public bool isGrounded;
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
            anticipationWallAhead = ProbeForWall(anticipationWallProbeOrigin),
            dangerWallAhead = ProbeForWall(dangerWallProbeOrigin),
            contactWallAhead = ProbeForWall(contactWallProbeOrigin),
            ceilingAhead = ProbeForCeiling(),
            anticipationLedgeAhead = ProbeForLedge(anticipationLedgeProbeOrigin),
            dangerLedgeAhead = ProbeForLedge(dangerLedgeProbeOrigin),
            isAtLedge = atLedge,
            canSafelyDrop = atLedge && ProbeForSafeDrop(), // A LÓGICA QUE ESTAVA EM FALTA
            jumpablePlatformAhead = ProbeForJumpablePlatform(),
            climbableWallAhead = ProbeForClimbableWall(),
            canMantleLedge = ProbeForLedgeMantle(),
            isGrounded = _motor.IsGrounded()
        };
    }
    #endregion

    #region SONDAS INTERNAS
    private bool ProbeForWall(Transform origin) => origin != null && Physics2D.Raycast(origin.position, transform.right, wallProbeDistance, groundLayer);
    private bool ProbeForLedge(Transform origin) => origin != null && !Physics2D.Raycast(origin.position, Vector2.down, 1.0f, groundLayer);
    private bool ProbeForSafeDrop() { if (ledgeProbeOrigin == null) return false; return Physics2D.Raycast(ledgeProbeOrigin.position, Vector2.down, safeDropDepth, groundLayer); }
    private bool ProbeForCeiling() => ceilingProbeOrigin != null && Physics2D.Raycast(ceilingProbeOrigin.position, transform.right, ceilingProbeDistance, groundLayer);
    private bool ProbeForJumpablePlatform() { if (jumpProbeOrigin == null) return false; RaycastHit2D hit = Physics2D.Raycast(jumpProbeOrigin.position, transform.right, jumpProbeDistance, groundLayer); return hit.collider && (hit.point.y - transform.position.y) < maxJumpHeight; }
    private bool ProbeForClimbableWall() => climbProbeOrigin != null && Physics2D.Raycast(climbProbeOrigin.position, transform.right, wallProbeDistance, climbableLayer);
    private bool ProbeForLedgeMantle() => ledgeMantleProbeOrigin != null && _motor.IsClimbing && !Physics2D.Raycast(ledgeMantleProbeOrigin.position, transform.right, ledgeMantleDistance, groundLayer);
    #endregion

    #region VISUALIZAÇÃO
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;

        if (anticipationWallProbeOrigin) { Gizmos.color = ProbeForWall(anticipationWallProbeOrigin) ? Color.green : new Color(0, 0.5f, 0); Gizmos.DrawLine(anticipationWallProbeOrigin.position, anticipationWallProbeOrigin.position + transform.right * wallProbeDistance); }
        if (dangerWallProbeOrigin) { Gizmos.color = ProbeForWall(dangerWallProbeOrigin) ? Color.yellow : new Color(0.5f, 0.5f, 0); Gizmos.DrawLine(dangerWallProbeOrigin.position, dangerWallProbeOrigin.position + transform.right * wallProbeDistance); }
        if (contactWallProbeOrigin) { Gizmos.color = ProbeForWall(contactWallProbeOrigin) ? Color.red : new Color(0.5f, 0, 0); Gizmos.DrawLine(contactWallProbeOrigin.position, contactWallProbeOrigin.position + transform.right * wallProbeDistance); }

        if (anticipationLedgeProbeOrigin) { Gizmos.color = ProbeForLedge(anticipationLedgeProbeOrigin) ? Color.green : new Color(0, 0.5f, 0); Gizmos.DrawLine(anticipationLedgeProbeOrigin.position, anticipationLedgeProbeOrigin.position + Vector3.down * 1.0f); }
        if (dangerLedgeProbeOrigin) { Gizmos.color = ProbeForLedge(dangerLedgeProbeOrigin) ? Color.yellow : new Color(0.5f, 0.5f, 0); Gizmos.DrawLine(dangerLedgeProbeOrigin.position, dangerLedgeProbeOrigin.position + Vector3.down * 1.0f); }

        // A sonda de queda real (vermelha/amarela)
        if (ledgeProbeOrigin) { Gizmos.color = ProbeForLedge(ledgeProbeOrigin) ? (ProbeForSafeDrop() ? Color.yellow : Color.red) : new Color(0.5f, 0, 0); Gizmos.DrawLine(ledgeProbeOrigin.position, ledgeProbeOrigin.position + Vector3.down * safeDropDepth); }

        if (ceilingProbeOrigin) { Gizmos.color = ProbeForCeiling() ? Color.yellow : new Color(0.5f, 0.5f, 0); Gizmos.DrawLine(ceilingProbeOrigin.position, ceilingProbeOrigin.position + transform.right * ceilingProbeDistance); }
        if (jumpProbeOrigin) { Gizmos.color = ProbeForJumpablePlatform() ? Color.magenta : new Color(0.5f, 0, 0.5f); Gizmos.DrawLine(jumpProbeOrigin.position, jumpProbeOrigin.position + transform.right * jumpProbeDistance); }
        if (climbProbeOrigin) { Gizmos.color = ProbeForClimbableWall() ? Color.green : new Color(0, 0.5f, 0); Gizmos.DrawLine(climbProbeOrigin.position, climbProbeOrigin.position + transform.right * wallProbeDistance); }
        if (ledgeMantleProbeOrigin) { Gizmos.color = Color.white; Gizmos.DrawLine(ledgeMantleProbeOrigin.position, ledgeMantleProbeOrigin.position + transform.right * ledgeMantleDistance); }
    }
    #endregion
}