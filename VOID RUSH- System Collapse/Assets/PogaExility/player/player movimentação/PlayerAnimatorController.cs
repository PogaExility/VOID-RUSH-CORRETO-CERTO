using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;

    // Hashes dos seus parâmetros (TODOS BOOLS)
    private int paradoHash;
    private int andandoHash;
    private int pousandoHash;
    private int derrapagemHash;
    private int pulandoHash;
    private int isDashingHash;

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

    // Atualiza os estados contínuos (andar, cair, etc.)
    public void UpdateAnimator(bool idle, bool running, bool falling, bool wallSliding)
    {
        animator.SetBool(paradoHash, idle);
        animator.SetBool(andandoHash, running);
        animator.SetBool(pousandoHash, falling);
        animator.SetBool(derrapagemHash, wallSliding);
    }

    // Métodos específicos para LIGAR/DESLIGAR as animações de ação
    public void SetJumping(bool isJumping)
    {
        animator.SetBool(pulandoHash, isJumping);
    }

    public void SetDashing(bool isDashing)
    {
        animator.SetBool(isDashingHash, isDashing);
    }
}