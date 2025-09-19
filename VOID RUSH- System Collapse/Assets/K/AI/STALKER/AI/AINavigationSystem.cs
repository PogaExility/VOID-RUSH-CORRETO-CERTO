using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    #region REFERENCES & STATE
    private AIPlatformerMotor _motor;
    private CapsuleCollider2D _collider; // Referência para o colisor
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
    [Tooltip("Sensor que aponta para CIMA para verificar se é seguro levantar.")]
    public Transform standUpProbeOrigin;

    [Header("▶ Configuração das Sondas")]
    public LayerMask groundLayer;
    public LayerMask climbableLayer;
    [Space]
    public float wallProbeDistance = 0.2f;
    public float ceilingProbeDistance = 1.0f;
    public float jumpProbeDistance = 2.0f;
    public float maxJumpHeight = 2.5f;
    public float ledgeMantleDistance = 1.0f;
    [Header("▶ Depuração Visual")]
    public bool showDebugGizmos = true;
    public float gizmoCubeSize = 0.1f; // Tamanho dos cubos de diagnóstico
    #endregion

    #region UNITY LIFECYCLE
    void Awake()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        if (_motor == null)
        {
            Debug.LogError("ERRO CRÍTICO: AIPlatformerMotor não encontrado no AINavigationSystem!", this);
            this.enabled = false;
        }
    }
    #endregion

    #region PUBLIC API
    public struct NavQueryResult
    {
        public bool anticipationWallAhead, dangerWallAhead, contactWallAhead, ceilingAhead, anticipationLedgeAhead, dangerLedgeAhead, isAtLedge, jumpablePlatformAhead, climbableWallAhead, canMantleLedge, isGrounded;
        public bool isClearToStandUp;
    }

    public NavQueryResult QueryEnvironment()
    {
        if (_motor == null) return new NavQueryResult();
        return new NavQueryResult
        {
            anticipationWallAhead = ProbeForWall(anticipationWallProbeOrigin),
            dangerWallAhead = ProbeForWall(dangerWallProbeOrigin),
            contactWallAhead = ProbeForWall(contactWallProbeOrigin),
            ceilingAhead = ProbeForCeiling(ceilingProbeOrigin),
            anticipationLedgeAhead = ProbeForLedge(anticipationLedgeProbeOrigin),
            dangerLedgeAhead = ProbeForLedge(dangerLedgeProbeOrigin),
            isAtLedge = ProbeForLedge(ledgeProbeOrigin),
            jumpablePlatformAhead = ProbeForJumpablePlatform(),
            climbableWallAhead = ProbeForClimbableWall(),
            canMantleLedge = ProbeForLedgeMantle(),
            isGrounded = _motor.IsGrounded(),
            isClearToStandUp = ProbeForStandUpClearance()
        };
    }
    #endregion

    #region SONDAS INTERNAS
    private bool ProbeForWall(Transform origin) => origin != null && Physics2D.Raycast(origin.position, transform.right, wallProbeDistance, groundLayer);
    private bool ProbeForLedge(Transform origin) => origin != null && !Physics2D.Raycast(origin.position, Vector2.down, 1.0f, groundLayer);
    private bool ProbeForCeiling(Transform origin) => origin != null && Physics2D.Raycast(origin.position, transform.right, ceilingProbeDistance, groundLayer);
    private bool ProbeForStandUpClearance() => standUpProbeOrigin != null && !Physics2D.Raycast(standUpProbeOrigin.position, Vector2.up, _motor.StandingHeight, groundLayer);
    private bool ProbeForJumpablePlatform() { /* ... */ return false; }
    private bool ProbeForClimbableWall() { /* ... */ return false; }
    private bool ProbeForLedgeMantle() { /* ... */ return false; }
    #endregion

    #region VISUALIZAÇÃO DIDÁTICA
    // =================================================================================================
    // CÓDIGO DE GIZMOS ATUALIZADO PARA MÁXIMA VISIBILIDADE
    // LEGENDA:
    // - LINHA: Raio de detecção
    // - CUBO SÓLIDO: A sonda está ATIVA (retornando TRUE)
    // - VERDE: Antecipação / Escalável
    // - AMARELO: Perigo
    // - VERMELHO: Contato / Parada
    // - CIANO: Espaço livre para levantar
    // =================================================================================================
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying || _motor == null) return;

        // --- SONDAS DE PAREDE ---
        DrawProbeGizmo(anticipationWallProbeOrigin, transform.right, wallProbeDistance, ProbeForWall(anticipationWallProbeOrigin), Color.green);
        DrawProbeGizmo(dangerWallProbeOrigin, transform.right, wallProbeDistance, ProbeForWall(dangerWallProbeOrigin), Color.yellow);
        DrawProbeGizmo(contactWallProbeOrigin, transform.right, wallProbeDistance, ProbeForWall(contactWallProbeOrigin), Color.red);

        // --- SONDAS DE BORDA ---
        DrawProbeGizmo(anticipationLedgeProbeOrigin, Vector3.down, 1.0f, ProbeForLedge(anticipationLedgeProbeOrigin), Color.green);
        DrawProbeGizmo(dangerLedgeProbeOrigin, Vector3.down, 1.0f, ProbeForLedge(dangerLedgeProbeOrigin), Color.yellow);
        DrawProbeGizmo(ledgeProbeOrigin, Vector3.down, 1.0f, ProbeForLedge(ledgeProbeOrigin), Color.red);

        // --- SONDAS DE VERTICALIDADE ---
        DrawProbeGizmo(ceilingProbeOrigin, transform.right, ceilingProbeDistance, ProbeForCeiling(ceilingProbeOrigin), Color.yellow);
        DrawProbeGizmo(standUpProbeOrigin, Vector3.up, _motor.StandingHeight, ProbeForStandUpClearance(), Color.cyan);
    }

    private void DrawProbeGizmo(Transform origin, Vector3 direction, float distance, bool isActive, Color activeColor)
    {
        if (origin == null) return;

        Color inactiveColor = new Color(activeColor.r * 0.3f, activeColor.g * 0.3f, activeColor.b * 0.3f, 0.5f);
        Gizmos.color = isActive ? activeColor : inactiveColor;

        Gizmos.DrawLine(origin.position, origin.position + direction * distance);
        if (isActive)
        {
            Gizmos.DrawCube(origin.position, Vector3.one * gizmoCubeSize);
        }
    }
    #endregion
}