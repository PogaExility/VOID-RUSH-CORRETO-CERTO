using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    #region CONFIGURATION
    [Header("▶ Configuração de Sondas")]
    public float wallProbeDistance = 0.8f;
    public float groundProbeDistance = 1.2f;
    public float dangerDropHeight = 1.0f;
    public LayerMask groundLayer;
    #endregion

    #region REFERENCES
    private AIPlatformerMotor _motor;
    #endregion

    void Start()
    {
        _motor = GetComponent<AIPlatformerMotor>();
    }

    #region Sondas
    public bool IsPathBlocked()
    {
        return Physics2D.Raycast(transform.position, transform.right, wallProbeDistance, groundLayer);
    }

    public bool IsFacingEdge()
    {
        Vector2 probeOrigin = (Vector2)transform.position + (Vector2)transform.right * 0.5f;
        return !Physics2D.Raycast(probeOrigin, Vector2.down, 1.5f, groundLayer);
    }

    public bool DetectTerrainDanger(out Vector3 dangerPoint)
    {
        dangerPoint = Vector3.zero;
        if (!_motor.IsGrounded()) return false;
        Vector2 probeOrigin = (Vector2)transform.position + (Vector2)transform.right * groundProbeDistance;
        RaycastHit2D hit = Physics2D.Raycast(probeOrigin, Vector2.down, dangerDropHeight, groundLayer);
        if (!hit.collider)
        {
            dangerPoint = probeOrigin;
            return true;
        }
        return false;
    }
    #endregion

    #region NAVEGAÇÃO UNIFICADA (RESTAURADO)
    /// <summary>
    /// O único método que controla o movimento. Ele reage a obstáculos.
    /// É chamado tanto pela Patrulha como pela Caça.
    /// </summary>
    public void Navigate(float moveSpeed, float jumpForce)
    {
        if (IsPathBlocked())
        {
            _motor.Jump(jumpForce); // Tenta saltar sobre obstáculos
        }
        else if (IsFacingEdge())
        {
            // Para na beira. O Controller decidirá quando se virar.
            _motor.Stop();
        }
        else
        {
            _motor.Move(moveSpeed);
        }
    }
    #endregion

    #region GIZMOS
    void OnDrawGizmosSelected()
    {
        if (_motor == null)
        {
            if (Application.isPlaying) _motor = GetComponent<AIPlatformerMotor>();
            if (_motor == null) return;
        }
        Vector2 probeOrigin = (Vector2)transform.position + (Vector2)transform.right * groundProbeDistance;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(probeOrigin, probeOrigin + Vector2.down * dangerDropHeight);
    }
    #endregion
}