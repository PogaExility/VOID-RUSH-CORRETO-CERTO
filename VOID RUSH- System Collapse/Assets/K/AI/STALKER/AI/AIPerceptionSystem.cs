using UnityEngine;
using UnityEditor;

public class AIPerceptionSystem : MonoBehaviour
{
    // ... (todo o código até ChangeAwareness permanece igual) ...
    #region Estados e Configurações
    public enum GazeMode { Idle, TargetTracking }
    public enum AwarenessState { DORMANT, PATROLLING, SUSPICIOUS, ALERT, HUNTING }

    [Header("▶ Estado Atual")]
    public AwarenessState currentAwareness = AwarenessState.PATROLLING;

    [Header("▶ Configuração de Percepção")]
    public Transform eyes;
    public LayerMask playerLayer;
    public LayerMask visionBlockers;

    [Header("▶ Atributos da Visão")]
    public float visionRange = 15f;
    [Range(0, 360)] public float visionAngle = 90f;
    public float eyeRotationSpeed = 8f;
    public float lkpSafeZoneRadius = 1.5f;

    [Header("▶ Atributos de Alerta")]
    public float inertiaDuration = 0.5f;
    public float searchScanSpeed = 90f;
    public float searchScanAngle = 60f;
    public float memoryDuration = 10f;

    [Header("▶ Depuração Visual")]
    public bool showDebugGizmos = true;

    private GazeMode _currentGazeMode = GazeMode.Idle;
    public Vector3 LastKnownPlayerPosition { get; private set; }
    public bool IsAwareOfPlayer => currentAwareness == AwarenessState.ALERT || currentAwareness == AwarenessState.HUNTING;
    private float _awarenessTimer = 0f;
    private Quaternion _targetEyeRotation = Quaternion.identity;
    private Vector3 _lastKnownPlayerVelocity;
    private float _currentSearchAngle;

    private Transform _player;
    private Rigidbody2D _playerRb;
    #endregion

    #region Ciclo de Vida e Lógica
    void Start()
    {
        _player = AIManager.Instance.playerTarget;
        _playerRb = _player.GetComponent<Rigidbody2D>();
        if (eyes == null) { this.enabled = false; return; }
    }

    void Update()
    {
        HandleAwareness();
        UpdateEyeTarget();
        UpdateEyeRotation();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAwareOfPlayer && other.CompareTag("Player")) { ChangeAwareness(AwarenessState.HUNTING); }
    }

    private void UpdateEyeTarget()
    {
        Vector3 directionToTarget;
        if (currentAwareness == AwarenessState.HUNTING || currentAwareness == AwarenessState.ALERT) { _currentGazeMode = GazeMode.TargetTracking; }
        else { _currentGazeMode = GazeMode.Idle; }

        switch (_currentGazeMode)
        {
            case GazeMode.TargetTracking:
                if (currentAwareness == AwarenessState.HUNTING) { directionToTarget = (_player.position - eyes.position).normalized; }
                else
                {
                    if (Vector3.Distance(eyes.position, LastKnownPlayerPosition) < lkpSafeZoneRadius) { directionToTarget = _lastKnownPlayerVelocity.normalized.magnitude > 0.1f ? _lastKnownPlayerVelocity.normalized : transform.right; }
                    else
                    {
                        float timeSinceLostSight = memoryDuration - _awarenessTimer;
                        if (timeSinceLostSight < inertiaDuration) { Vector3 predictedPosition = LastKnownPlayerPosition + (_lastKnownPlayerVelocity * timeSinceLostSight); directionToTarget = (predictedPosition - eyes.position).normalized; }
                        else { _currentSearchAngle += Time.deltaTime * searchScanSpeed; Quaternion scanRotation = Quaternion.AngleAxis(Mathf.Sin(_currentSearchAngle * Mathf.Deg2Rad) * searchScanAngle, Vector3.forward); directionToTarget = scanRotation * (LastKnownPlayerPosition - eyes.position).normalized; }
                    }
                }
                break;
            default:
                directionToTarget = transform.right;
                break;
        }
        _targetEyeRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
    }

    private void UpdateEyeRotation()
    {
        eyes.rotation = Quaternion.Slerp(eyes.rotation, _targetEyeRotation, Time.deltaTime * eyeRotationSpeed);
    }

    private void HandleAwareness()
    {
        if (CanSeePlayer()) { ChangeAwareness(AwarenessState.HUNTING); }
        else
        {
            if (currentAwareness == AwarenessState.HUNTING) { ChangeAwareness(AwarenessState.ALERT); }
            else if (currentAwareness == AwarenessState.ALERT) { _awarenessTimer -= Time.deltaTime; if (_awarenessTimer <= 0) ChangeAwareness(AwarenessState.PATROLLING); }
        }
    }

    private void ChangeAwareness(AwarenessState newState)
    {
        if (currentAwareness == newState) return;

        Debug.Log($"[AIPerceptionSystem] MUDANÇA DE ESTADO: De {currentAwareness} para {newState}.");

        var oldState = currentAwareness;
        currentAwareness = newState;
        if (newState == AwarenessState.PATROLLING) { _currentGazeMode = GazeMode.Idle; }
        if (newState == AwarenessState.HUNTING) { LastKnownPlayerPosition = _player.position; if (_playerRb != null) _lastKnownPlayerVelocity = _playerRb.linearVelocity; _awarenessTimer = memoryDuration; }
        if (oldState == AwarenessState.HUNTING && newState == AwarenessState.ALERT) { LastKnownPlayerPosition = _player.position; if (_playerRb != null) _lastKnownPlayerVelocity = _playerRb.linearVelocity; _currentSearchAngle = 0f; }
    }

    public bool CanSeePlayer()
    {
        // ... (código igual) ...
        if (_player == null) return false;
        Vector2 directionToPlayer = (_player.position - eyes.position).normalized;
        float distanceToPlayer = Vector2.Distance(eyes.position, _player.position);
        if (distanceToPlayer > visionRange) return false;
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToPlayer, distanceToPlayer, visionBlockers);
        if (hit.collider != null) return false;
        Vector2 eyeForward = eyes.transform.up;
        float angleToPlayer = Vector2.Angle(eyeForward, directionToPlayer);
        return angleToPlayer < visionAngle / 2f;
    }
    #endregion

    #region Visualização Neural Direta
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || eyes == null) return;
        Vector3 forward = eyes.transform.up;
        Gizmos.color = Color.yellow;
        float stepAngle = 5f;
        for (float angle = -visionAngle / 2; angle < visionAngle / 2; angle += stepAngle)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 direction = rotation * forward;
            RaycastHit2D hit = Physics2D.Raycast(eyes.position, direction, visionRange, visionBlockers);
            Vector3 endPoint = hit.collider ? (Vector3)hit.point : eyes.position + direction * visionRange;
            Gizmos.DrawLine(eyes.position, endPoint);
        }
        if (Application.isPlaying && currentAwareness == AwarenessState.ALERT) { Handles.color = new Color(1, 0.5f, 0, 0.05f); Handles.DrawSolidDisc(eyes.position, Vector3.forward, lkpSafeZoneRadius); Gizmos.color = Color.red; Gizmos.DrawWireSphere(LastKnownPlayerPosition, 1f); Gizmos.color = Color.magenta; Gizmos.DrawRay(LastKnownPlayerPosition, _lastKnownPlayerVelocity.normalized * 3f); }
    }
    #endregion
}