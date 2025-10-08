// NOME DO ARQUIVO: ObjetoInterativo.cs

using UnityEngine;
using UnityEngine.Events;
using System.Collections; // -- NOVO -- Necess�rio para usar Corrotinas (IEnumerator)

public enum TipoDeAtaqueAceito
{
    ApenasMelee,
    ApenasRanged,
    Ambos
}

public class ObjetoInterativo : MonoBehaviour
{
    [Header("Configura��o de Vida")]
    [Tooltip("Quantos ataques o objeto aguenta antes de quebrar/ativar.")]
    [SerializeField] private int vidaMaxima = 1;

    [Tooltip("Qual tipo de ataque pode danificar este objeto.")]
    [SerializeField] private TipoDeAtaqueAceito tipoDeAtaqueAceito = TipoDeAtaqueAceito.Ambos;

    [Header("Feedback Visual e Sonoro")]
    [Tooltip("A cor que o objeto ir� piscar ao receber dano.")]
    [SerializeField] private Color corDeDano = Color.red; // -- NOVO --

    [Tooltip("A dura��o em segundos do efeito de tremor e flash.")]
    [SerializeField] private float duracaoFeedback = 0.15f; // -- NOVO --

    [Tooltip("A intensidade do efeito de tremor.")]
    [SerializeField] private float intensidadeTremor = 0.1f; // -- NOVO --

    [SerializeField] private GameObject efeitoDeQuebraPrefab;
    [SerializeField] private AudioClip somDeDano;
    [SerializeField] private AudioClip somDeQuebra;
    private AudioSource audioSource;

    [Header("A��es")]
    [Tooltip("Se marcado, o objeto ser� destru�do quando a vida chegar a zero.")]
    [SerializeField] private bool destruirAoQuebrar = true;

    [Tooltip("A��es a serem executadas quando o objeto � quebrado/ativado.")]
    public UnityEvent aoQuebrar;

    private int vidaAtual;

    // -- IN�CIO DAS NOVAS VARI�VEIS INTERNAS --
    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private Vector3 posicaoInicial;
    private Coroutine feedbackCoroutine;
    // -- FIM DAS NOVAS VARI�VEIS INTERNAS --

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // -- NOVO -- Pega a refer�ncia do SpriteRenderer para podermos mudar a cor.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("ObjetoInterativo n�o encontrou um SpriteRenderer neste GameObject. O efeito de cor n�o funcionar�.", this);
        }
    }

    private void Start()
    {
        vidaAtual = vidaMaxima;

        // -- NOVO -- Guarda a posi��o e cor originais do objeto no in�cio.
        posicaoInicial = transform.position;
        if (spriteRenderer != null)
        {
            corOriginal = spriteRenderer.color;
        }
    }

    public void ReceberDano(TipoDeAtaqueAceito tipoDeAtaque)
    {
        bool podeReceberDano = tipoDeAtaqueAceito == TipoDeAtaqueAceito.Ambos || tipoDeAtaqueAceito == tipoDeAtaque;

        if (!podeReceberDano) return;

        vidaAtual--;

        if (somDeDano != null)
        {
            audioSource.PlayOneShot(somDeDano);
        }

        // -- IN�CIO DA L�GICA DE FEEDBACK --
        // Se j� existe um feedback rodando, pare ele primeiro para evitar sobreposi��o.
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            ResetarFeedbackVisual(); // Reseta a apar�ncia para o estado original
        }
        // Inicia a nova corrotina de feedback.
        feedbackCoroutine = StartCoroutine(FeedbackDeDanoCoroutine());
        // -- FIM DA L�GICA DE FEEDBACK --

        if (vidaAtual <= 0)
        {
            QuebrarObjeto();
        }
    }

    // -- IN�CIO DA NOVA CORROTINA --
    /// <summary>
    /// Executa o efeito visual de flash de cor e tremor por um curto per�odo.
    /// </summary>
    private IEnumerator FeedbackDeDanoCoroutine()
    {
        if (spriteRenderer != null) spriteRenderer.color = corDeDano;

        float tempoDecorrido = 0f;
        while (tempoDecorrido < duracaoFeedback)
        {
            float offsetX = Random.Range(-1f, 1f) * intensidadeTremor;
            float offsetY = Random.Range(-1f, 1f) * intensidadeTremor;
            transform.position = posicaoInicial + new Vector3(offsetX, offsetY, 0);

            tempoDecorrido += Time.deltaTime;
            yield return null; // Espera at� o pr�ximo frame.
        }

        // Garante que o objeto volte ao estado original ao final do efeito.
        ResetarFeedbackVisual();
        feedbackCoroutine = null; // Libera a refer�ncia da corrotina.
    }
    // -- FIM DA NOVA CORROTINA --

    // -- IN�CIO DA NOVA FUN��O AUXILIAR --
    /// <summary>
    /// Reseta a posi��o e a cor do objeto para seus valores originais.
    /// </summary>
    private void ResetarFeedbackVisual()
    {
        transform.position = posicaoInicial;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = corOriginal;
        }
    }
    // -- FIM DA NOVA FUN��O AUXILIAR --

    private void QuebrarObjeto()
    {
        // -- NOVO -- Garante que qualquer efeito de feedback seja interrompido antes de destruir o objeto.
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        if (somDeQuebra != null)
        {
            AudioSource.PlayClipAtPoint(somDeQuebra, transform.position);
        }

        if (efeitoDeQuebraPrefab != null)
        {
            Instantiate(efeitoDeQuebraPrefab, transform.position, Quaternion.identity);
        }

        aoQuebrar.Invoke();

        if (destruirAoQuebrar)
        {
            Destroy(gameObject);
        }
    }
}