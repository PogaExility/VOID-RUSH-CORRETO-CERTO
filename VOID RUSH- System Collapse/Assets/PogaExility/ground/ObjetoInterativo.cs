// NOME DO ARQUIVO: ObjetoInterativo.cs (VERSÃO FINAL COM ANIMATOR)

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic; // Necessário para a lista no Override Controller

#region ENUMS DE CONFIGURAÇÃO
public enum TipoDeAtaqueAceito { ApenasMelee, ApenasRanged, Ambos }
public enum ModoDeAtivacao { PorDano, PorHit, PorBotao }
public enum ModoDeUso { Unico, Reativavel }
public enum ModoFeedbackVisual { Nenhum, TrocarSprite, TocarAnimacao }
#endregion

[RequireComponent(typeof(Collider2D))]
public class ObjetoInterativo : MonoBehaviour
{
    #region CAMPOS DO INSPECTOR
    [Header("1. MODO DE ATIVAÇÃO PRINCIPAL")]
    [SerializeField] private ModoDeAtivacao modoDeAtivacao = ModoDeAtivacao.PorDano;

    [Header("2. COMPORTAMENTO GERAL")]
    [SerializeField] private ModoDeUso modoDeUso = ModoDeUso.Unico;

    // --- Opções para 'Por Dano' ---
    [Header("3. OPÇÕES PARA 'POR DANO'")]
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito_Dano = TipoDeAtaqueAceito.Ambos;
    [SerializeField] private Color corDeDano = Color.red;
    [SerializeField] private float intensidadeTremor = 0.1f;
    [SerializeField] private float duracaoFeedbackDano = 0.15f;
    [SerializeField] private GameObject efeitoDeQuebraPrefab;

    // --- Opções para 'Por Hit' ---
    [Header("4. OPÇÕES PARA 'POR HIT'")]
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito_Hit = TipoDeAtaqueAceito.Ambos;

    // --- Opções para 'Por Botão' ---
    [Header("5. OPÇÕES PARA 'POR BOTÃO'")]
    [SerializeField] private GameObject promptVisual;

    // --- Opções de Feedback de Ativação ---
    [Header("6. FEEDBACK DE ATIVAÇÃO")]
    [SerializeField] private ModoFeedbackVisual modoVisual = ModoFeedbackVisual.TrocarSprite;
    // Campo para o Animator Controller Base
    [Tooltip("Arraste aqui o Animator Controller base, como o 'AC_InteragivelBase'.")]
    [SerializeField] private RuntimeAnimatorController controllerBase;
    [SerializeField] private Sprite spriteAtivo;
    [SerializeField] private Sprite spriteInativo;
    [SerializeField] private AnimationClip clipeAtivando;
    [SerializeField] private AnimationClip clipeDesativando;
    [SerializeField] private AudioClip somAtivar;
    [SerializeField] private AudioClip somDesativar;

    [Header("7. AÇÕES (EVENTOS)")]
    public UnityEvent aoAtivar;
    public UnityEvent aoDesativar;
    #endregion

    #region VARIÁVEIS INTERNAS
    private int vidaAtual;
    private bool estaAtivo = false;
    private bool bloqueado = false;
    private bool jogadorNaArea = false;
    // Componentes ATUALIZADOS
    private SpriteRenderer spriteRenderer;
    private Animator animator; // Agora usamos Animator
    private AudioSource audioSource;
    private Vector3 posicaoInicial;
    private Coroutine feedbackDanoCoroutine;
    private AnimatorOverrideController overrideController; // Nosso controller customizado
    #endregion

    #region MÉTODOS UNITY
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Pega o Animator, não adiciona um se não houver

        GetComponent<Collider2D>().isTrigger = (modoDeAtivacao == ModoDeAtivacao.PorBotao);

