// NOME DO ARQUIVO: ObjetoInterativo.cs (VERS�O UNIFICADA)

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

#region ENUMS DE CONFIGURA��O
// Define COMO o objeto � ativado.
public enum ModoDeAtivacao
{
    PorDano,        // Tem vida e quebra com ataques (ex: parede falsa)
    PorHit,         // N�o tem vida, apenas reage a um hit (ex: alavanca)
    PorBotao        // N�o reage a hits, apenas ao bot�o 'E' (ex: painel)
}

// Define SE o objeto pode ser usado mais de uma vez.
public enum ModoDeUso
{
    Unico,          // S� pode ser ativado uma vez.
    Reativavel      // Pode ser ativado e desativado m�ltiplas vezes.
}

// Define o TIPO de feedback visual para ativa��o/desativa��o.
public enum ModoFeedbackVisual
{
    Nenhum,
    TrocarSprite,
    TocarAnimacao
}
#endregion

[RequireComponent(typeof(Collider2D))]
public class ObjetoInterativo : MonoBehaviour
{
    #region CAMPOS DO INSPECTOR
    [Header("1. MODO DE ATIVA��O PRINCIPAL")]
    [Tooltip("Como este objeto � ativado? Isso mudar� as op��es abaixo.")]
    [SerializeField] private ModoDeAtivacao modoDeAtivacao = ModoDeAtivacao.PorDano;

    [Header("2. COMPORTAMENTO GERAL")]
    [Tooltip("Pode ser usado uma vez ou pode ser ligado/desligado?")]
    [SerializeField] private ModoDeUso modoDeUso = ModoDeUso.Unico;

    // --- Op��es para 'Por Dano' ---
    [Header("3. OP��ES PARA 'POR DANO'")]
    [Tooltip("Quantos ataques o objeto aguenta antes de ativar.")]
    [SerializeField] private int vidaMaxima = 3;
    [Tooltip("Qual tipo de ataque pode danificar este objeto.")]
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito_Dano = TipoDeAtaqueAceito.Ambos;
    [Tooltip("A cor que o objeto ir� piscar ao receber dano.")]
    [SerializeField] private Color corDeDano = Color.red;
    [Tooltip("A intensidade do efeito de tremor ao levar dano.")]
    [SerializeField] private float intensidadeTremor = 0.1f;
    [Tooltip("A dura��o do efeito de flash/tremor de dano.")]
    [SerializeField] private float duracaoFeedbackDano = 0.15f;
    [SerializeField] private GameObject efeitoDeQuebraPrefab;

    // --- Op��es para 'Por Hit' ---
    [Header("4. OP��ES PARA 'POR HIT'")]
    [Tooltip("Qual tipo de ataque pode ativar este objeto.")]
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito_Hit = TipoDeAtaqueAceito.Ambos;

    // --- Op��es para 'Por Bot�o' ---
    [Header("5. OP��ES PARA 'POR BOT�O'")]
    [Tooltip("O objeto (ex: um sprite com a tecla 'E') que aparece para mostrar que � poss�vel interagir.")]
    [SerializeField] private GameObject promptVisual;

    // --- Op��es de Feedback de Ativa��o (para Hit e Bot�o) ---
    [Header("6. FEEDBACK DE ATIVA��O (PARA HIT E BOT�O)")]
    [SerializeField] private ModoFeedbackVisual modoVisual = ModoFeedbackVisual.TrocarSprite;
    [Tooltip("Sprite para o estado ATIVADO.")]
    [SerializeField] private Sprite spriteAtivo;
    [Tooltip("Sprite para o estado INATIVO.")]
    [SerializeField] private Sprite spriteInativo;
    [Tooltip("Anima��o ao ATIVAR.")]
    [SerializeField] private AnimationClip clipeAtivando;
    [Tooltip("Anima��o ao DESATIVAR.")]
    [SerializeField] private AnimationClip clipeDesativando;
    [SerializeField] private AudioClip somAtivar;
    [SerializeField] private AudioClip somDesativar;

    [Header("7. A��ES (EVENTOS)")]
    [Tooltip("A��o executada quando o objeto � ativado (vida chega a zero, � atingido ou bot�o 'E' � pressionado).")]
    public UnityEvent aoAtivar;
    [Tooltip("A��o executada quando o objeto � desativado (no modo Reativ�vel).")]
    public UnityEvent aoDesativar;
    #endregion

    #region VARI�VEIS INTERNAS
    private int vidaAtual;
    private bool estaAtivo = false;
    private bool bloqueado = false; // Bloqueia ap�s uso �nico
    private bool jogadorNaArea = false;

    // Componentes
    private SpriteRenderer spriteRenderer;
    private Animation animationComponent;
    private AudioSource audioSource;
    private Vector3 posicaoInicial;
    private Coroutine feedbackDanoCoroutine;
    #endregion

    #region M�TODOS UNITY (AWAKE, START, TRIGGERS)
    private void Awake()
    {
        // Pega componentes essenciais
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animationComponent = GetComponent<Animation>();

        // Configura o colisor
        GetComponent<Collider2D>().isTrigger = (modoDeAtivacao == ModoDeAtivacao.PorBotao);

        // Prepara o componente de Anima��o se necess�rio
        if (modoVisual == ModoFeedbackVisual.TocarAnimacao && animationComponent == null)
        {
            animationComponent = gameObject.AddComponent<Animation>();
        }
        if (animationComponent != null)
        {
            animationComponent.playAutomatically = false;
            if (clipeAtivando != null) animationComponent.AddClip(clipeAtivando, clipeAtivando.name);
            if (clipeDesativando != null) animationComponent.AddClip(clipeDesativando, clipeDesativando.name);
        }

        if (promptVisual != null) promptVisual.SetActive(false);
    }

