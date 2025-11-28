using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
public class EnemyAnimationLink : MonoBehaviour
{
    private EnemyBrain _brain;
    public Animator animator;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null || _brain.motor == null) return;
        // Usa a velocidade real para definir Idle/Walk
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

    public void TriggerFinalPhase()
    {
        if (animator) animator.SetTrigger("FinalPhase");
    }
}