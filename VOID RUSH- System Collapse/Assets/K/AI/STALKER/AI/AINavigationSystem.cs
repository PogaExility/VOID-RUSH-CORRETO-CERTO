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
    [Tooltip("Sensor que aponta para CIMA para verificar se é seguro levantar.")]
    public Transform standUpProbeOrigin;
    [Tooltip("Sensor posicionado na altura de agachar para detectar túneis escaláveis.")]
    public Transform crouchEnterProbeOrigin;

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
    public float gizmoCubeSize = 0.1f;
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
        // --- ALTERAÇÃO: Novo estado para o cérebro ---
        public bool canEnterCrouchTunnel;
    }

    // --- ALTERAÇÃO: O MÉTODO QueryEnvironment FOI COMPLETAMENTE REESCRITO PARA IMPLEMENTAR A LÓGICA DE PRIORIDADES ---
    public NavQueryResult QueryEnvironment()
    {
        if (_motor == null) return new NavQueryResult();

        // ETAPA 1: LER TODOS OS SENSORES BRUTOS
        bool rawContactWall = ProbeForWall(contactWallProbeOrigin);
        bool rawDangerWall = ProbeForWall(dangerWallProbeOrigin);
        bool rawAnticipationWall = ProbeForWall(anticipationWallProbeOrigin);

        bool rawIsAtLedge = ProbeForLedge(ledgeProbeOrigin);
        bool rawDangerLedge = ProbeForLedge(dangerLedgeProbeOrigin);
        bool rawAnticipationLedge = ProbeForLedge(anticipationLedgeProbeOrigin);

        // ETAPA 2: APLICAR A LÓGICA DE ESTÁGIOS (INTRA-SISTEMA)
        // A ameaça de maior nível anula as de nível mais baixo.
        bool finalContactWall = rawContactWall;
        bool finalDangerWall = rawDangerWall && !finalContactWall;
        bool finalAnticipationWall = rawAnticipationWall && !rawDangerWall && !rawContactWall;

        bool finalIsAtLedge = rawIsAtLedge;
        bool finalDangerLedge = rawDangerLedge && !finalIsAtLedge;
        bool finalAnticipationLedge = rawAnticipationLedge && !rawDangerLedge && !rawIsAtLedge;

        // ETAPA 3: APLICAR A LÓGICA DE PRIORIDADE (INTER-SISTEMA)
        // Se QUALQUER sensor de parede estiver ativo, ignore TODOS os sensores de borda.
        bool isAnyWallSensorActive = finalContactWall || finalDangerWall || finalAnticipationWall;

        var result = new NavQueryResult();

        // Atribui os resultados de parede
        result.contactWallAhead = finalContactWall;
        result.dangerWallAhead = finalDangerWall;
        result.anticipationWallAhead = finalAnticipationWall;

        // Atribui os resultados de borda, aplicando a prioridade de parede
        result.isAtLedge = finalIsAtLedge && !isAnyWallSensorActive;
        result.dangerLedgeAhead = finalDangerLedge && !isAnyWallSensorActive;
        result.anticipationLedgeAhead = finalAnticipationLedge && !isAnyWallSensorActive;

        // Atribui os resultados restantes que não possuem prioridades complexas
        result.ceilingAhead = ProbeForCeiling(ceilingProbeOrigin);
        result.jumpablePlatformAhead = ProbeForJumpablePlatform();
        result.climbableWallAhead = ProbeForClimbableWall();
        result.canMantleLedge = ProbeForLedgeMantle();
        result.isGrounded = _motor.IsGrounded();
        result.isClearToStandUp = ProbeForStandUpClearance();
        result.canEnterCrouchTunnel = ProbeForCrouchTunnel(crouchEnterProbeOrigin);

        return result;
    }
    #endregion

    #region SONDAS INTERNAS
    private bool ProbeForWall(Transform origin) => origin != null && Physics2D.Raycast(origin.position, transform.right, wallProbeDistance, groundLayer);
    private bool ProbeForLedge(Transform origin) => origin != null && !Physics2D.Raycast(origin.position, Vector2.down, 1.0f, groundLayer);
    private bool ProbeForCeiling(Transform origin) => origin != null && Physics2D.Raycast(origin.position, transform.right, ceilingProbeDistance, groundLayer);
    private bool ProbeForStandUpClearance() => standUpProbeOrigin != null && !Physics2D.Raycast(standUpProbeOrigin.position, Vector2.up, _motor.StandingHeight, groundLayer);
    private bool ProbeForJumpablePlatform() { return false; } // Implementação futura
    private bool ProbeForClimbableWall() { return false; } // Implementação futura
    private bool ProbeForLedgeMantle() { return false; } // Implementação futura
    private bool ProbeForCrouchTunnel(Transform origin) => origin != null && !Physics2D.Raycast(origin.position, transform.right, wallProbeDistance, groundLayer);
    #endregion

    #region VISUALIZAÇÃO DIDÁTICA
    // --- ALTERAÇÃO: A VISUALIZAÇÃO AGORA USA O RESULTADO PROCESSADO DO QueryEnvironment ---
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying || _motor == null) return;
        NavQueryResult perception = QueryEnvironment();

        DrawProbeGizmo(anticipationWallProbeOrigin, transform.right, wallProbeDistance, perception.anticipationWallAhead, Color.green);
        DrawProbeGizmo(dangerWallProbeOrigin, transform.right, wallProbeDistance, perception.dangerWallAhead, Color.yellow);
        DrawProbeGizmo(contactWallProbeOrigin, transform.right, wallProbeDistance, perception.contactWallAhead, Color.red);
        DrawProbeGizmo(climbProbeOrigin, transform.right, wallProbeDistance, perception.climbableWallAhead, Color.green);

        DrawProbeGizmo(anticipationLedgeProbeOrigin, Vector3.down, 1.0f, perception.anticipationLedgeAhead, Color.green);
        DrawProbeGizmo(dangerLedgeProbeOrigin, Vector3.down, 1.0f, perception.dangerLedgeAhead, Color.yellow);
        DrawProbeGizmo(ledgeProbeOrigin, Vector3.down, 1.0f, perception.isAtLedge, Color.red);

        DrawProbeGizmo(ceilingProbeOrigin, transform.right, ceilingProbeDistance, perception.ceilingAhead, Color.yellow);
        DrawProbeGizmo(standUpProbeOrigin, Vector3.up, _motor.StandingHeight, perception.isClearToStandUp, Color.cyan);
        DrawProbeGizmo(crouchEnterProbeOrigin, transform.right, wallProbeDistance, perception.canEnterCrouchTunnel, Color.magenta);
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
