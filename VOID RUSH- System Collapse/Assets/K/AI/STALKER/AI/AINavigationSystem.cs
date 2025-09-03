using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    #region CONFIGURATION
    [Header("▶ Configuração de Navegação")]
    public float groundProbeDistance = 1f;
    public float wallProbeDistance = 0.8f;
    public float jumpCheckDistance = 3f;
    public LayerMask groundLayer;
    #endregion

    #region REFERENCES
    private AIPlatformerMotor _motor;
    #endregion

    void Start()
    {
        _motor = GetComponent<AIPlatformerMotor>();
    }

    #region PUBLIC API
    public bool IsPathBlocked()
    {
        return Physics2D.Raycast(transform.position, Vector2.right * _motor.currentFacingDirection, wallProbeDistance, groundLayer);
    }

    public bool IsFacingEdge()
    {
        Vector2 probeOrigin = (Vector2)transform.position + new Vector2(groundProbeDistance * _motor.currentFacingDirection, 0);
        return !Physics2D.Raycast(probeOrigin, Vector2.down, 2f, groundLayer);
    }

    // Uma função mais complexa para decidir o que fazer ao se mover para um alvo
    public void NavigateTowards(Vector3 targetPosition, float moveSpeed, float jumpForce)
    {
        _motor.FaceTarget(targetPosition);

        if (IsPathBlocked())
        {
            // Tenta pular sobre o obstáculo
            _motor.Jump(jumpForce);
        }
        else if (IsFacingEdge())
        {
            // Decide se deve pular o vão
            // Lógica mais complexa de análise de pulo iria aqui
            _motor.Stop(); // Por enquanto, para.
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
        if (!Application.isPlaying) return;

        // Wall Probe
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.right * _motor.currentFacingDirection * wallProbeDistance));

        // Ground Ahead Probe
        Gizmos.color = Color.green;
        Vector2 probeOrigin = (Vector2)transform.position + new Vector2(groundProbeDistance * _motor.currentFacingDirection, 0);
        Gizmos.DrawLine(probeOrigin, probeOrigin + (Vector2.down * 2f));
    }
    #endregion
}