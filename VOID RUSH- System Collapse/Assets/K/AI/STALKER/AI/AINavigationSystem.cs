using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    // Enum atualizado para incluir a nova percepção de "queda segura"
    public enum ObstacleType { None, CrouchTunnel, JumpablePlatform, FullWall, Ledge, DroppableLedge }

    // Enum para a sua lógica de escalada configurável
    public enum VaultHeightLogic
    {
        BaseOnly,      // Apenas obstáculos de 1 tile de altura são escaláveis.
        UpToMidHeight  // Obstáculos de 1 E 2 tiles de altura são escaláveis.
    }

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

    [Header("▶ LÓGICA TÁTICA CONFIGURÁVEL")]
    [Tooltip("Define qual a altura máxima que a IA considera escalável.")]
    public VaultHeightLogic vaultLogic = VaultHeightLogic.BaseOnly;
    [Tooltip("A altura máxima (em tiles, ex: 3 = 3 tiles) que a IA considera segura para descer.")]
    public float maxDropDownHeight = 3.0f;

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
            // Lógica de agachar (está funcionando perfeitamente)
            bool seesMidWall = ProbeForWall(Probe_Height_2_Mid);
            if (seesMidWall) { result.detectedObstacle = ObstacleType.FullWall; }
            else if (ProbeForLedge(out float _)) { result.detectedObstacle = ObstacleType.Ledge; }
        }
        else
        {
            // Lógica de estado em pé
            bool seesBaseWall = ProbeForWall(Probe_Height_1_Base);
            bool seesMidWall = ProbeForWall(Probe_Height_2_Mid);
            bool seesTopWall = ProbeForWall(Probe_Height_3_Top);
            bool seesCeiling = ProbeForCeiling(Probe_Ceiling_Check);

            if (seesCeiling && seesTopWall) { result.detectedObstacle = ObstacleType.CrouchTunnel; }
            else if (seesBaseWall)
            {
                // Sua lógica de escalada configurável
                bool isVaultable = false;
                switch (vaultLogic)
                {
                    case VaultHeightLogic.BaseOnly:
                        if (!seesMidWall && !seesTopWall) isVaultable = true;
                        break;
                    case VaultHeightLogic.UpToMidHeight:
                        if (!seesTopWall) isVaultable = true;
                        break;
                }
                result.detectedObstacle = isVaultable ? ObstacleType.JumpablePlatform : ObstacleType.FullWall;
            }
            else if (ProbeForLedge(out float distanceToGroundBelow))
            {
                // Sua lógica de queda controlada
                result.detectedObstacle = distanceToGroundBelow <= maxDropDownHeight ? ObstacleType.DroppableLedge : ObstacleType.Ledge;
            }
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
    private bool ProbeForCeiling(Transform origin) => origin != null && Physics2D.Raycast(origin.position, Vector2.up, ceilingProbeHeight, groundLayer);

    private bool ProbeForLedge(out float distance)
    {
        distance = float.MaxValue;
        if (Probe_Ledge_Check == null) return false;

        if (!Physics2D.Raycast(Probe_Ledge_Check.position, Vector2.down, ledgeProbeDistance, groundLayer))
        {
            RaycastHit2D hit = Physics2D.Raycast(Probe_Ledge_Check.position, Vector2.down, maxDropDownHeight + ledgeProbeDistance, groundLayer);
            if (hit.collider != null)
            {
                distance = hit.distance;
            }
            return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;
        DrawProbeGizmo(Probe_Height_1_Base, ProbeForWall(Probe_Height_1_Base), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Height_2_Mid, ProbeForWall(Probe_Height_2_Mid), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Height_3_Top, ProbeForWall(Probe_Height_3_Top), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Ledge_Check, ProbeForLedge(out float _), Color.magenta, Vector2.down, ledgeProbeDistance);
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