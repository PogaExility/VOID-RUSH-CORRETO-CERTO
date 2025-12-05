using UnityEngine;
using System.Collections.Generic; // Necessário para usar Dicionários

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
    fallingCotoco,
    recarregando,
    Combo1,
    Combo2,
    Combo3,
    abaixando,
    rastejando,
    levantando,
    subindoEscada,
    descendoEscada

}

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Referências dos Animators")]
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator handAnimator;

    [Header("Configurações de Animação")]
    [Tooltip("A duração original, em segundos, do seu clipe de animação 'recarregando'.")]
    [SerializeField] public float reloadAnimationBaseDuration = 1f;

    // MUDANÇA CRÍTICA: O novo "CÉREBRO". Um dicionário para guardar o estado atual de CADA animator.
    // Inicializa aqui para garantir que nunca seja nulo
    private Dictionary<AnimatorTarget, PlayerAnimState> currentStateByTarget = new Dictionary<AnimatorTarget, PlayerAnimState>();
    private int cotocoLayerIndex;

    #region State Hashes
    // Hashes Básicos
    private static readonly int ParadoHash = Animator.StringToHash("parado");
    private static readonly int AndandoHash = Animator.StringToHash("andando");
    private static readonly int PulandoHash = Animator.StringToHash("pulando");
    private static readonly int FallingHash = Animator.StringToHash("falling");
    private static readonly int DashHash = Animator.StringToHash("dash");
    private static readonly int DerrapagemHash = Animator.StringToHash("derrapagem");
    private static readonly int PousandoHash = Animator.StringToHash("pousando");
    private static readonly int AbaixandoHash = Animator.StringToHash("abaixando");
    private static readonly int RastejandoHash = Animator.StringToHash("rastejando");
    private static readonly int LevantandoHash = Animator.StringToHash("levantando");
    private static readonly int SubindoEscadaHash = Animator.StringToHash("subindoEscada");
    private static readonly int DescendoEscadaHash = Animator.StringToHash("descendoEscada");

    // Hashes de Combate/Ação
    private static readonly int BlockHash = Animator.StringToHash("block");
    private static readonly int ParryHash = Animator.StringToHash("parry");
    private static readonly int DashAereoHash = Animator.StringToHash("dashAereo");
    private static readonly int FlipHash = Animator.StringToHash("flip");
    private static readonly int ReloadingHash = Animator.StringToHash("recarregando");

    // CORREÇÃO: Certifique-se que estes nomes existem no seu Animator
    private static readonly int DanoHash = Animator.StringToHash("dano");
    private static readonly int MorrendoHash = Animator.StringToHash("morrendo");

    // Hashes do Cotoco (Mira) - CORRIGIDO O NOME "pular" para "pulando"
    private static readonly int ParadoCotocoHash = Animator.StringToHash("paradoCotoco");
    private static readonly int AndarCotocoHash = Animator.StringToHash("andarCotoco");
    private static readonly int PulandoCotocoHash = Animator.StringToHash("pulandoCotoco"); // <--- AQUI ESTAVA O ERRO PROVAVELMENTE
    private static readonly int FallingCotocoHash = Animator.StringToHash("fallingCotoco");

    // Hashes de Combo
    private static readonly int PoucaVidaParadoHash = Animator.StringToHash("poucaVidaParado");
    private static readonly int Combo1Hash = Animator.StringToHash("Combo1");
    private static readonly int Combo2Hash = Animator.StringToHash("Combo2");
    private static readonly int Combo3Hash = Animator.StringToHash("Combo3");
    #endregion


    void Awake()
    {
        if (bodyAnimator == null) bodyAnimator = GetComponent<Animator>();
        currentStateByTarget = new Dictionary<AnimatorTarget, PlayerAnimState>();

        cotocoLayerIndex = bodyAnimator.GetLayerIndex("CotocoLayer");

        if (cotocoLayerIndex == -1)
        {
            Debug.LogWarning("AVISO: A layer 'CotocoLayer' não foi encontrada no Animator do corpo. A mira não vai funcionar. Verifique se o nome está escrito exatamente igual.", this.gameObject);
        }
    }
    public void SetAnimatorFloat(AnimatorTarget target, string parameterName, float value)
    {
        Animator targetAnimator = GetTargetAnimator(target);
        if (targetAnimator != null)
        {
            // CORREÇÃO: "parametername" foi trocado para "parameterName"
            targetAnimator.SetFloat(parameterName, value);
        }
    }

    public void PlayState(AnimatorTarget target, PlayerAnimState state, int layer = 0)
    {
        Animator targetAnimator = GetTargetAnimator(target);
        if (targetAnimator == null) return;

        int stateHash = GetStateHash(state);

        // --- CORREÇÃO DE SEGURANÇA ---
        // Antes: Só checava a variável do script.
        // Agora: Checa se a Unity REALMENTE está tocando a animação certa.
        if (currentStateByTarget.TryGetValue(target, out PlayerAnimState currentState))
        {
            if (currentState == state)
            {
                // Se o script diz que está tocando, vamos confirmar com o Animator
                if (targetAnimator.GetCurrentAnimatorStateInfo(layer).shortNameHash == stateHash)
                {
                    // Está tocando mesmo, então não faz nada para não reiniciar o clipe
                    return;
                }
                // Se chegou aqui, é porque o script achou que estava tocando, mas o Animator estava em outra.
                // Então deixamos passar para FORÇAR o Play novamente.
            }
        }

        currentStateByTarget[target] = state;

        // Se for morte, forçamos um Debug para você saber que o código funcionou
        if (state == PlayerAnimState.morrendo)
        {
            Debug.Log($"[ANIMATOR] Tentando tocar estado: morrendo (Hash: {stateHash}). Verifique se o nome no Animator é exatamente 'morrendo'.");
        }

        targetAnimator.Play(stateHash, layer, 0f);
    }

    public void PlayState(AnimatorTarget target, PlayerAnimState state, float normalizedTime)
    {
        Animator targetAnimator = GetTargetAnimator(target);
        if (targetAnimator == null) return;

        currentStateByTarget[target] = state;
        int stateHash = GetStateHash(state);
        targetAnimator.Play(stateHash, 0, normalizedTime);
    }

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
    public void SetAimLayerWeight(float weight)
    {
        weight = Mathf.Clamp01(weight);

        if (cotocoLayerIndex != -1)
        {
            bodyAnimator.SetLayerWeight(cotocoLayerIndex, weight);
        }
    }

    public void PlayStateByName(AnimatorTarget target, string stateName, int layer = 0)
    {
        Animator targetAnimator = GetTargetAnimator(target);
        if (targetAnimator != null && targetAnimator.isActiveAndEnabled)
        {
            targetAnimator.Play(stateName, layer, 0f);
        }
        else if (targetAnimator == null)
        {
            Debug.LogError($"Maestro falhou: Animator para o alvo '{target}' é NULO.", this);
        }
    }

    public void SetAnimatorSpeed(AnimatorTarget target, float speed)
    {
        Animator targetAnimator = GetTargetAnimator(target);
        if (targetAnimator != null)
        {
            targetAnimator.speed = speed;
        }
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

            // --- ADIÇÕES PARA MORTE E DANO ---
            case PlayerAnimState.dano: return DanoHash;
            case PlayerAnimState.morrendo: return MorrendoHash;
            // ---------------------------------

            case PlayerAnimState.poucaVidaParado: return PoucaVidaParadoHash;
            case PlayerAnimState.paradoCotoco: return ParadoCotocoHash;
            case PlayerAnimState.andarCotoco: return AndarCotocoHash;
            case PlayerAnimState.pulandoCotoco: return PulandoCotocoHash;
            case PlayerAnimState.fallingCotoco: return FallingCotocoHash;
            case PlayerAnimState.recarregando: return ReloadingHash;
            case PlayerAnimState.Combo1: return Combo1Hash;
            case PlayerAnimState.Combo2: return Combo2Hash;
            case PlayerAnimState.Combo3: return Combo3Hash;
            case PlayerAnimState.abaixando: return AbaixandoHash;
            case PlayerAnimState.rastejando: return RastejandoHash;
            case PlayerAnimState.levantando: return LevantandoHash;
            case PlayerAnimState.subindoEscada: return SubindoEscadaHash;
            case PlayerAnimState.descendoEscada: return DescendoEscadaHash;
            default: return ParadoHash;
        }
    }
}