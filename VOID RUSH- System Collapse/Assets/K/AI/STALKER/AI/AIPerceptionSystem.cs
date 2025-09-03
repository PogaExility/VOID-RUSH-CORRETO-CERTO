using UnityEngine;
using UnityEditor;

public class AIPerceptionSystem : MonoBehaviour
{
    #region Estados e Configurações
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
    [Tooltip("Raio da 'bolha' pessoal. Se a LKP estiver dentro deste raio, ativa o modo de previsão.")]
    public float lkpSafeZoneRadius = 1.5f; // NOVO: A Zona Anti-Flicker

    [Header("▶ Atributos de Alerta")]
    public float inertiaDuration = 0.5f;
    public float searchScanSpeed = 90f;
    public float searchScanAngle = 60f;

    [Header("▶ Timers")]
    public float suspicionDuration = 3f;
    public float memoryDuration = 10f;

    // Variáveis de Estado Internas
    public Vector3 LastKnownPlayerPosition { get; private set; }
    public bool IsAwareOfPlayer => currentAwareness == AwarenessState.ALERT || currentAwareness == AwarenessState.HUNTING;
    private float _awarenessTimer = 0f;
    private Vector3 _initialEyesPosition;
    private Quaternion _targetEyeRotation = Quaternion.identity;
    private Vector3 _lastKnownPlayerVelocity;
    private float _currentSearchAngle;

    private Transform _player;
    private Rigidbody2D _playerRb;
    private AIPlatformerMotor _motor;
    #endregion

    #region Ciclo de Vida do Unity
    void Start()
    {
        _player = AIManager.Instance.playerTarget;
        _playerRb = _player.GetComponent<Rigidbody2D>();
        _motor = GetComponent<AIPlatformerMotor>();

        if (eyes == null)
        {
            Debug.LogError($"[AIPerceptionSystem] FATAL: O Transform 'Eyes' não foi atribuído no Inspector de {gameObject.name}!");
            this.enabled = false;
            return;
        }
        _initialEyesPosition = eyes.localPosition;
    }

