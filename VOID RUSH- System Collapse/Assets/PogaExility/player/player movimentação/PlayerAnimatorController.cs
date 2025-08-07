using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;

    // Hashes para performance, usando os SEUS nomes de parâmetros
    private int paradoHash;
    private int andandoHash;
    private int pousandoHash;  // 'isFalling' no meu código
    private int derrapagemHash; // 'isWallSliding' no meu código
    private int pulandoTriggerHash; // 'Jump' no meu código
    private int isDashingTriggerHash; // 'Dash' no meu código

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Mapeando os nomes do seu Animator para as variáveis do código
        paradoHash = Animator.StringToHash("parado");       // Era 'isIdle'
        andandoHash = Animator.StringToHash("andando");     // Era 'isRunning'
        pousandoHash = Animator.StringToHash("pousando");   // Era 'isFalling'
        derrapagemHash = Animator.StringToHash("derrapagem"); // Era 'isWallSliding'

        // Assumindo que "pulando" e "isDashing" são Triggers, como configuramos
        pulandoTriggerHash = Animator.StringToHash("pulando");
        isDashingTriggerHash = Animator.StringToHash("isDashing");
    }

    // O método agora recebe os mesmos booleanos de antes, mas usa os seus hashes
    public void UpdateAnimator(bool idle, bool running, bool falling, bool wallSliding)
    {
        // Nota: Assumi que "parado" é um booleano como os outros.
        // Se for um estado default sem parâmetro, esta linha pode ser removida.
        animator.SetBool(paradoHash, idle);

        animator.SetBool(andandoHash, running);
        animator.SetBool(pousandoHash, falling);
        animator.SetBool(derrapagemHash, wallSliding);
    }

    public void TriggerJump()
    {
        // Dispara o trigger "pulando"
        animator.SetTrigger(pulandoTriggerHash);
    }

    public void TriggerDash()
    {
        // Dispara o trigger "isDashing"
        animator.SetTrigger(isDashingTriggerHash);
    }
}