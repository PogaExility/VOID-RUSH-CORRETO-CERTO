// NOME DO ARQUIVO: PortaController.cs

using UnityEngine;
using System.Collections;

// Enum para escolher o comportamento da porta no Inspector da Unity.
public enum ModoDeAbertura
{
    Mover,      // A porta ir� deslizar para uma nova posi��o.
    Animar,     // A porta ir� tocar uma anima��o.
    Desativar   // A porta ir� simplesmente desaparecer.
}

[RequireComponent(typeof(AudioSource))] // Garante que sempre haver� um AudioSource.
public class PortaController : MonoBehaviour
{
    [Header("Configura��o Geral")]
    [Tooltip("Define como a porta deve se comportar ao ser aberta.")]
    [SerializeField] private ModoDeAbertura modoDeAbertura = ModoDeAbertura.Mover;

    [Tooltip("Som que tocar� quando a porta abrir.")]
    [SerializeField] private AudioClip somDeAbertura;

    // --- Vari�veis para o Modo 'Mover' ---
    [Header("Op��es para o Modo 'Mover'")]
    [Tooltip("O quanto a porta vai se mover a partir de sua posi��o inicial. Ex: (0, 5, 0) para subir 5 unidades.")]
    [SerializeField] private Vector3 deslocamentoAoAbrir = new Vector3(0, 5f, 0);

    [Tooltip("Quanto tempo, em segundos, a porta levar� para completar o movimento.")]
    [SerializeField] private float duracaoDoMovimento = 2f;

    // --- Vari�veis para o Modo 'Animar' ---
    [Header("Op��es para o Modo 'Animar'")]
    [Tooltip("O nome do 'Trigger' no Animator Controller da porta a ser ativado.")]
    [SerializeField] private string nomeDoTriggerAnimacao = "Abrir";

    // --- Componentes e Controle de Estado ---
    private AudioSource audioSource;
    private Animator animator;
    private bool estaAberta = false;
    private Vector3 posicaoInicial;
    private Vector3 posicaoFinal;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Se estiver no modo de Anima��o, tenta pegar o componente Animator.
        if (modoDeAbertura == ModoDeAbertura.Animar)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Modo 'Animar' selecionado, mas n�o h� um componente Animator na porta!", this);
            }
        }
    }

    private void Start()
    {
        // Guarda as posi��es para o modo de movimento.
        posicaoInicial = transform.position;
        posicaoFinal = posicaoInicial + deslocamentoAoAbrir;
    }

    /// <summary>
    /// Esta � a fun��o p�blica que ser� chamada pelo UnityEvent do ObjetoInterativo.
    /// </summary>
    public void AbrirPorta()
    {
        // Se a porta j� estiver aberta, n�o faz mais nada.
        if (estaAberta)
        {
            return;
        }
        estaAberta = true;

        // Toca o som de abertura, se houver um.
        if (somDeAbertura != null)
        {
            audioSource.PlayOneShot(somDeAbertura);
        }

        // Executa a a��o correspondente ao modo selecionado.
        switch (modoDeAbertura)
        {
            case ModoDeAbertura.Mover:
                StartCoroutine(MoverPortaCoroutine());
                break;
            case ModoDeAbertura.Animar:
                if (animator != null)
                {
                    animator.SetTrigger(nomeDoTriggerAnimacao);
                }
                break;
            case ModoDeAbertura.Desativar:
                gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Corrotina que move a porta suavemente da posi��o inicial para a final.
    /// </summary>
    private IEnumerator MoverPortaCoroutine()
    {
        float tempoDecorrido = 0;

        while (tempoDecorrido < duracaoDoMovimento)
        {
            // Interpola a posi��o da porta ao longo do tempo.
            transform.position = Vector3.Lerp(posicaoInicial, posicaoFinal, tempoDecorrido / duracaoDoMovimento);
            tempoDecorrido += Time.deltaTime;
            yield return null; // Espera o pr�ximo frame.
        }

        // Garante que a porta termine exatamente na posi��o final.
        transform.position = posicaoFinal;
    }
}