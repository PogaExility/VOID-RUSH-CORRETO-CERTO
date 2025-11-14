using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic; // Adicionado para a lista de pontos do Gizmo

/// <summary>
/// O módulo de "Sentidos" da IA. Este componente é o único responsável por
/// perceber o mundo ao redor, incluindo visão e audição. Ele gerencia os estados de
/// consciência da IA e fornece informações processadas para o AIController.
/// </summary>
public class AIPerception : MonoBehaviour
{
    // =================================================================================================
    // CONFIGURAÇÃO
    // =================================================================================================

    [Header("▶ Referências Essenciais")]
    [Tooltip("O ponto de origem da visão (geralmente um objeto filho na altura da cabeça).")]
    [SerializeField] private Transform eyes;

    [Header("▶ Atributos da Percepção")]
    [Tooltip("Velocidade com que os olhos giram para rastrear um alvo.")]
    [SerializeField] private float eyeRotationSpeed = 8f;
    [Tooltip("Quão sensível a IA é a sons. Valores maiores detectam sons mais distantes/baixos.")]
    [SerializeField][Range(0f, 1f)] private float hearingSensitivity = 0.5f;

    // =================================================================================================
    // ESTADO E REFERÊNCIAS INTERNAS
    // =================================================================================================

    public enum AwarenessState { Dormant, Patrolling, Suspicious, Alert, Hunting }

    public AwarenessState CurrentAwareness { get; private set; } = AwarenessState.Patrolling;
    public Vector2 LastKnownPlayerPosition { get; private set; }
    public bool IsAwareOfPlayer => CurrentAwareness == AwarenessState.Alert || CurrentAwareness == AwarenessState.Hunting;
    public Transform Eyes => eyes;

    private Transform playerTransform;
    private Rigidbody2D playerRb;
    private EnemySO enemyData;
    private AIController controller;

    private float awarenessTimer;
    private Vector2 lastKnownPlayerVelocity;
    private Quaternion targetEyeLocalRotation = Quaternion.identity;

    #region Inicialização e Ciclo de Vida
    public void Initialize(AIController ownerController, Transform target)
    {
        this.controller = ownerController;
        this.enemyData = ownerController.enemyData;
        this.playerTransform = target;

        if (this.playerTransform != null)
        {
            this.playerRb = playerTransform.GetComponent<Rigidbody2D>();
        }

        if (eyes == null)
        {
            Debug.LogWarning($"O inimigo '{gameObject.name}' não tem o Transform 'eyes' atribuído. Usando o transform principal.", this);
            eyes = this.transform;
        }
    }

    private void Update()
    {
        if (enemyData == null || playerTransform == null) return;
        UpdateAwareness();
    }

    private void LateUpdate()
    {
        UpdateGazeTarget();
        UpdateGazeRotation();
    }
    #endregion

    #region API Pública
    public void HearSound(Vector3 soundPosition, float intensity)
    {
        if (IsAwareOfPlayer) return;
        if (intensity < (1f - hearingSensitivity)) return;

        LastKnownPlayerPosition = soundPosition;
        ChangeAwareness(AwarenessState.Suspicious);
    }

    public bool CheckVision()
    {
        if (playerTransform == null) return false;

        Vector2 origin = eyes.position;
        Vector2 target = playerTransform.position;

        float distance = Vector2.Distance(origin, target);
        if (distance > enemyData.visionRange) return false;

        Vector2 directionToPlayer = (target - origin).normalized;

        // --- CORREÇÃO CRÍTICA AQUI ---
        // A direção "para frente" é o 'up' dos olhos, por causa de como o LookRotation funciona em 2D.
        Vector2 forwardDirection = eyes.up;

        float angleToPlayer = Vector2.Angle(forwardDirection, directionToPlayer);
        if (angleToPlayer > enemyData.visionAngle / 2) return false;

        RaycastHit2D hit = Physics2D.Raycast(origin, directionToPlayer, distance, enemyData.obstacleLayer);
        return hit.collider == null || hit.collider.gameObject.layer == playerTransform.gameObject.layer;
    }
    #endregion

