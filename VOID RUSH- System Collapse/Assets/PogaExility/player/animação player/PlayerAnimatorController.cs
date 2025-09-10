using UnityEngine;
using System.Collections.Generic; // Necess�rio para usar Dicion�rios

public enum AnimatorTarget
{
    PlayerBody,
    PlayerHand
}

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
    flip,
    dano,
    morrendo,
    poucaVidaParado,
    paradoCotoco,
    andarCotoco,
    pulandoCotoco,
    recarregando
}

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Refer�ncias dos Animators")]
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator handAnimator;

    // MUDAN�A CR�TICA: O novo "C�REBRO". Um dicion�rio para guardar o estado atual de CADA animator.
    private Dictionary<AnimatorTarget, PlayerAnimState> currentStateByTarget;

    #region State Hashes
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
    private static readonly int DanoHash = Animator.StringToHash("dano");
    private static readonly int MorrendoHash = Animator.StringToHash("morrendo");
    private static readonly int PoucaVidaParadoHash = Animator.StringToHash("poucaVidaParado");
    private static readonly int ParadoCotocoHash = Animator.StringToHash("paradoCotoco");
    private static readonly int AndarCotocoHash = Animator.StringToHash("andarCotoco");
    private static readonly int PulandoCotocoHash = Animator.StringToHash("pulandoCotoco");
    private static readonly int ReloadingHash = Animator.StringToHash("recarregando");
    #endregion

    void Awake()
    {
        if (bodyAnimator == null)
        {
            bodyAnimator = GetComponent<Animator>();
        }
        // Inicializa o "c�rebro"
        currentStateByTarget = new Dictionary<AnimatorTarget, PlayerAnimState>();
    }

    public void PlayState(AnimatorTarget target, PlayerAnimState state, int layer = 0)
    {
        // MUDAN�A CR�TICA: A L�GICA DO "C�REBRO"
        // 1. Tenta pegar o estado atual que j� est� tocando para este alvo.
        currentStateByTarget.TryGetValue(target, out PlayerAnimState currentState);

        // 2. Se o estado que queremos tocar j� for o atual, n�o fazemos NADA.
        //    Isso impede que a anima��o seja reiniciada todo frame.
        if (currentState == state)
        {
            return;
        }

        Animator targetAnimator = GetTargetAnimator(target);

        if (targetAnimator == null)
        {
            Debug.LogWarning($"Animator para o alvo '{target}' n�o foi configurado.", this);
            return;
        }

        // 3. Se for um novo estado, atualizamos o "c�rebro" e tocamos a anima��o.
        currentStateByTarget[target] = state;
        int stateHash = GetStateHash(state);
        targetAnimator.Play(stateHash, layer, 0f);
    }

    // Fun��o auxiliar para pegar o animator correto.
    private Animator GetTargetAnimator(AnimatorTarget target)
    {
        switch (target)
        {
            case AnimatorTarget.PlayerBody: return bodyAnimator;
            case AnimatorTarget.PlayerHand: return handAnimator;
            default: return null;
        }
    }

    public AnimatorStateInfo GetCurrentAnimatorStateInfo(AnimatorTarget target, int layerIndex = 0)
    {
        Animator targetAnimator = GetTargetAnimator(target);
        return targetAnimator != null ? targetAnimator.GetCurrentAnimatorStateInfo(layerIndex) : default;
    }

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
            case PlayerAnimState.dano: return DanoHash;
            case PlayerAnimState.morrendo: return MorrendoHash;
            case PlayerAnimState.poucaVidaParado: return PoucaVidaParadoHash;
            case PlayerAnimState.paradoCotoco: return ParadoCotocoHash;
            case PlayerAnimState.andarCotoco: return AndarCotocoHash;
            case PlayerAnimState.pulandoCotoco: return PulandoCotocoHash;
            case PlayerAnimState.recarregando: return ReloadingHash;
            default: return ParadoHash;
        }
    }
}