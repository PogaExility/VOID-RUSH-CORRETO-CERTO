using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    public SO_EnemyStats stats;

    [Header("Referências")]
    public Transform eyes;
    public Transform attackPoint;

    // Acesso rápido aos módulos
    [HideInInspector] public EnemyMotor motor;
    [HideInInspector] public EnemySensors sensors;
    [HideInInspector] public EnemyCombat combat;
    [HideInInspector] public EnemyHealth health;

    public Transform CurrentTarget { get; set; }
    public Vector3 LastKnownPosition { get; set; }

    void Awake()
    {
        motor = GetComponent<EnemyMotor>();
        sensors = GetComponent<EnemySensors>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<EnemyHealth>();

        if (attackPoint == null) attackPoint = transform;
        if (stats == null) Debug.LogError("FALTA O SO_ENEMYSTATS!");
    }

    public void OnPlayerDetected(Transform player)
    {
        CurrentTarget = player;
        LastKnownPosition = player.position;
    }
}