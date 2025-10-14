// NOME DO ARQUIVO: ObjetoInterativo.cs (VERSÃO UNIFICADA)

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

#region ENUMS DE CONFIGURAÇÃO
// Define COMO o objeto é ativado.
public enum ModoDeAtivacao
{
    PorDano,        // Tem vida e quebra com ataques (ex: parede falsa)
    PorHit,         // Não tem vida, apenas reage a um hit (ex: alavanca)
    PorBotao        // Não reage a hits, apenas ao botão 'E' (ex: painel)
}

// Define SE o objeto pode ser usado mais de uma vez.
public enum ModoDeUso
{
    Unico,          // Só pode ser ativado uma vez.
    Reativavel      // Pode ser ativado e desativado múltiplas vezes.
}

// Define o TIPO de feedback visual para ativação/desativação.
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
    [Header("1. MODO DE ATIVAÇÃO PRINCIPAL")]
    [Tooltip("Como este objeto é ativado? Isso mudará as opções abaixo.")]
    [SerializeField] private ModoDeAtivacao modoDeAtivacao = ModoDeAtivacao.PorDano;

    [Header("2. COMPORTAMENTO GERAL")]
    [Tooltip("Pode ser usado uma vez ou pode ser ligado/desligado?")]
    [SerializeField] private ModoDeUso modoDeUso = ModoDeUso.Unico;

    // --- Opções para 'Por Dano' ---
    [Header("3. OPÇÕES PARA 'POR DANO'")]
    [Tooltip("Quantos ataques o objeto aguenta antes de ativar.")]
    [SerializeField] private int vidaMaxima = 3;
    [Tooltip("Qual tipo de ataque pode danificar este objeto.")]
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito_Dano = TipoDeAtaqueAceito.Ambos;
    [Tooltip("A cor que o objeto irá piscar ao receber dano.")]
    [SerializeField] private Color corDeDano = Color.red;
    [Tooltip("A intensidade do efeito de tremor ao levar dano.")]
    [SerializeField] private float intensidadeTremor = 0.1f;
    [Tooltip("A duração do efeito de flash/tremor de dano.")]
    [SerializeField] private float duracaoFeedbackDano = 0.15f;
    [SerializeField] private GameObject efeitoDeQuebraPrefab;

    // --- Opções para 'Por Hit' ---
    [Header("4. OPÇÕES PARA 'POR HIT'")]
    [Tooltip("Qual tipo de ataque pode ativar este objeto.")]
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito_Hit = TipoDeAtaqueAceito.Ambos;

    // --- Opções para 'Por Botão' ---
    [Header("5. OPÇÕES PARA 'POR BOTÃO'")]
    [Tooltip("O objeto (ex: um sprite com a tecla 'E') que aparece para mostrar que é possível interagir.")]
    [SerializeField] private GameObject promptVisual;

    // --- Opções de Feedback de Ativação (para Hit e Botão) ---
    [Header("6. FEEDBACK DE ATIVAÇÃO (PARA HIT E BOTÃO)")]
    [SerializeField] private ModoFeedbackVisual modoVisual = ModoFeedbackVisual.TrocarSprite;
    [Tooltip("Sprite para o estado ATIVADO.")]
    [SerializeField] private Sprite spriteAtivo;
    [Tooltip("Sprite para o estado INATIVO.")]
    [SerializeField] private Sprite spriteInativo;
    [Tooltip("Animação ao ATIVAR.")]
    [SerializeField] private AnimationClip clipeAtivando;
    [Tooltip("Animação ao DESATIVAR.")]
    [SerializeField] private AnimationClip clipeDesativando;
    [SerializeField] private AudioClip somAtivar;
    [SerializeField] private AudioClip somDesativar;

    [Header("7. AÇÕES (EVENTOS)")]
    [Tooltip("Ação executada quando o objeto é ativado (vida chega a zero, é atingido ou botão 'E' é pressionado).")]
    public UnityEvent aoAtivar;
    [Tooltip("Ação executada quando o objeto é desativado (no modo Reativável).")]
    public UnityEvent aoDesativar;
    #endregion

    #region VARIÁVEIS INTERNAS
    private int vidaAtual;
    private bool estaAtivo = false;
    private bool bloqueado = false; // Bloqueia após uso único
    private bool jogadorNaArea = false;

    // Componentes
    private SpriteRenderer spriteRenderer;
    private Animation animationComponent;
    private AudioSource audioSource;
    private Vector3 posicaoInicial;
    private Coroutine feedbackDanoCoroutine;
    #endregion

    #region MÉTODOS UNITY (AWAKE, START, TRIGGERS)
    private void Awake()
    {
        // Pega componentes essenciais
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animationComponent = GetComponent<Animation>();

        // Configura o colisor
        GetComponent<Collider2D>().isTrigger = (modoDeAtivacao == ModoDeAtivacao.PorBotao);

        // Prepara o componente de Animação se necessário
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
                // ATENÇÃO: PlayerController precisará ser modificado para aceitar ObjetoInterativo
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
                // ATENÇÃO: PlayerController precisará ser modificado
                // player.RemoverInteragivel(this);
            }
            jogadorNaArea = false;
            if (promptVisual != null) promptVisual.SetActive(false);
        }
    }
    #endregion

    #region MÉTODOS DE ATIVAÇÃO (PÚBLICOS)
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
    /// Ponto de entrada para interação com o botão 'E' (chamado pelo PlayerController).
    /// </summary>
    public void Interagir()
    {
        if (modoDeAtivacao != ModoDeAtivacao.PorBotao || bloqueado || !jogadorNaArea) return;
        AlternarEstado();
    }
    #endregion

    #region LÓGICA INTERNA
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
                // Objeto se destrói
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