// NOME DO ARQUIVO: PortaController.cs

using UnityEngine;
using System.Collections;

// Enum para escolher o comportamento da porta no Inspector da Unity.
public enum ModoDeAbertura
{
    Mover,      // A porta irá deslizar para uma nova posição.
    Animar,     // A porta irá tocar uma animação.
    Desativar   // A porta irá simplesmente desaparecer.
}

[RequireComponent(typeof(AudioSource))] // Garante que sempre haverá um AudioSource.
public class PortaController : MonoBehaviour
{
    [Header("Configuração Geral")]
    [Tooltip("Define como a porta deve se comportar ao ser aberta.")]
    [SerializeField] private ModoDeAbertura modoDeAbertura = ModoDeAbertura.Mover;

    [Tooltip("Som que tocará quando a porta abrir.")]
    [SerializeField] private AudioClip somDeAbertura;

    // --- Variáveis para o Modo 'Mover' ---
    [Header("Opções para o Modo 'Mover'")]
    [Tooltip("O quanto a porta vai se mover a partir de sua posição inicial. Ex: (0, 5, 0) para subir 5 unidades.")]
    [SerializeField] private Vector3 deslocamentoAoAbrir = new Vector3(0, 5f, 0);

    [Tooltip("Quanto tempo, em segundos, a porta levará para completar o movimento.")]
    [SerializeField] private float duracaoDoMovimento = 2f;

    // --- Variáveis para o Modo 'Animar' ---
    [Header("Opções para o Modo 'Animar'")]
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

        // Se estiver no modo de Animação, tenta pegar o componente Animator.
        if (modoDeAbertura == ModoDeAbertura.Animar)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Modo 'Animar' selecionado, mas não há um componente Animator na porta!", this);
            }
        }
    }

    private void Start()
    {
        // Guarda as posições para o modo de movimento.
        posicaoInicial = transform.position;
        posicaoFinal = posicaoInicial + deslocamentoAoAbrir;
    }

    /// <summary>
    /// Esta é a função pública que será chamada pelo UnityEvent do ObjetoInterativo.
    /// </summary>
    public void AbrirPorta()
    {
        // Se a porta já estiver aberta, não faz mais nada.
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

        // Executa a ação correspondente ao modo selecionado.
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
    /// Corrotina que move a porta suavemente da posição inicial para a final.
    /// </summary>
    private IEnumerator MoverPortaCoroutine()
    {
        float tempoDecorrido = 0;

        while (tempoDecorrido < duracaoDoMovimento)
        {
            // Interpola a posição da porta ao longo do tempo.
            transform.position = Vector3.Lerp(posicaoInicial, posicaoFinal, tempoDecorrido / duracaoDoMovimento);
            tempoDecorrido += Time.deltaTime;
            yield return null; // Espera o próximo frame.
        }

        // Garante que a porta termine exatamente na posição final.
        transform.position = posicaoFinal;
    }
}