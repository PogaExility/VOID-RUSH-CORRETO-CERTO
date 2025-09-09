using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    #region CONFIGURATION
    [Header("▶ Configuração das Sondas")]
    public LayerMask groundLayer;
    [Space]
    public float wallProbeDistance = 0.8f;
    public float ceilingProbeHeight = 1.0f;
    [Space]
    public Vector2 holeProbeOffset = new Vector2(1.2f, 1.0f);
    public float holeProbeLength = 1.5f;
    [Space]
    public Vector2 jumpProbeOffset = new Vector2(0.5f, 1.0f);
    public float maxJumpHeight = 2.5f;
    [Space]
    public float safeDropDepth = 3.0f;

    [Header("▶ Depuração Visual")]
    [Tooltip("Ative para ver os Raycasts de navegação no Editor.")]
    public bool showDebugGizmos = true; // Ativado por padrão para depuração
    #endregion

    private AIPlatformerMotor _motor;

    void Start()
    {
        _motor = GetComponent<AIPlatformerMotor>();
    }

    // A ESTRUTURA DE DADOS QUE O CÉREBRO USA
    public struct NavQueryResult
    {
        public bool wallAhead;
        public bool ceilingAhead;
        public bool holeAhead;
        public bool canSafelyDrop;
        public bool jumpablePlatformAhead;
    }

    // O MÉTODO CENTRAL QUE FOI RESTAURADO
    public NavQueryResult QueryEnvironment()
    {
        return new NavQueryResult
        {
            wallAhead = ProbeForWall(),
            ceilingAhead = ProbeForCeiling(),
            holeAhead = ProbeForHole(),
            canSafelyDrop = ProbeForLedgeDrop(),
            jumpablePlatformAhead = ProbeForJumpablePlatform()
        };
    }

    #region Implementação das Sondas Individuais
    private bool ProbeForWall() => Physics2D.Raycast(transform.position, transform.right, wallProbeDistance, groundLayer);

    private bool ProbeForCeiling() => Physics2D.Raycast((Vector2)transform.position + new Vector2(0, ceilingProbeHeight), transform.right, wallProbeDistance, groundLayer);

    private bool ProbeForHole()
    {
        Vector2 topProbeOrigin = (Vector2)transform.position + new Vector2(holeProbeOffset.x * _motor.currentFacingDirection, holeProbeOffset.y);
        Vector2 bottomProbeOrigin = (Vector2)transform.position + new Vector2(holeProbeOffset.x * _motor.currentFacingDirection, 0);
        return !Physics2D.Raycast(bottomProbeOrigin, Vector2.down, holeProbeLength, groundLayer) && !Physics2D.Raycast(topProbeOrigin, Vector2.down, holeProbeLength, groundLayer);
    }

    private bool ProbeForJumpablePlatform()
    {
        Vector2 probeOrigin = (Vector2)transform.position + new Vector2(jumpProbeOffset.x * _motor.currentFacingDirection, jumpProbeOffset.y);
        RaycastHit2D hit = Physics2D.Raycast(probeOrigin, transform.right, 2f, groundLayer);
        return hit.collider && hit.point.y - transform.position.y < maxJumpHeight;
    }

    private bool ProbeForLedgeDrop()
    {
        Vector2 probeOrigin = (Vector2)transform.position + (Vector2)transform.right * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(probeOrigin, Vector2.down, safeDropDepth, groundLayer);
        return hit.collider != null;
    }
    #endregion

    #region Visualização Neural Direta (Gizmos como Raycasts)
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        if (_motor == null) _motor = GetComponent<AIPlatformerMotor>();
        if (_motor == null) return;

        Vector2 pos = transform.position;
        Vector2 right = transform.right;

        // Visualização 1:1 do ProbeForWall
        Gizmos.color = ProbeForWall() ? Color.red : Color.gray;
        Gizmos.DrawLine(pos, pos + right * wallProbeDistance);

        // Visualização 1:1 do ProbeForCeiling
        Gizmos.color = ProbeForCeiling() ? Color.Lerp(Color.red, Color.yellow, 0.5f) : Color.gray;
        Gizmos.DrawLine(pos + new Vector2(0, ceilingProbeHeight), pos + new Vector2(0, ceilingProbeHeight) + right * wallProbeDistance);

        // Visualização 1:1 do ProbeForHole
        Vector2 topProbe = pos + new Vector2(holeProbeOffset.x * _motor.currentFacingDirection, holeProbeOffset.y);
        Vector2 bottomProbe = pos + new Vector2(holeProbeOffset.x * _motor.currentFacingDirection, 0);
        Gizmos.color = ProbeForHole() ? Color.cyan : Color.gray;
        Gizmos.DrawLine(topProbe, topProbe + Vector2.down * holeProbeLength);
        Gizmos.DrawLine(bottomProbe, bottomProbe + Vector2.down * holeProbeLength);

        // Visualização 1:1 do ProbeForJumpablePlatform
        Gizmos.color = ProbeForJumpablePlatform() ? Color.magenta : Color.gray;
        Vector2 jumpProbe = pos + new Vector2(jumpProbeOffset.x * _motor.currentFacingDirection, jumpProbeOffset.y);
        Gizmos.DrawLine(jumpProbe, jumpProbe + right * 2f);

        // Visualização 1:1 do ProbeForLedgeDrop
        Gizmos.color = ProbeForLedgeDrop() ? Color.yellow : Color.gray;
        Vector2 dropProbe = pos + right * 0.5f;
        Gizmos.DrawLine(dropProbe, dropProbe + Vector2.down * safeDropDepth);
    }
    #endregion
}