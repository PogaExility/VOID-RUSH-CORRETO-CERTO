using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private int paradoHash, andandoHash, pousandoHash, derrapagemHash, pulandoHash, isDashingHash;

    void Awake()
    {
        animator = GetComponent<Animator>();
        paradoHash = Animator.StringToHash("parado");
        andandoHash = Animator.StringToHash("andando");
        pousandoHash = Animator.StringToHash("pousando");
        derrapagemHash = Animator.StringToHash("derrapagem");
        pulandoHash = Animator.StringToHash("pulando");
        isDashingHash = Animator.StringToHash("isDashing");
    }

    public void UpdateAnimationState(bool idle, bool running, bool falling, bool wallSliding, bool jumping, bool dashing)
    {
        animator.SetBool(paradoHash, idle);
        animator.SetBool(andandoHash, running);
        animator.SetBool(pousandoHash, falling);
        animator.SetBool(derrapagemHash, wallSliding);
        animator.SetBool(pulandoHash, jumping);
        animator.SetBool(isDashingHash, dashing);
    }
}