    private void Start()
    {
        posicaoInicial = transform.position;
        vidaAtual = vidaMaxima;

        // Garante estado visual inicial correto
        if (estaAtivo) TocarFeedbackDeAtivacao(true, true);
        else TocarFeedbackDeAtivacao(false, true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (modoDeAtivacao != ModoDeAtivacao.PorBotao || bloqueado) return;

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // ATEN��O: PlayerController precisar� ser modificado para aceitar ObjetoInterativo
                // player.RegistrarInteragivel(this); 
            }
            jogadorNaArea = true;
            if (promptVisual != null) promptVisual.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (modoDeAtivacao != ModoDeAtivacao.PorBotao) return;

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // ATEN��O: PlayerController precisar� ser modificado
                // player.RemoverInteragivel(this);
            }
            jogadorNaArea = false;
            if (promptVisual != null) promptVisual.SetActive(false);
        }
    }
    #endregion

    #region M�TODOS DE ATIVA��O (P�BLICOS)
    /// <summary>
    /// Ponto de entrada para ataques de armas (chamado pelo SlashEffect e Projectile).
    /// </summary>
    public void ReceberHit(TipoDeAtaqueAceito tipoDoAtaque)
    {
        if (bloqueado) return;

        switch (modoDeAtivacao)
        {
            case ModoDeAtivacao.PorDano:
                if (tipoDeAtaqueAceito_Dano == TipoDeAtaqueAceito.Ambos || tipoDoAtaque == tipoDeAtaqueAceito_Dano)
                {
                    ProcessarDano();
                }
                break;
            case ModoDeAtivacao.PorHit:
                if (tipoDeAtaqueAceito_Hit == TipoDeAtaqueAceito.Ambos || tipoDoAtaque == tipoDeAtaqueAceito_Hit)
                {
                    AlternarEstado();
                }
                break;
        }
    }

    /// <summary>
    /// Ponto de entrada para intera��o com o bot�o 'E' (chamado pelo PlayerController).
    /// </summary>
    public void Interagir()
    {
        if (modoDeAtivacao != ModoDeAtivacao.PorBotao || bloqueado || !jogadorNaArea) return;
        AlternarEstado();
    }
    #endregion

    #region L�GICA INTERNA
    private void ProcessarDano()
    {
        if (vidaAtual <= 0) return;

        vidaAtual--;
        TocarFeedbackDeDano();

        if (vidaAtual <= 0)
        {
            Ativar();
        }
    }

    private void AlternarEstado()
    {
        if (estaAtivo)
        {
            Desativar();
        }
        else
        {
            Ativar();
        }
    }

    private void Ativar()
    {
        if (estaAtivo && modoDeUso == ModoDeUso.Reativavel) return;

        estaAtivo = true;

        if (modoDeAtivacao != ModoDeAtivacao.PorDano)
            TocarFeedbackDeAtivacao(true);
        else if (efeitoDeQuebraPrefab != null)
            Instantiate(efeitoDeQuebraPrefab, transform.position, Quaternion.identity);

        aoAtivar.Invoke();

        if (modoDeUso == ModoDeUso.Unico)
        {
            bloqueado = true;
            if (promptVisual != null) promptVisual.SetActive(false);
            if (modoDeAtivacao == ModoDeAtivacao.PorDano)
            {
                // Objeto se destr�i
                Destroy(gameObject, 0.1f);
            }
        }
    }

    private void Desativar()
    {
        if (!estaAtivo || modoDeUso != ModoDeUso.Reativavel) return;

        estaAtivo = false;

        if (modoDeAtivacao != ModoDeAtivacao.PorDano)
            TocarFeedbackDeAtivacao(false);

        aoDesativar.Invoke();
    }
    #endregion

    #region FEEDBACK
    private void TocarFeedbackDeDano()
    {
        if (feedbackDanoCoroutine != null) StopCoroutine(feedbackDanoCoroutine);
        feedbackDanoCoroutine = StartCoroutine(FeedbackDanoCoroutine());
    }

    private IEnumerator FeedbackDanoCoroutine()
    {
        if (spriteRenderer != null) spriteRenderer.color = corDeDano;

        float tempoDecorrido = 0f;
        while (tempoDecorrido < duracaoFeedbackDano)
        {
            float offsetX = Random.Range(-1f, 1f) * intensidadeTremor;
            float offsetY = Random.Range(-1f, 1f) * intensidadeTremor;
            transform.position = posicaoInicial + new Vector3(offsetX, offsetY, 0);
            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        transform.position = posicaoInicial;
        if (spriteRenderer != null) spriteRenderer.color = Color.white; // Assumindo cor original branca
        feedbackDanoCoroutine = null;
    }

    private void TocarFeedbackDeAtivacao(bool ativar, bool silencioso = false)
    {
        // Visual
        switch (modoVisual)
        {
            case ModoFeedbackVisual.TrocarSprite:
                if (spriteRenderer != null) spriteRenderer.sprite = ativar ? spriteAtivo : spriteInativo;
                break;
            case ModoFeedbackVisual.TocarAnimacao:
                if (animationComponent != null)
                {
                    AnimationClip clipe = ativar ? clipeAtivando : clipeDesativando;
                    if (clipe != null) animationComponent.Play(clipe.name);
                }
                break;
        }
        // Sonoro
        if (!silencioso)
        {
            AudioClip som = ativar ? somAtivar : somDesativar;
            if (som != null) audioSource.PlayOneShot(som);
        }
    }
    #endregion
}