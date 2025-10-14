using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    public enum ObstacleType { None, CrouchTunnel, JumpablePlatform, FullWall, Ledge }

    #region REFERÊNCIAS
    private AIPlatformerMotor _motor;
    #endregion

    #region CONFIGURAÇÃO DAS SONDAS
    [Header("▶ Grid de Detecção Frontal")]
    public Transform Probe_Height_1_Base;
    public Transform Probe_Height_2_Mid;
    public Transform Probe_Height_3_Top;
    [Header("▶ Sondas Especialistas")]
    public Transform Probe_Ledge_Check;
    public Transform Probe_Ceiling_Check;
    [Header("▶ Cortina de Segurança (3 Sondas)")]
    public Transform Probe_Crouch_Safety_Front;
    public Transform Probe_Crouch_Safety_Mid;
    public Transform Probe_Crouch_Safety_Back;

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
        _motor = GetComponentInParent<AIPlatformerMotor>();
    }

    public struct NavQueryResult { public ObstacleType detectedObstacle; public bool isGrounded; }

    public NavQueryResult QueryEnvironment()
    {
        var result = new NavQueryResult
        {
            isGrounded = _motor.IsGrounded(),
            detectedObstacle = ObstacleType.None
        };

        if (_motor.IsCrouching)
        {
            // =====================================================================================
            // SUA LÓGICA "FODA-SE" IMPLEMENTADA AQUI
            // =====================================================================================
            // Quando agachada, a IA só se importa com paredes que bloqueiam seu torso ou com quedas.

            bool seesMidWall = ProbeForWall(Probe_Height_2_Mid);

            if (seesMidWall)
            {
                // Se a sonda do meio está bloqueada, É uma parede de verdade. Pare.
                result.detectedObstacle = ObstacleType.FullWall;
            }
            else if (ProbeForLedge(Probe_Ledge_Check))
            {
                // Se não há uma parede, verifique se há uma queda.
                result.detectedObstacle = ObstacleType.Ledge;
            }

            // Se NENHUMA das condições acima for atendida (ou seja, se APENAS a sonda
            // da base estiver ativa), o veredito permanecerá 'None'. A IA irá ignorar
            // a rampa e continuar andando, deixando a física fazer o trabalho de subir.
        }
        else
        {
            // --- LÓGICA QUANDO ESTÁ EM PÉ (Inalterada e Correta) ---
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

    public bool CanStandUp()
    {
        bool frontIsClear = !ProbeForCeiling(Probe_Crouch_Safety_Front);
        bool midIsClear = !ProbeForCeiling(Probe_Crouch_Safety_Mid);
        bool backIsClear = !ProbeForCeiling(Probe_Crouch_Safety_Back);

        return frontIsClear && midIsClear && backIsClear;
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
        DrawProbeGizmo(Probe_Crouch_Safety_Front, !ProbeForCeiling(Probe_Crouch_Safety_Front), Color.cyan, Vector2.up, ceilingProbeHeight);
        DrawProbeGizmo(Probe_Crouch_Safety_Mid, !ProbeForCeiling(Probe_Crouch_Safety_Mid), Color.cyan, Vector2.up, ceilingProbeHeight);
        DrawProbeGizmo(Probe_Crouch_Safety_Back, !ProbeForCeiling(Probe_Crouch_Safety_Back), Color.cyan, Vector2.up, ceilingProbeHeight);
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