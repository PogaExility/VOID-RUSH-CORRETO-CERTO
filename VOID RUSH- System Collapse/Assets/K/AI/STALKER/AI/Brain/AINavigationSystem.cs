using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    public enum WallType { None, FullWall, LedgeLow }

    [Header("▶ Sondas de Parede")]
    public Transform Probe_Wall_Top;
    public Transform Probe_Wall_Mid;
    public Transform Probe_Wall_Base;

    [Header("▶ Sonda de Duto")]
    public Transform Probe_Vent_Inside_Check;

    [Header("▶ Sondas de Segurança")]
    public Transform Probe_Crouch_Safety_Front;
    public Transform Probe_Crouch_Safety_Mid;
    public Transform Probe_Crouch_Safety_Back;

    [Header("▶ Configuração")]
    public LayerMask groundLayer;
    public float wallProbeDistance = 0.1f;
    public float ceilingProbeHeight = 1.0f;

    [Header("▶ Responsividade")]
    [Tooltip("Tempo reduzido para ser quase instantâneo, mas filtrando ruído.")]
    public float ventRecognitionDelay = 0.15f; // REDUZIDO DE 1.0 PARA 0.15

    public bool showDebugGizmos = true;

    public WallType CurrentWallType { get; private set; }
    private float _timeSinceTopClear = 0f;

    private void Update()
    {
        if (Probe_Wall_Top != null)
        {
            bool topBlocked = Physics2D.Raycast(Probe_Wall_Top.position, transform.right, wallProbeDistance, groundLayer);
            if (!topBlocked) _timeSinceTopClear += Time.deltaTime;
            else _timeSinceTopClear = 0f;
        }
        CurrentWallType = ScanWallType();
    }

    private WallType ScanWallType()
    {
        if (Probe_Wall_Top == null || Probe_Wall_Mid == null || Probe_Wall_Base == null) return WallType.None;

        bool top = Physics2D.Raycast(Probe_Wall_Top.position, transform.right, wallProbeDistance, groundLayer);
        bool mid = Physics2D.Raycast(Probe_Wall_Mid.position, transform.right, wallProbeDistance, groundLayer);
        bool base_ = Physics2D.Raycast(Probe_Wall_Base.position, transform.right, wallProbeDistance, groundLayer);

        if (base_ && (mid || top)) return WallType.FullWall;
        if (base_ && !mid && !top) return WallType.LedgeLow;

        return WallType.None;
    }

    public bool HasClimbableWall() => CurrentWallType != WallType.None;

    public bool IsVentOpening()
    {
        if (_timeSinceTopClear < ventRecognitionDelay) return false;
        if (Probe_Wall_Base == null || Probe_Vent_Inside_Check == null) return false;

        bool baseHit = Physics2D.Raycast(Probe_Wall_Base.position, transform.right, wallProbeDistance, groundLayer);
        bool midHit = Physics2D.Raycast(Probe_Wall_Mid.position, transform.right, wallProbeDistance, groundLayer);
        bool insideCeilingHit = Physics2D.Raycast(Probe_Vent_Inside_Check.position, Vector2.up, ceilingProbeHeight, groundLayer);

        return baseHit && !midHit && insideCeilingHit;
    }

    public bool ShouldStartCrouching()
    {
        bool topBlocked = Physics2D.Raycast(Probe_Wall_Top.position, transform.right, wallProbeDistance, groundLayer);
        bool baseIsOpen = !Physics2D.Raycast(Probe_Wall_Base.position, transform.right, wallProbeDistance, groundLayer);
        return topBlocked && baseIsOpen;
    }

    public bool CanStandUpSafely()
    {
        bool front = !Physics2D.Raycast(Probe_Crouch_Safety_Front.position, Vector2.up, ceilingProbeHeight, groundLayer);
        bool mid = !Physics2D.Raycast(Probe_Crouch_Safety_Mid.position, Vector2.up, ceilingProbeHeight, groundLayer);
        bool back = !Physics2D.Raycast(Probe_Crouch_Safety_Back.position, Vector2.up, ceilingProbeHeight, groundLayer);
        return front && mid && back;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        DrawRay(Probe_Wall_Top, transform.right, wallProbeDistance);
        DrawRay(Probe_Wall_Mid, transform.right, wallProbeDistance);
        DrawRay(Probe_Wall_Base, transform.right, wallProbeDistance);

        if (Probe_Vent_Inside_Check != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Probe_Vent_Inside_Check.position, Probe_Vent_Inside_Check.position + Vector3.up * ceilingProbeHeight);
        }
    }

    void DrawRay(Transform t, Vector3 dir, float dist)
    {
        if (t == null) return;
        bool hit = Physics2D.Raycast(t.position, dir, dist, groundLayer);
        Gizmos.color = hit ? Color.red : Color.green;
        Gizmos.DrawLine(t.position, t.position + dir * dist);
    }
}