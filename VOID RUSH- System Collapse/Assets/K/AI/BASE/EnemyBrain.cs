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

    void Awake()
    {
        motor = GetComponent<EnemyMotor>();
        sensors = GetComponent<EnemySensors>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<EnemyHealth>();

        // Desliga Root Motion para não bugar a física
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.applyRootMotion = false;

        if (attackPoint == null) attackPoint = transform;
        if (stats == null) Debug.LogError($"[EnemyBrain] {name} sem SO_EnemyStats!");
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