    #region Lógica Principal de Percepção
    private void UpdateAwareness()
    {
        awarenessTimer -= Time.deltaTime;

        if (CheckVision())
        {
            ChangeAwareness(AwarenessState.Hunting);
        }
        else
        {
            if (CurrentAwareness == AwarenessState.Hunting)
            {
                ChangeAwareness(AwarenessState.Alert);
            }
            else if (CurrentAwareness == AwarenessState.Alert && awarenessTimer <= 0)
            {
                ChangeAwareness(AwarenessState.Patrolling);
            }
            else if (CurrentAwareness == AwarenessState.Suspicious && awarenessTimer <= 0)
            {
                ChangeAwareness(AwarenessState.Patrolling);
            }
        }
    }

    private void ChangeAwareness(AwarenessState newState)
    {
        if (CurrentAwareness == newState) return;

        CurrentAwareness = newState;

        switch (CurrentAwareness)
        {
            case AwarenessState.Hunting:
                LastKnownPlayerPosition = playerTransform.position;
                if (playerRb != null) lastKnownPlayerVelocity = playerRb.linearVelocity;
                awarenessTimer = enemyData.memoryDuration;
                break;
            case AwarenessState.Alert:
                LastKnownPlayerPosition = playerTransform.position;
                if (playerRb != null) lastKnownPlayerVelocity = playerRb.linearVelocity;
                awarenessTimer = enemyData.memoryDuration;
                break;
            case AwarenessState.Suspicious:
                awarenessTimer = enemyData.memoryDuration / 2f;
                break;
        }
    }
    #endregion

    #region Lógica de Rastreio Visual (Gaze Control)
    private void UpdateGazeTarget()
    {
        Vector3 targetPos;
        if (CurrentAwareness == AwarenessState.Hunting)
        {
            targetPos = playerTransform.position;
        }
        else if (CurrentAwareness == AwarenessState.Alert || CurrentAwareness == AwarenessState.Suspicious)
        {
            float timeSinceLost = enemyData.memoryDuration - awarenessTimer;
            targetPos = LastKnownPlayerPosition + (Vector2)(lastKnownPlayerVelocity * Mathf.Min(timeSinceLost, 0.5f));
        }
        else
        {
            targetPos = eyes.position + transform.right; // Quando ocioso, olha para a frente do corpo.
        }

        Vector3 directionToTarget = targetPos - eyes.position;
        Quaternion targetWorldRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget.normalized);
        targetEyeLocalRotation = Quaternion.Inverse(transform.rotation) * targetWorldRotation;
    }

    private void UpdateGazeRotation()
    {
        if (eyes != null)
        {
            eyes.localRotation = Quaternion.Slerp(eyes.localRotation, targetEyeLocalRotation, Time.deltaTime * eyeRotationSpeed);
        }
    }
    #endregion

    #region Gizmos para Debug

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null)
        {
            if (controller == null) controller = GetComponentInParent<AIController>();
            if (controller == null || controller.enemyData == null) return;
            enemyData = controller.enemyData;
        }

        Vector3 origin = (eyes != null) ? eyes.position : transform.position;

        // --- CORREÇÃO CRÍTICA AQUI TAMBÉM ---
        // O Gizmo deve usar a mesma direção que a lógica: a direção atual dos olhos.
        Vector3 forward = (eyes != null) ? eyes.up : transform.right;

        // Desenha o leque de raios
        int stepCount = 20;
        float stepAngle = enemyData.visionAngle / stepCount;

        switch (Application.isPlaying ? CurrentAwareness : AwarenessState.Patrolling)
        {
            case AwarenessState.Patrolling: Handles.color = new Color(1, 1, 0, 0.1f); break;
            case AwarenessState.Suspicious: Handles.color = new Color(1, 0.5f, 0, 0.15f); break;
            case AwarenessState.Alert: Handles.color = new Color(1, 0.2f, 0, 0.2f); break;
            case AwarenessState.Hunting: Handles.color = new Color(1, 0, 0, 0.25f); break;
        }

        Vector3 arcStartDirection = Quaternion.AngleAxis(-enemyData.visionAngle / 2, Vector3.forward) * forward;
        Handles.DrawSolidArc(origin, Vector3.forward, arcStartDirection, enemyData.visionAngle, enemyData.visionRange);

        // Desenha a última posição conhecida do jogador
        if (Application.isPlaying && (IsAwareOfPlayer || CurrentAwareness == AwarenessState.Suspicious))
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(LastKnownPlayerPosition, Vector3.forward, 0.5f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(LastKnownPlayerPosition, LastKnownPlayerPosition + (Vector2)(lastKnownPlayerVelocity.normalized * 2f));
        }
    }
#endif

    #endregion
}