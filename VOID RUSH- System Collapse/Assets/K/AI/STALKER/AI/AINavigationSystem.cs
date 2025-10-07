using UnityEngine;

public class AINavigationSystem : MonoBehaviour
{
    public enum ObstacleType { None, FullWall, JumpableWall, CrouchTunnel, Ledge }

    public struct NavQueryResult
    {
        public ObstacleType detectedObstacle;
        public float distanceToObstacle;
        public bool isGrounded;
    }

    #region HIERARQUIA DE SONDAS (6 ESPECIALISTAS)
    [Header("▶ ARQUITETURA FINAL (GRID GRANULAR)")]
    [Tooltip("Sonda de detecção na altura do Tile 1 (Base).")]
    public Transform Probe_Height_1_Base;
    [Tooltip("Sonda de detecção na altura do Tile 2 (Meio).")]
    public Transform Probe_Height_2_Mid;
    [Tooltip("Sonda de detecção na altura do Tile 3 (Topo).")]
    public Transform Probe_Height_3_Top;
    [Tooltip("Sonda especialista em detectar precipícios. Olha para BAIXO.")]
    public Transform Probe_Ledge_Check;
    [Tooltip("Sonda especialista em detectar tetos baixos. Olha para CIMA.")]
    public Transform Probe_Ceiling_Check;
    [Tooltip("Sonda de segurança para se levantar / permanecer agachado. Olha para CIMA.")]
    public Transform Probe_Crouch_Safety;
    #endregion

    #region CONFIGURAÇÃO
    [Header("▶ Configuração das Sondas")]
    public LayerMask obstacleLayer;
    public float detectionDistance = 5f;
    public float ceilingProbeHeight = 0.5f;
    #endregion

    private AIPlatformerMotor _motor;
    void Awake() { _motor = GetComponent<AIPlatformerMotor>(); }

    public NavQueryResult QueryEnvironment()
    {
        var result = new NavQueryResult
        {
            isGrounded = _motor.IsGrounded(),
            detectedObstacle = ObstacleType.None,
            distanceToObstacle = float.MaxValue
        };

        // PRIORIDADE 1: Detecção de Paredes usando o grid
        RaycastHit2D hitBase = Probe(Probe_Height_1_Base, transform.right, detectionDistance);
        if (hitBase.collider != null)
        {
            result.distanceToObstacle = hitBase.distance;

            // Classifica a parede com base na altura do grid
            bool hitMid = Probe(Probe_Height_2_Mid, transform.right, hitBase.distance).collider != null;
            bool hitTop = Probe(Probe_Height_3_Top, transform.right, hitBase.distance).collider != null;

            if (hitTop) // Se a sonda mais alta acerta, é uma parede intransponível.
            {
                result.detectedObstacle = ObstacleType.FullWall;
            }
            else // Se a mais alta não acerta, é pulável (seja de 1 ou 2 tiles).
            {
                result.detectedObstacle = ObstacleType.JumpableWall;
            }
        }
        // PRIORIDADE 2: Detecção de Túneis (só se não houver parede na base)
        else if (Probe(Probe_Ceiling_Check, Vector2.up, ceilingProbeHeight).collider != null)
        {
            result.detectedObstacle = ObstacleType.CrouchTunnel;
            result.distanceToObstacle = 0;
        }
        // PRIORIDADE 3: Detecção de Bordas (último recurso)
        else if (!Probe(Probe_Ledge_Check, Vector2.down, detectionDistance).collider)
        {
            result.detectedObstacle = ObstacleType.Ledge;
            result.distanceToObstacle = Vector3.Distance(transform.position, Probe_Ledge_Check.position);
        }

        return result;
    }

    public bool CanStandUp()
    {
        return !Probe(Probe_Crouch_Safety, Vector2.up, 0.1f).collider;
    }

    private RaycastHit2D Probe(Transform origin, Vector2 direction, float distance)
    {
        if (origin == null) { Debug.LogError($"Sonda não atribuída no Inspector!", this); return new RaycastHit2D(); }
        return Physics2D.Raycast(origin.position, direction, distance, obstacleLayer);
    }
}