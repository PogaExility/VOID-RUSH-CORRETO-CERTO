using UnityEngine;
using UnityEditor;

public class AIPerceptionSystem : MonoBehaviour
{
    #region Estados e Configurações
    public enum GazeMode { Idle, TargetTracking, InvestigatingDanger }
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
    public float eyeRotationSpeed = 5f;
    public float lkpSafeZoneRadius = 1.5f;

    [Header("▶ Gaze de Patrulha")]
    public float dangerInvestigationDuration = 2.0f;

    [Header("▶ Atributos de Alerta")]
    public float inertiaDuration = 0.5f;
    public float searchScanSpeed = 90f;
    public float searchScanAngle = 60f;
    public float memoryDuration = 10f;

    private GazeMode _currentGazeMode = GazeMode.Idle;
    private float _gazeTimer;
    private Vector3 _dangerFocusPoint;
    public Vector3 LastKnownPlayerPosition { get; private set; }
    public bool IsAwareOfPlayer => currentAwareness == AwarenessState.ALERT || currentAwareness == AwarenessState.HUNTING;
    private float _awarenessTimer = 0f;
    private Quaternion _targetEyeRotation = Quaternion.identity;
    private Vector3 _lastKnownPlayerVelocity;
    private float _currentSearchAngle;

    private Transform _player;
    private Rigidbody2D _playerRb;
    private AINavigationSystem _navigation;
    #endregion

    #region Ciclo de Vida e Deteção
    void Start()
    {
        _player = AIManager.Instance.playerTarget;
        _playerRb = _player.GetComponent<Rigidbody2D>();
        _navigation = GetComponent<AINavigationSystem>();
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
    #endregion

    #region Lógica Central de Mira dos Olhos
    private void UpdateEyeTarget()
    {
        Vector3 directionToTarget;

        if (currentAwareness == AwarenessState.HUNTING || currentAwareness == AwarenessState.ALERT)
        {
            _currentGazeMode = GazeMode.TargetTracking;
        }
        else
        {
            if (_navigation.DetectTerrainDanger(out _dangerFocusPoint) && _currentGazeMode != GazeMode.InvestigatingDanger)
            {
                _currentGazeMode = GazeMode.InvestigatingDanger;
                _gazeTimer = dangerInvestigationDuration;
            }
            if (_currentGazeMode == GazeMode.InvestigatingDanger)
            {
                _gazeTimer -= Time.deltaTime;
                if (_gazeTimer <= 0) { _currentGazeMode = GazeMode.Idle; }
            }
        }

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

            case GazeMode.InvestigatingDanger:
                directionToTarget = (_dangerFocusPoint - eyes.position).normalized;
                break;

            case GazeMode.Idle:
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
    #endregion

    #region Lógica de Estados e Percepção
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
        if (newState == AwarenessState.PATROLLING) { _currentGazeMode = GazeMode.Idle; }

        // AQUI ESTÁ A MELHORIA DE PRECISÃO
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
        Vector2 eyeForward = eyes.transform.up;
        float angleToPlayer = Vector2.Angle(eyeForward, directionToPlayer);
        return angleToPlayer < visionAngle / 2f;
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        if (eyes == null) return;
        Vector3 forward = eyes.transform.up;
        Gizmos.color = new Color(1, 1, 0, 0.25f);
        float stepAngle = 5f;
        for (float angle = -visionAngle / 2; angle < visionAngle / 2; angle += stepAngle)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 direction = rotation * forward;
            RaycastHit2D hit = Physics2D.Raycast(eyes.position, direction, visionRange, visionBlockers);
            if (hit.collider != null) Gizmos.DrawLine(eyes.position, hit.point);
            else Gizmos.DrawLine(eyes.position, eyes.position + direction * visionRange);
        }
        if (Application.isPlaying && _currentGazeMode == GazeMode.InvestigatingDanger) { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(_dangerFocusPoint, 0.5f); Gizmos.DrawLine(eyes.position, _dangerFocusPoint); }
        if (Application.isPlaying && currentAwareness == AwarenessState.ALERT) { Handles.color = new Color(1, 0.5f, 0, 0.05f); Handles.DrawSolidDisc(eyes.position, Vector3.forward, lkpSafeZoneRadius); Gizmos.color = Color.red; Gizmos.DrawWireSphere(LastKnownPlayerPosition, 1f); Gizmos.color = Color.magenta; Gizmos.DrawRay(LastKnownPlayerPosition, _lastKnownPlayerVelocity.normalized * 3f); }
    }
    #endregion
}