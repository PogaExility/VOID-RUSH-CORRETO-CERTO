using UnityEngine;

// ====================================================================================
// DEFINIÇÕES DE DADOS (AGORA DENTRO DESTE ARQUIVO PARA EVITAR ERROS)
// ====================================================================================
public enum LedgeType
{
    None,
    SolidWall,
    HighOpening,
    LowOpening
}

public struct WallAnalysisData
{
    public bool IsWallDetected;
    public LedgeType TypeOfLedge;
    public Vector2 LedgePosition;
}
// ====================================================================================


// NOME DA CLASSE CORRIGIDO PARA CORRESPONDER AO NOME DO ARQUIVO: AIStalkerWallSensor
[RequireComponent(typeof(AIPlatformerMotor))]
public class AIStalkerWallSensor : MonoBehaviour
{
    private AIPlatformerMotor _motor;
    private CapsuleCollider2D _collider;

    [Header("▶ Configuração do Sensor")]
    public float wallDetectionDistance = 0.5f;
    public float spaceAnalysisDistance = 1.0f;
    public LayerMask groundLayer;

    [Header("▶ Depuração Visual")]
    public bool showDebugGizmos = true;
    private Vector2 _lastLedgePosition;
    private LedgeType _lastLedgeType;

    void Awake()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        _collider = GetComponent<CapsuleCollider2D>();
    }

    public WallAnalysisData AnalyzeWallInFront()
    {
        var analysis = new WallAnalysisData();
        RaycastHit2D wallHit = Physics2D.BoxCast((Vector2)transform.position + new Vector2(0, _motor.StandingHeight / 2), new Vector2(_collider.size.x, _motor.StandingHeight * 0.9f), 0f, transform.right, wallDetectionDistance, groundLayer);

        if (!wallHit.collider)
        {
            analysis.IsWallDetected = false;
            analysis.TypeOfLedge = LedgeType.None;
            _lastLedgeType = LedgeType.None;
            return analysis;
        }

        analysis.IsWallDetected = true;
        RaycastHit2D ledgeHit = Physics2D.Raycast(wallHit.point + new Vector2(_collider.size.x * _motor.currentFacingDirection, _motor.StandingHeight), Vector2.down, _motor.StandingHeight * 2, groundLayer);

        if (!ledgeHit.collider)
        {
            analysis.TypeOfLedge = LedgeType.SolidWall;
            _lastLedgeType = LedgeType.SolidWall;
            return analysis;
        }

        analysis.LedgePosition = ledgeHit.point;
        _lastLedgePosition = ledgeHit.point;

        float crouchHeight = 1.9f;
        Vector2 standingCheckOrigin = ledgeHit.point + new Vector2(0.1f * _motor.currentFacingDirection, _motor.StandingHeight / 2 + 0.1f);
        bool canStand = !Physics2D.BoxCast(standingCheckOrigin, new Vector2(_collider.size.x, _motor.StandingHeight * 0.9f), 0f, transform.right, spaceAnalysisDistance, groundLayer);

        if (canStand)
        {
            analysis.TypeOfLedge = LedgeType.HighOpening;
            _lastLedgeType = LedgeType.HighOpening;
            return analysis;
        }

        Vector2 crouchingCheckOrigin = ledgeHit.point + new Vector2(0.1f * _motor.currentFacingDirection, crouchHeight / 2 + 0.1f);
        bool canCrouch = !Physics2D.BoxCast(crouchingCheckOrigin, new Vector2(_collider.size.x, crouchHeight * 0.9f), 0f, transform.right, spaceAnalysisDistance, groundLayer);

        if (canCrouch)
        {
            analysis.TypeOfLedge = LedgeType.LowOpening;
            _lastLedgeType = LedgeType.LowOpening;
            return analysis;
        }

        analysis.TypeOfLedge = LedgeType.SolidWall;
        _lastLedgeType = LedgeType.SolidWall;
        return analysis;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;
        if (_lastLedgeType == LedgeType.HighOpening) Gizmos.color = Color.green;
        else if (_lastLedgeType == LedgeType.LowOpening) Gizmos.color = Color.yellow;
        else if (_lastLedgeType == LedgeType.SolidWall) Gizmos.color = Color.red;
        else Gizmos.color = Color.clear;

        if (_lastLedgeType != LedgeType.None && _lastLedgeType != LedgeType.SolidWall)
        {
            Gizmos.DrawSphere(_lastLedgePosition, 0.2f);
        }
    }
}