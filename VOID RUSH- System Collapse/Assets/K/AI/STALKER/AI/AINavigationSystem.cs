using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    public enum ObstacleType { None, CrouchTunnel, JumpablePlatform, FullWall, Ledge }

    #region REFERÊNCIAS
    private AIPlatformerMotor _motor;
    #endregion

    #region CONFIGURAÇÃO DAS SONDAS
    [Header("▶ HIERARQUIA DE SONDAS (6 SONDAS)")]
    public Transform Probe_Height_1_Base;
    public Transform Probe_Height_2_Mid;
    public Transform Probe_Height_3_Top;
    public Transform Probe_Ledge_Check;
    public Transform Probe_Ceiling_Check;
    public Transform Probe_Crouch_Safety;

    [Header("▶ Configuração das Sondas")]
    public LayerMask groundLayer;
    public float wallProbeDistance = 0.2f;
    public float ledgeProbeDistance = 1.0f;
    public float ceilingProbeHeight = 0.5f;

    [Header("▶ Depuração Visual")]
    public bool showDebugGizmos = true;
    #endregion

    void Awake()
    {
        // Usa GetComponentInParent porque o motor está no objeto raiz, um nível acima.
        _motor = GetComponentInParent<AIPlatformerMotor>();
        if (_motor == null) { Debug.LogError("AINavigationSystem: AIPlatformerMotor não foi encontrado no objeto pai!", this); }
    }

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

        // =====================================================================================
        // LÓGICA DE PERCEPÇÃO BASEADA EM ESTADO
        // =====================================================================================
        if (_motor.IsCrouching)
        {
            // --- LÓGICA QUANDO ESTÁ AGACHADO ---
            // A IA só se preocupa com paredes baixas e bordas dentro do túnel.
            if (ProbeForWall(Probe_Height_1_Base))
            {
                result.detectedObstacle = ObstacleType.FullWall;
            }
            else if (ProbeForLedge(Probe_Ledge_Check))
            {
                result.detectedObstacle = ObstacleType.Ledge;
            }
        }
        else
        {
            // --- LÓGICA QUANDO ESTÁ EM PÉ (SUA ARQUITETURA COMPLETA) ---
            bool seesBaseWall = ProbeForWall(Probe_Height_1_Base);
            bool seesMidWall = ProbeForWall(Probe_Height_2_Mid);
            bool seesTopWall = ProbeForWall(Probe_Height_3_Top);
            bool seesCeiling = ProbeForCeiling(Probe_Ceiling_Check);

            if (seesCeiling && seesTopWall) { result.detectedObstacle = ObstacleType.CrouchTunnel; }
            else if (seesBaseWall && !seesMidWall && !seesTopWall) { result.detectedObstacle = ObstacleType.JumpablePlatform; }
            else if (seesBaseWall) { result.detectedObstacle = ObstacleType.FullWall; }
            else if (ProbeForLedge(Probe_Ledge_Check)) { result.detectedObstacle = ObstacleType.Ledge; }
        }

        return result;
    }

    // O "Switch" de Segurança
    public bool CanStandUp()
    {
        return !ProbeForCeiling(Probe_Crouch_Safety);
    }

    #region Sondas e Gizmos
    private bool ProbeForWall(Transform origin) => origin != null && Physics2D.Raycast(origin.position, transform.right, wallProbeDistance, groundLayer);
    private bool ProbeForLedge(Transform origin) => origin != null && !Physics2D.Raycast(origin.position, Vector2.down, ledgeProbeDistance, groundLayer);
    private bool ProbeForCeiling(Transform origin) => origin != null && Physics2D.Raycast(origin.position, Vector2.up, ceilingProbeHeight, groundLayer);

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;
        DrawProbeGizmo(Probe_Height_1_Base, ProbeForWall(Probe_Height_1_Base), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Height_2_Mid, ProbeForWall(Probe_Height_2_Mid), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Height_3_Top, ProbeForWall(Probe_Height_3_Top), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Ledge_Check, ProbeForLedge(Probe_Ledge_Check), Color.magenta, Vector2.down, ledgeProbeDistance);
        DrawProbeGizmo(Probe_Ceiling_Check, ProbeForCeiling(Probe_Ceiling_Check), Color.yellow, Vector2.up, ceilingProbeHeight);
        DrawProbeGizmo(Probe_Crouch_Safety, !CanStandUp(), Color.cyan, Vector2.up, ceilingProbeHeight);
    }

    private void DrawProbeGizmo(Transform origin, bool isActive, Color activeColor, Vector3 direction, float distance)
    {
        if (origin == null) return;
        Color inactiveColor = new Color(activeColor.r * 0.3f, activeColor.g * 0.3f, activeColor.b * 0.3f, 0.5f);
        Gizmos.color = isActive ? activeColor : inactiveColor;
        Gizmos.DrawLine(origin.position, origin.position + direction * distance);
    }
    #endregion
}