using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
public class EnemyAnimationLink : MonoBehaviour
{
    private EnemyBrain _brain;
    public Animator animator;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        // Tenta achar no filho (Visual) se não estiver assinalado manualmente
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null || _brain.motor == null) return;

        // Pega a velocidade do RigidBody
        float speed = _brain.motor.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    public void TriggerAttackAnim()
    {
        if (animator) animator.SetTrigger("Attack");
    }

    public void SetKamikazePrepare(bool state)
    {
        if (animator) animator.SetBool("PrepareExplosion", state);
    }
}