        // LÓGICA DE ANIMAÇÃO COM ANIMATOR
        if (modoVisual == ModoFeedbackVisual.TocarAnimacao)
        {
            if (animator == null)
            {
                Debug.LogError($"Objeto '{gameObject.name}' está em modo TocarAnimacao mas não tem um componente 'Animator'!", this);
                return;
            }
            if (controllerBase == null)
            {
                Debug.LogError($"Objeto '{gameObject.name}' está em modo TocarAnimacao mas não tem um Controller Base configurado!", this);
                return;
            }

            // Cria um Animator Override Controller em tempo de execução
            overrideController = new AnimatorOverrideController(controllerBase);
            animator.runtimeAnimatorController = overrideController;
        }
        else if (animator != null)
        {
            // Se não usamos animação, desativa o Animator para não interferir
            animator.enabled = false;
        }

        if (promptVisual != null) promptVisual.SetActive(false);
    }

    private void Start()
    {
        posicaoInicial = transform.position;
        vidaAtual = vidaMaxima;

        // Aplica os clipes de animação (override) no Start, após o Animator ser inicializado
        if (modoVisual == ModoFeedbackVisual.TocarAnimacao && overrideController != null)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            // Assume que o controller base tem clipes chamados "placeholder_ativando" e "placeholder_desativando"
            if (clipeAtivando != null)
                overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(overrideController.animationClips[0], clipeAtivando));
            if (clipeDesativando != null && overrideController.animationClips.Length > 1)
                overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(overrideController.animationClips[1], clipeDesativando));

            overrideController.ApplyOverrides(overrides);
        }

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
            if (player != null) player.RegistrarInteragivel(this);
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
            if (player != null) player.RemoverInteragivel(this);
            jogadorNaArea = false;
            if (promptVisual != null) promptVisual.SetActive(false);
        }
    }
    #endregion

    #region MÉTODOS DE ATIVAÇÃO PÚBLICOS
    public void ReceberHit(TipoDeAtaqueAceito tipoDoAtaque)
    {
        if (bloqueado) return;

        switch (modoDeAtivacao)
        {
            case ModoDeAtivacao.PorDano:
                if (tipoDeAtaqueAceito_Dano == TipoDeAtaqueAceito.Ambos || tipoDoAtaque == tipoDeAtaqueAceito_Dano) ProcessarDano();
                break;
            case ModoDeAtivacao.PorHit:
                if (tipoDeAtaqueAceito_Hit == TipoDeAtaqueAceito.Ambos || tipoDoAtaque == tipoDeAtaqueAceito_Hit) AlternarEstado();
                break;
        }
    }

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
        if (vidaAtual <= 0) Ativar();
    }

    private void AlternarEstado()
    {
        if (estaAtivo) Desativar();
        else Ativar();
    }

    private void Ativar()
    {
        if (estaAtivo && modoDeUso == ModoDeUso.Reativavel) return;
        estaAtivo = true;

        if (modoDeAtivacao != ModoDeAtivacao.PorDano) TocarFeedbackDeAtivacao(true);
        else if (efeitoDeQuebraPrefab != null) Instantiate(efeitoDeQuebraPrefab, transform.position, Quaternion.identity);

        aoAtivar.Invoke();

        if (modoDeUso == ModoDeUso.Unico)
        {
            bloqueado = true;
            if (promptVisual != null) promptVisual.SetActive(false);
            if (modoDeAtivacao == ModoDeAtivacao.PorDano) Destroy(gameObject, 0.1f);
        }
    }

    private void Desativar()
    {
        if (!estaAtivo || modoDeUso != ModoDeUso.Reativavel) return;
        estaAtivo = false;
        if (modoDeAtivacao != ModoDeAtivacao.PorDano) TocarFeedbackDeAtivacao(false);
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
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
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
                if (animator != null && animator.enabled)
                {
                    string trigger = ativar ? "Ativar" : "Desativar";
                    animator.SetTrigger(trigger);
                }
                break;
        }
        // Sonoro
        if (!silencioso)
        {
            AudioClip som = ativar ? somAtivar : somDesativar;
            if (som != null && audioSource != null) audioSource.PlayOneShot(som);
        }
    }
    #endregion
}