// NOME DO ARQUIVO: ObjetoInterativo.cs (VERS�O FINAL COM ANIMATOR)

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic; // Necess�rio para a lista no Override Controller

#region ENUMS DE CONFIGURA��O
public enum TipoDeAtaqueAceito { ApenasMelee, ApenasRanged, Ambos }
public enum ModoDeAtivacao { PorDano, PorHit, PorBotao }
public enum ModoDeUso { Unico, Reativavel }
public enum ModoFeedbackVisual { Nenhum, TrocarSprite, TocarAnimacao }
#endregion

[RequireComponent(typeof(Collider2D))]
public class ObjetoInterativo : MonoBehaviour
{
    #region CAMPOS DO INSPECTOR
    [Header("1. MODO DE ATIVA��O PRINCIPAL")]
    [SerializeField] private ModoDeAtivacao modoDeAtivacao = ModoDeAtivacao.PorDano;

    [Header("2. COMPORTAMENTO GERAL")]
    [SerializeField] private ModoDeUso modoDeUso = ModoDeUso.Unico;

    // --- Op��es para 'Por Dano' ---
    [Header("3. OP��ES PARA 'POR DANO'")]
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito_Dano = TipoDeAtaqueAceito.Ambos;
    [SerializeField] private Color corDeDano = Color.red;
    [SerializeField] private float intensidadeTremor = 0.1f;
    [SerializeField] private float duracaoFeedbackDano = 0.15f;
    [SerializeField] private GameObject efeitoDeQuebraPrefab;

    // --- Op��es para 'Por Hit' ---
    [Header("4. OP��ES PARA 'POR HIT'")]
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito_Hit = TipoDeAtaqueAceito.Ambos;

    // --- Op��es para 'Por Bot�o' ---
    [Header("5. OP��ES PARA 'POR BOT�O'")]
    [SerializeField] private GameObject promptVisual;

    // --- Op��es de Feedback de Ativa��o ---
    [Header("6. FEEDBACK DE ATIVA��O")]
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

    [Header("7. A��ES (EVENTOS)")]
    public UnityEvent aoAtivar;
    public UnityEvent aoDesativar;
    #endregion

    #region VARI�VEIS INTERNAS
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

    #region M�TODOS UNITY
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Pega o Animator, n�o adiciona um se n�o houver

        GetComponent<Collider2D>().isTrigger = (modoDeAtivacao == ModoDeAtivacao.PorBotao);

        // L�GICA DE ANIMA��O COM ANIMATOR
        if (modoVisual == ModoFeedbackVisual.TocarAnimacao)
        {
            if (animator == null)
            {
                Debug.LogError($"Objeto '{gameObject.name}' est� em modo TocarAnimacao mas n�o tem um componente 'Animator'!", this);
                return;
            }
            if (controllerBase == null)
            {
                Debug.LogError($"Objeto '{gameObject.name}' est� em modo TocarAnimacao mas n�o tem um Controller Base configurado!", this);
                return;
            }

            // Cria um Animator Override Controller em tempo de execu��o
            overrideController = new AnimatorOverrideController(controllerBase);
            animator.runtimeAnimatorController = overrideController;
        }
        else if (animator != null)
        {
            // Se n�o usamos anima��o, desativa o Animator para n�o interferir
            animator.enabled = false;
        }

        if (promptVisual != null) promptVisual.SetActive(false);
    }

    private void Start()
    {
        posicaoInicial = transform.position;
        vidaAtual = vidaMaxima;

        // Aplica os clipes de anima��o (override) no Start, ap�s o Animator ser inicializado
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

    #region M�TODOS DE ATIVA��O P�BLICOS
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

    #region L�GICA INTERNA
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