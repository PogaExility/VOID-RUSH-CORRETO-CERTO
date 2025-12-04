using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [Header("ARRASTE O PERFIL AQUI")]
    public SO_EnemyStats stats;

    [Header("Debug Visual")]
    public bool showGizmos = true;

    [Header("Referências Visuais")]
    public Transform eyes;
    public Transform attackPoint;

    [HideInInspector] public EnemyMotor motor;
    [HideInInspector] public EnemySensors sensors;
    [HideInInspector] public EnemyCombat combat;
    [HideInInspector] public EnemyHealth health;

    public Transform CurrentTarget { get; set; }
    public Vector3 LastKnownPosition { get; set; }

    // Posição onde o inimigo nasceu (Home)
    public Vector3 StartPosition { get; private set; }

    void Awake()
    {
        motor = GetComponent<EnemyMotor>();
        sensors = GetComponent<EnemySensors>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<EnemyHealth>();

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.applyRootMotion = false;

        if (attackPoint == null) attackPoint = transform;
        if (stats == null) Debug.LogError($"[EnemyBrain] {name} sem SO_EnemyStats!");
    }

    void Start()
    {
        // Salva onde o inimigo nasceu
        StartPosition = transform.position;
    }

    void Update()
    {
        // --- LÓGICA DE RETORNO AO SPAWN ---
        // Se não tem alvo detectado, verifica se precisa voltar para casa
        if (CurrentTarget == null && motor != null)
        {
            float distToHome = Vector2.Distance(transform.position, StartPosition);

            // Se estiver longe de casa, anda até lá
            if (distToHome > stats.stopDistancePadding)
            {
                // CORREÇÃO AQUI: Usamos MoveTo em vez de Move
                // Passamos 'false' no segundo parâmetro para indicar que NÃO é perseguição (usa patrolSpeed)
                motor.MoveTo(StartPosition, false);
            }
            else
            {
                // Chegou em casa, para e trava
                motor.Stop();
            }
        }
    }

    public void OnPlayerDetected(Transform player)
    {
        CurrentTarget = player;
        LastKnownPosition = player.position;
    }

    public void ResetEyeRotation()
    {
        if (eyes != null) eyes.localRotation = Quaternion.identity;
    }
}