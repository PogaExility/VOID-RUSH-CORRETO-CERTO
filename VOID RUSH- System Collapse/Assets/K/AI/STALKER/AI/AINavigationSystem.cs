using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    private AIPlatformerMotor _motor;

    [Header("▶ Pontos de Origem das Sondas")]
    public Transform wallProbeOrigin;
    public Transform ledgeProbeOrigin;
    public Transform climbProbeOrigin;
    public Transform ledgeMantleProbeOrigin;

    [Header("▶ Configuração das Sondas")]
    public LayerMask groundLayer;
    public LayerMask climbableLayer;
    [Space]
    public float wallProbeDistance = 1.0f;
    public float ledgeMantleDistance = 1.0f;
    public float safeDropDepth = 3.0f;

    [Header("▶ Depuração Visual")]
    public bool showDebugGizmos = true;
    // O #endregion que estava aqui foi removido.

    void Start()
    {
        _motor = GetComponent<AIPlatformerMotor>();
    }

    public struct NavQueryResult
    {
        public bool wallAhead;
        public bool isAtLedge;
        public bool canSafelyDrop;
        public bool climbableWallAhead;
        public bool canMantleLedge;
    }

    public NavQueryResult QueryEnvironment()
    {
        bool atLedge = IsAtLedge();
        return new NavQueryResult
        {
            wallAhead = ProbeForWall(),
            isAtLedge = atLedge,
            canSafelyDrop = atLedge && ProbeForSafeDrop(),
            climbableWallAhead = ProbeForClimbableWall(),
            canMantleLedge = ProbeForLedgeMantle()
        };
    }

    #region Implementação das Sondas
    private bool ProbeForWall() => Physics2D.Raycast(wallProbeOrigin.position, transform.right, wallProbeDistance, groundLayer);

    private bool IsAtLedge() => _motor.IsGrounded() && !Physics2D.Raycast((Vector2)ledgeProbeOrigin.position, Vector2.down, 0.5f, groundLayer);

    private bool ProbeForSafeDrop() => Physics2D.Raycast((Vector2)ledgeProbeOrigin.position, Vector2.down, safeDropDepth, groundLayer);

    private bool ProbeForClimbableWall() => Physics2D.Raycast(climbProbeOrigin.position, transform.right, wallProbeDistance, climbableLayer);

    private bool ProbeForLedgeMantle() => _motor.IsClimbing && !Physics2D.Raycast(ledgeMantleProbeOrigin.position, transform.right, ledgeMantleDistance, groundLayer);
    #endregion

    #region Visualização Neural Direta
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Adicionadas verificações de nulidade para evitar erros no editor antes de 'Start' ser chamado
        if (Application.isPlaying)
        {
            // Parede (Vermelho)
            Gizmos.color = ProbeForWall() ? Color.red : new Color(0.5f, 0, 0);
            if (wallProbeOrigin) Gizmos.DrawLine(wallProbeOrigin.position, wallProbeOrigin.position + transform.right * wallProbeDistance);

            // Queda (Amarelo/Vermelho)
            Gizmos.color = IsAtLedge() ? (ProbeForSafeDrop() ? Color.yellow : Color.red) : new Color(0.5f, 0.5f, 0);
            if (ledgeProbeOrigin) Gizmos.DrawLine(ledgeProbeOrigin.position, ledgeProbeOrigin.position + Vector3.down * safeDropDepth);

            // Escalada (Verde)
            Gizmos.color = ProbeForClimbableWall() ? Color.green : new Color(0, 0.5f, 0);
            if (climbProbeOrigin) Gizmos.DrawLine(climbProbeOrigin.position, climbProbeOrigin.position + transform.right * wallProbeDistance);

            // Fim da Escalada (Branco)
            Gizmos.color = Color.white;
            if (ledgeMantleProbeOrigin) Gizmos.DrawLine(ledgeMantleProbeOrigin.position, ledgeMantleProbeOrigin.position + transform.right * ledgeMantleDistance);
        }
    }
    #endregion
}