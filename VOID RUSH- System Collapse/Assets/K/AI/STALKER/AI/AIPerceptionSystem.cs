using UnityEngine;
using UnityEditor;
using System.Collections;

public class AIPerceptionSystem : MonoBehaviour
{
    #region REFERENCES & STATE
    private GazeMode _currentGazeMode = GazeMode.Idle;
    private Vector3 _obstacleFocusPoint;
    public Vector3 LastKnownPlayerPosition { get; private set; }
    public bool IsAwareOfPlayer => currentAwareness == AwarenessState.ALERT || currentAwareness == AwarenessState.HUNTING;
    private float _awarenessTimer = 0f;
    private Quaternion _targetLocalRotation = Quaternion.identity;
    private Vector3 _lastKnownPlayerVelocity;
    private float _currentSearchAngle;
    private Transform _player;
    private Rigidbody2D _playerRb;
    public GazeMode CurrentGazeMode { get; private set; } = GazeMode.Idle;
    private Coroutine _gazeCoroutine;
    #endregion

    #region CONFIGURATION
    public enum GazeMode { Idle, TargetTracking, AnalyzingObstacle }
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
    #endregion

    #region UNITY LIFECYCLE
    void Start()
    {
        _player = AIManager.Instance.playerTarget;
        _playerRb = _player.GetComponent<Rigidbody2D>();
        if (eyes == null) { this.enabled = false; return; }
    }