    void Update()
    {
        HandleAwareness();
        UpdateEyeTarget();
        UpdateEyeRotation();
        AnimateEyesJiggle();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAwareOfPlayer && other.CompareTag("Player"))
        {
            ChangeAwareness(AwarenessState.HUNTING);
        }
    }
    #endregion

    #region Lógica Central de Mira dos Olhos (REFEITA COM PREVISÃO)
    private void UpdateEyeTarget()
    {
        Vector3 directionToTarget;

        switch (currentAwareness)
        {
            case AwarenessState.HUNTING:
                directionToTarget = (_player.position - eyes.position).normalized;
                _targetEyeRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
                break;

            case AwarenessState.ALERT:
                float distanceToLKP = Vector3.Distance(eyes.position, LastKnownPlayerPosition);

                // --- A NOVA LÓGICA DE PREVISÃO ---
                if (distanceToLKP < lkpSafeZoneRadius)
                {
                    // LKP está perto demais! Ativar modo de previsão.
                    // Olha na direção da última velocidade conhecida do jogador.
                    if (_lastKnownPlayerVelocity.sqrMagnitude > 0.01f)
                    {
                        directionToTarget = _lastKnownPlayerVelocity.normalized;
                    }
                    else // Failsafe: se o jogador estava parado, olha para a frente.
                    {
                        directionToTarget = _motor.isFacingRight ? Vector3.right : Vector3.left;
                    }
                }
                else
                {
                    // LKP está a uma distância segura. Usa a lógica normal de inércia/busca.
                    float timeSinceLostSight = memoryDuration - _awarenessTimer;
                    if (timeSinceLostSight < inertiaDuration)
                    {
                        Vector3 predictedPosition = LastKnownPlayerPosition + (_lastKnownPlayerVelocity * timeSinceLostSight);
                        directionToTarget = (predictedPosition - eyes.position).normalized;
                    }
                    else
                    {
                        _currentSearchAngle += Time.deltaTime * searchScanSpeed;
                        Quaternion scanRotation = Quaternion.AngleAxis(Mathf.Sin(_currentSearchAngle * Mathf.Deg2Rad) * searchScanAngle, Vector3.forward);
                        directionToTarget = scanRotation * (LastKnownPlayerPosition - eyes.position).normalized;
                    }
                }
                _targetEyeRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
                break;

            default:
                Vector3 forwardDir = _motor.isFacingRight ? Vector3.right : Vector3.left;
                RaycastHit2D hit = Physics2D.Raycast(eyes.position + forwardDir * 0.5f, Vector2.down, 5f, visionBlockers);
                Vector3 lookTarget = hit.collider ? hit.point : eyes.position + forwardDir;
                directionToTarget = (lookTarget - eyes.position).normalized;
                _targetEyeRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
                break;
        }
    }

    private void UpdateEyeRotation()
    {
        eyes.rotation = Quaternion.Slerp(eyes.rotation, _targetEyeRotation, Time.deltaTime * eyeRotationSpeed);
    }
    #endregion

    #region Lógica de Estados
    private void HandleAwareness()
    {
        if (CanSeePlayer())
        {
            ChangeAwareness(AwarenessState.HUNTING);
        }
        else
        {
            if (currentAwareness == AwarenessState.HUNTING)
            {
                ChangeAwareness(AwarenessState.ALERT);
            }
            else if (currentAwareness == AwarenessState.ALERT)
            {
                _awarenessTimer -= Time.deltaTime;
                if (_awarenessTimer <= 0) ChangeAwareness(AwarenessState.PATROLLING);
            }
        }
    }

    private void ChangeAwareness(AwarenessState newState)
    {
        if (currentAwareness == newState) return;

        var oldState = currentAwareness;
        currentAwareness = newState;

        // Atualiza a informação CRÍTICA no momento em que o estado muda.
        if (newState == AwarenessState.HUNTING)
        {
            LastKnownPlayerPosition = _player.position;
            if (_playerRb != null) _lastKnownPlayerVelocity = _playerRb.linearVelocity;
            _awarenessTimer = memoryDuration;
        }

        if (oldState == AwarenessState.HUNTING && newState == AwarenessState.ALERT)
        {
            // Captura os dados FINAIS no instante em que perdeu o alvo.
            LastKnownPlayerPosition = _player.position;
            if (_playerRb != null) _lastKnownPlayerVelocity = _playerRb.linearVelocity;

            Debug.Log("[AIPerceptionSystem] Alvo perdido! Última velocidade: " + _lastKnownPlayerVelocity);
            _currentSearchAngle = 0f;
        }
    }
    #endregion

    #region Percepção e Animação
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

    private void AnimateEyesJiggle()
    {
        if (eyes == null) return;
        if (currentAwareness == AwarenessState.SUSPICIOUS)
        {
            Vector3 jiggleOffset = new Vector3(Mathf.Sin(Time.time * 30f) * 0.05f, Mathf.Cos(Time.time * 25f) * 0.05f, 0);
            eyes.localPosition = _initialEyesPosition + jiggleOffset;
        }
        else
        {
            eyes.localPosition = _initialEyesPosition;
        }
    }
    #endregion

    #region Gizmos (ATUALIZADO COM O RAIO DE PREVISÃO)
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

        if (Application.isPlaying && currentAwareness == AwarenessState.ALERT)
        {
            // Gizmo para a Zona de Segurança (Anti-Flicker)
            Handles.color = new Color(1, 0.5f, 0, 0.05f); // Laranja transparente
            Handles.DrawSolidDisc(eyes.position, Vector3.forward, lkpSafeZoneRadius);

            // Gizmo para LKP (Vermelho)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(LastKnownPlayerPosition, 1f);

            // Gizmo de Predição (ROSA/MAGENTA)
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(LastKnownPlayerPosition, _lastKnownPlayerVelocity.normalized * 3f); // Desenha um raio de 3 unidades na direção da previsão
        }
    }
    #endregion
}