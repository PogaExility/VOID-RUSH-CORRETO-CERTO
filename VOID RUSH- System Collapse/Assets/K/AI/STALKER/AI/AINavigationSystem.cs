using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    public enum ObstacleType { None, Wall, Ledge, DroppableLedge }

    [Header("▶ Sondas de Detecção Essenciais")]
    public Transform Probe_Wall_Base;
    public Transform Probe_Wall_Mid;
    public Transform Probe_Wall_Top;
    public Transform Probe_Ledge_Check;
    public Transform Probe_Ceiling_Check; // Para detectar tetos baixos
    public Transform Probe_Crouch_Safety_Mid;

    [Header("▶ Configuração das Sondas")]
    public LayerMask groundLayer;
    public float wallProbeDistance = 0.5f;
    public float ledgeProbeDistance = 1.0f;
    public float ceilingProbeHeight = 1.0f;
    public float maxDropDownHeight = 3.0f;
    public bool showDebugGizmos = true;

    private AIPlatformerMotor _motor;

    void Awake()
    {
        _motor = GetComponentInParent<AIPlatformerMotor>();
    }

    public struct NavQueryResult { public ObstacleType detectedObstacle; }

    public NavQueryResult QueryEnvironment()
    {
        var result = new NavQueryResult { detectedObstacle = ObstacleType.None };

        if (_motor.IsCrouching)
        {
            if (ProbeForWall(Probe_Wall_Mid)) result.detectedObstacle = ObstacleType.Wall;
            else if (ProbeForLedge(out _)) result.detectedObstacle = ObstacleType.Ledge;
        }
        else
        {
            if (ProbeForWall(Probe_Wall_Base))
            {
                result.detectedObstacle = ObstacleType.Wall;
            }
            else if (ProbeForLedge(out float distance))
            {
                result.detectedObstacle = distance <= maxDropDownHeight ? ObstacleType.DroppableLedge : ObstacleType.Ledge;
            }
        }
        return result;
    }

    // Novas funções de consulta para o Controller
    public bool SeesCeiling() => ProbeForCeiling(Probe_Ceiling_Check);
    public bool SeesTopWall() => ProbeForWall(Probe_Wall_Top);

    public bool CanStandUp() => !ProbeForCeiling(Probe_Crouch_Safety_Mid);

    // Funções de Sonda
    private bool ProbeForWall(Transform origin) => origin != null && Physics2D.Raycast(origin.position, transform.right, wallProbeDistance, groundLayer);
    private bool ProbeForCeiling(Transform origin) => origin != null && Physics2D.Raycast(origin.position, Vector2.up, ceilingProbeHeight, groundLayer);
    private bool ProbeForLedge(out float distance)
    {
        distance = float.MaxValue;
        if (Probe_Ledge_Check == null) return false;
        RaycastHit2D hit = Physics2D.Raycast(Probe_Ledge_Check.position, Vector2.down, maxDropDownHeight + ledgeProbeDistance, groundLayer);
        if (hit.collider == null)
        {
            if (!Physics2D.Raycast(Probe_Ledge_Check.position, Vector2.down, ledgeProbeDistance, groundLayer))
            {
                distance = maxDropDownHeight + ledgeProbeDistance;
                return true;
            }
        }
        else
        {
            distance = hit.distance;
            return true;
        }
        return false;
    }

    // Gizmos para Depuração Visual
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;
        DrawProbeGizmo(Probe_Wall_Base, ProbeForWall(Probe_Wall_Base), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Wall_Mid, ProbeForWall(Probe_Wall_Mid), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Wall_Top, ProbeForWall(Probe_Wall_Top), Color.red, transform.right, wallProbeDistance);
        DrawProbeGizmo(Probe_Ledge_Check, ProbeForLedge(out _), Color.magenta, Vector2.down, ledgeProbeDistance);
        DrawProbeGizmo(Probe_Ceiling_Check, ProbeForCeiling(Probe_Ceiling_Check), Color.yellow, Vector2.up, ceilingProbeHeight);
    }

    private void DrawProbeGizmo(Transform origin, bool isActive, Color activeColor, Vector3 direction, float distance)
    {
        if (origin == null) return;
        Gizmos.color = isActive ? activeColor : new Color(activeColor.r, activeColor.g, activeColor.b, 0.25f);
        Gizmos.DrawLine(origin.position, origin.position + direction * distance);
    }
}