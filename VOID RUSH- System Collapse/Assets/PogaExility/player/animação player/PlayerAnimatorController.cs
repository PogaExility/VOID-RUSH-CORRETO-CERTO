using UnityEngine;


public enum PlayerAnimState
{
    parado,
    andando,
    pulando,
    falling,
    dash,
    derrapagem,
    block,
    pousando,
    parry,
    dashAereo,
    flip
}


[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private PlayerAnimState currentState;
    
   

    // Hashes dos nomes dos estados
    private static readonly int ParadoHash = Animator.StringToHash("parado");
    private static readonly int AndandoHash = Animator.StringToHash("andando");
    private static readonly int PulandoHash = Animator.StringToHash("pulando");
    private static readonly int FallingHash = Animator.StringToHash("falling");
    private static readonly int DashHash = Animator.StringToHash("dash");
    private static readonly int DerrapagemHash = Animator.StringToHash("derrapagem");
    private static readonly int BlockHash = Animator.StringToHash("block");
    private static readonly int PousandoHash = Animator.StringToHash("pousando");
    private static readonly int ParryHash = Animator.StringToHash("parry");
    private static readonly int DashAereoHash = Animator.StringToHash("dashAereo");
    private static readonly int FlipHash = Animator.StringToHash("flip");

    [Tooltip("Duração da transição suave entre animações.")]
    public float crossFadeDuration = 0.1f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public Animator GetAnimator()
    {
        return animator;
    }
    public void PlayState(PlayerAnimState state)
    {
        if (state == currentState) return;

        currentState = state;
        int stateHash = GetStateHash(state);
        animator.CrossFade(stateHash, crossFadeDuration, 0, 0);
    }

    // A função OnLandingAnimationEnd foi REMOVIDA daqui.

    private int GetStateHash(PlayerAnimState state)
    {
        switch (state)
        {
            case PlayerAnimState.parado: return ParadoHash;
            case PlayerAnimState.andando: return AndandoHash;
            case PlayerAnimState.pulando: return PulandoHash;
            case PlayerAnimState.falling: return FallingHash;
            case PlayerAnimState.dash: return DashHash;
            case PlayerAnimState.derrapagem: return DerrapagemHash;
            case PlayerAnimState.block: return BlockHash;
            case PlayerAnimState.pousando: return PousandoHash;
            case PlayerAnimState.parry: return ParryHash;
            case PlayerAnimState.dashAereo: return DashAereoHash;
            case PlayerAnimState.flip: return FlipHash;
            default: return ParadoHash;
        }
    }
}