    void LateUpdate()
    {
        HandleAwareness();
        // A lógica de mira só é chamada se não houver uma coreografia a decorrer
        if (_gazeCoroutine == null)
        {
            UpdateEyeTarget();
        }
        UpdateEyeRotation();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAwareOfPlayer && other.CompareTag("Player")) { ChangeAwareness(AwarenessState.HUNTING); }
    }
    #endregion

    #region PUBLIC API
    public void StartObstacleAnalysis(Vector3 obstaclePoint, bool isLedge)
    {
        CurrentGazeMode = GazeMode.AnalyzingObstacle;
        _obstacleFocusPoint = obstaclePoint;
        if (_gazeCoroutine != null) StopCoroutine(_gazeCoroutine);

        if (isLedge) { _gazeCoroutine = StartCoroutine(AnalyzeLedgeGazeRoutine()); }
        else { _gazeCoroutine = StartCoroutine(AnalyzeWallGazeRoutine()); }
    }

    public void StopObstacleAnalysis()
    {
        if (_gazeCoroutine != null) StopCoroutine(_gazeCoroutine);
        _gazeCoroutine = null;
        CurrentGazeMode = GazeMode.Idle;
    }
    #endregion

    #region CORE LOGIC & GAZE ROUTINES
    private void UpdateEyeTarget()
    {
        Vector3 directionToTarget;
        switch (CurrentGazeMode)
        {
            case GazeMode.TargetTracking:
                if (currentAwareness == AwarenessState.HUNTING) { directionToTarget = _player.position - eyes.position; }
                else
                {
                    if (Vector3.Distance(eyes.position, LastKnownPlayerPosition) < lkpSafeZoneRadius) { directionToTarget = _lastKnownPlayerVelocity.normalized; }
                    else
                    {
                        float timeSinceLostSight = memoryDuration - _awarenessTimer;
                        if (timeSinceLostSight < inertiaDuration) { Vector3 predictedPosition = LastKnownPlayerPosition + (_lastKnownPlayerVelocity * timeSinceLostSight); directionToTarget = predictedPosition - eyes.position; }
                        else { _currentSearchAngle += Time.deltaTime * searchScanSpeed; Quaternion scanRotation = Quaternion.AngleAxis(Mathf.Sin(_currentSearchAngle * Mathf.Deg2Rad) * searchScanAngle, Vector3.forward); directionToTarget = scanRotation * (LastKnownPlayerPosition - eyes.position); }
                    }
                }
                break;
            default:
                directionToTarget = transform.right;
                break;
        }
        Quaternion targetWorldRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget.normalized);
        _targetLocalRotation = Quaternion.Inverse(transform.rotation) * targetWorldRotation;
    }

    private void UpdateEyeRotation()
    {
        eyes.localRotation = Quaternion.Slerp(eyes.localRotation, _targetLocalRotation, Time.deltaTime * eyeRotationSpeed);
    }

    private IEnumerator AnalyzeLedgeGazeRoutine()
    {
        Vector3 downDirection = (_obstacleFocusPoint - eyes.position).normalized;
        Quaternion downRotation = Quaternion.LookRotation(Vector3.forward, downDirection);
        Quaternion targetWorldDown = transform.rotation * downRotation;

        Vector3 upDirection = (_obstacleFocusPoint + Vector3.up * 3f - eyes.position).normalized;
        Quaternion upRotation = Quaternion.LookRotation(Vector3.forward, upDirection);
        Quaternion targetWorldUp = transform.rotation * upRotation;

        float halfDuration = 1.0f;
        float timer = 0;
        while (timer < halfDuration) { _targetLocalRotation = Quaternion.Slerp(eyes.localRotation, Quaternion.Inverse(transform.rotation) * targetWorldDown, timer / halfDuration); timer += Time.deltaTime; yield return null; }

        timer = 0;
        while (timer < halfDuration) { _targetLocalRotation = Quaternion.Slerp(eyes.localRotation, Quaternion.Inverse(transform.rotation) * targetWorldUp, timer / halfDuration); timer += Time.deltaTime; yield return null; }

        _gazeCoroutine = null; // Termina a rotina
    }

    private IEnumerator AnalyzeWallGazeRoutine()
    {
        Vector3 upDirection = (_obstacleFocusPoint + Vector3.up * 3f - eyes.position).normalized;
        Quaternion upRotation = Quaternion.LookRotation(Vector3.forward, upDirection);
        Quaternion targetWorldUp = transform.rotation * upRotation;

        float duration = 1.5f;
        float timer = 0;
        while (timer < duration) { _targetLocalRotation = Quaternion.Slerp(eyes.localRotation, Quaternion.Inverse(transform.rotation) * targetWorldUp, timer / duration); timer += Time.deltaTime; yield return null; }

        _gazeCoroutine = null; // Termina a rotina
    }
    #endregion

    #region AWARENESS & PERCEPTION
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
        var oldState = currentAwareness;
        currentAwareness = newState;

        if (newState == AwarenessState.PATROLLING) { CurrentGazeMode = GazeMode.Idle; }
        if (newState == AwarenessState.HUNTING || newState == AwarenessState.ALERT) { CurrentGazeMode = GazeMode.TargetTracking; }
        if (newState == AwarenessState.HUNTING) { LastKnownPlayerPosition = _player.position; if (_playerRb != null) _lastKnownPlayerVelocity = _playerRb.linearVelocity; _awarenessTimer = memoryDuration; }
        if (oldState == AwarenessState.HUNTING && newState == AwarenessState.ALERT) { LastKnownPlayerPosition = _player.position; if (_playerRb != null) _lastKnownPlayerVelocity = _playerRb.linearVelocity; _currentSearchAngle = 0f; }
    }

    public bool CanSeePlayer()
    {
        if (_player == null) return false;
        Vector2 directionToPlayer = (_player.position - eyes.position).normalized;
        float distanceToPlayer = Vector2.Distance(eyes.position, _player.position);
        if (distanceToPlayer > visionRange) return false;
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, directionToPlayer, distanceToPlayer, visionBlockers);
        if (hit.collider != null) return false;
        Vector2 eyeForward = eyes.up;
        float angleToPlayer = Vector2.Angle(eyeForward, directionToPlayer);
        return angleToPlayer < visionAngle / 2f;
    }
    #endregion

    #region VISUALIZATION
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || eyes == null) return;
        Vector3 forward = eyes.up;
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