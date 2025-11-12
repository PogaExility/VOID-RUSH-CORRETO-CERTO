using UnityEngine;
using UnityEditor;

public class AIPerception : MonoBehaviour
{
    [Header("▶ Referências Essenciais")]
    [Tooltip("O ponto de origem da visão (geralmente um objeto filho na altura da cabeça).")]
    [SerializeField] private Transform eyes;

    // Componentes e Estado Interno
    private Transform playerTransform;
    private EnemySO enemyData; // Os dados serão fornecidos pelo AIController
    private AIController controller;

    #region Inicialização

    /// <summary>
    /// Método de inicialização chamado pelo AIController.
    /// </summary>
    public void Initialize(AIController ownerController, Transform target)
    {
        this.controller = ownerController;
        this.enemyData = ownerController.enemyData;
        this.playerTransform = target;

        if (eyes == null)
        {
            Debug.LogWarning($"O inimigo '{gameObject.name}' não tem o Transform 'eyes' atribuído no AIPerception. Usando o transform principal como fallback.", this);
            eyes = this.transform;
        }
    }

    #endregion

    #region Lógica de Percepção

    /// <summary>
    /// A principal função de consulta. Verifica se o jogador pode ser visto.
    /// </summary>
    /// <returns>Verdadeiro se o jogador estiver dentro do cone de visão e sem obstruções.</returns>
    public bool CanSeePlayer()
    {
        if (playerTransform == null || enemyData == null) return false;

        Vector2 origin = eyes.position;
        Vector2 target = playerTransform.position;

        // 1. Verificação de Distância (a mais barata)
        float distanceToPlayer = Vector2.Distance(origin, target);
        if (distanceToPlayer > enemyData.visionRange)
        {
            return false;
        }

        // 2. Verificação de Ângulo
        Vector2 directionToPlayer = (target - origin).normalized;
        Vector2 forwardDirection = transform.right; // Usa o transform.right do objeto PAI
        float angleToPlayer = Vector2.Angle(forwardDirection, directionToPlayer);

        if (angleToPlayer > enemyData.visionAngle / 2)
        {
            return false;
        }

        // 3. Verificação de Linha de Visão (a mais cara)
        RaycastHit2D hit = Physics2D.Raycast(origin, directionToPlayer, distanceToPlayer, enemyData.visionBlockers);
        if (hit.collider != null)
        {
            // Se o raio atingiu algo, verifica se NÃO é o jogador. Se não for, a visão está bloqueada.
            if (!hit.collider.CompareTag("Player")) // Certifique-se que o jogador tem a tag "Player"
            {
                return false;
            }
        }

        // Se passou por todas as verificações, o jogador é visível.
        return true;
    }

    #endregion

    #region Gizmos para Debug

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Só desenha se tivermos os dados (mesmo antes de dar Play)
        if (controller == null) controller = GetComponent<AIController>();
        if (controller == null || controller.enemyData == null) return;

        EnemySO data = controller.enemyData;
        Vector3 origin = (eyes != null) ? eyes.position : transform.position;
        Vector3 forward = transform.right;

        // Desenha o leque de raios
        int stepCount = 20;
        float stepAngle = data.visionAngle / stepCount;

        for (float angle = -data.visionAngle / 2; angle <= data.visionAngle / 2; angle += stepAngle)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * forward;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, data.visionRange, data.visionBlockers);

            Vector3 endPoint = origin + direction * data.visionRange;
            if (hit.collider != null)
            {
                endPoint = hit.point;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, endPoint);
        }

        // Desenha o polígono preenchido
        // (Nota: esta parte é mais complexa de replicar 100% sem o sistema de Estados,
        // mas o leque de raios já é o feedback visual mais importante).
    }
#endif

    #endregion
}