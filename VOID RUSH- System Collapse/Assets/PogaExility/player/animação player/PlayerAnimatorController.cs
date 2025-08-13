using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;

    // --- Hashes de Movimento ---
    private int paradoHash, andandoHash, pousandoHash, derrapagemHash, pulandoHash, isDashingHash;

    // --- Hashes de Combate (NOVOS) ---
    private int blockHash, parryHash, flipHash;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Cache dos hashes de movimento
        paradoHash = Animator.StringToHash("parado");
        andandoHash = Animator.StringToHash("andando");
        pousandoHash = Animator.StringToHash("pousando");
        derrapagemHash = Animator.StringToHash("derrapagem");
        pulandoHash = Animator.StringToHash("pulando");
        isDashingHash = Animator.StringToHash("isDashing");

        // Cache dos hashes de combate (NOVOS)
        blockHash = Animator.StringToHash("block");
        parryHash = Animator.StringToHash("parry");
        flipHash = Animator.StringToHash("flip");
    }

    // A função foi atualizada para receber os novos estados de combate.
    public void UpdateAnimationState(
        bool idle, bool running, bool falling, bool wallSliding,
        bool jumping, bool dashing,
        bool blocking, bool parrying, bool flipping) // <-- NOVOS PARÂMETROS
    {
        // Movimento
        animator.SetBool(paradoHash, idle);
        animator.SetBool(andandoHash, running);
        animator.SetBool(pousandoHash, falling);
        animator.SetBool(derrapagemHash, wallSliding);
        animator.SetBool(pulandoHash, jumping);
        animator.SetBool(isDashingHash, dashing);

        // Combate (NOVOS)
        animator.SetBool(blockHash, blocking);
        animator.SetBool(parryHash, parrying);
        animator.SetBool(flipHash, flipping);
    }
}