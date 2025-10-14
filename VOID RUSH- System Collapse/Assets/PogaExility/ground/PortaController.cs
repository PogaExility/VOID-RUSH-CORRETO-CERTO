// NOME DO ARQUIVO: PortaController.cs

using UnityEngine;
using System.Collections;

public enum ModoDeAbertura
{
    Mover,
    Animar,
    Desativar // Lembre-se: Desativar = A porta some.
}

[RequireComponent(typeof(AudioSource))]
public class PortaController : MonoBehaviour
{
    [Header("Configuração Geral")]
    [SerializeField] private ModoDeAbertura modoDeAbertura = ModoDeAbertura.Mover;
    [SerializeField] private AudioClip somDeAbertura;
    [SerializeField] private AudioClip somDeFechamento; // --- NOVO ---

    [Header("Opções para o Modo 'Mover'")]
    [SerializeField] private Vector3 deslocamentoAoAbrir = new Vector3(0, 5f, 0);
    [SerializeField] private float duracaoDoMovimento = 2f;

    [Header("Opções para o Modo 'Animar'")]
    // --- ALTERADO --- Nome da variável para maior clareza.
    [SerializeField] private string nomeDoTriggerAnimacaoAbrir = "Abrir";
    [SerializeField] private string nomeDoTriggerAnimacaoFechar = "Fechar"; // --- NOVO ---

    // --- Componentes e Controle de Estado ---
    private AudioSource audioSource;
    private Animator animator;
    private bool estaAberta = false;
    private Vector3 posicaoInicial;
    private Vector3 posicaoFinal;
    private Coroutine moveCoroutine; // --- NOVO --- Para controlar o movimento em andamento.

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        if (modoDeAbertura == ModoDeAbertura.Animar && animator == null)
        {
            Debug.LogError("Modo 'Animar' selecionado, mas não há um componente Animator na porta!", this);
        }
    }

    private void Start()
    {
        posicaoInicial = transform.position;
        posicaoFinal = posicaoInicial + deslocamentoAoAbrir;
    }

    public void AbrirPorta()
    {
        if (estaAberta) return;
        estaAberta = true;

        if (somDeAbertura != null) audioSource.PlayOneShot(somDeAbertura);

        // Interrompe qualquer movimento anterior
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        switch (modoDeAbertura)
        {
            case ModoDeAbertura.Mover:
                // --- ALTERADO --- Usa a nova corrotina genérica.
                moveCoroutine = StartCoroutine(MoverCoroutine(transform.position, posicaoFinal));
                break;
            case ModoDeAbertura.Animar:
                if (animator != null) animator.SetTrigger(nomeDoTriggerAnimacaoAbrir);
                break;
            case ModoDeAbertura.Desativar:
                // Abrir, neste modo, significa desaparecer.
                gameObject.SetActive(false);
                break;
        }
    }

    // --- INÍCIO DA NOVA FUNÇÃO ---
    /// <summary>
    /// Fecha a porta, revertendo a ação de abertura.
    /// </summary>
    public void FecharPorta()
    {
        if (!estaAberta && gameObject.activeSelf) return; // Se já está fechada e visível, não faz nada.
        estaAberta = false;

        if (somDeFechamento != null) audioSource.PlayOneShot(somDeFechamento);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        switch (modoDeAbertura)
        {
            case ModoDeAbertura.Mover:
                moveCoroutine = StartCoroutine(MoverCoroutine(transform.position, posicaoInicial));
                break;
            case ModoDeAbertura.Animar:
                if (animator != null) animator.SetTrigger(nomeDoTriggerAnimacaoFechar);
                break;
            case ModoDeAbertura.Desativar:
                // Fechar, neste modo, significa reaparecer.
                gameObject.SetActive(true);
                break;
        }
    }
    // --- FIM DA NOVA FUNÇÃO ---

    // --- CORROTINA REATORADA ---
    /// <summary>
    /// Corrotina genérica que move o objeto de uma posição inicial para uma final.
    /// </summary>
    private IEnumerator MoverCoroutine(Vector3 startPos, Vector3 endPos)
    {
        float tempoDecorrido = 0;

        // Se a porta já está no destino, não faz nada.
        if (Vector3.Distance(startPos, endPos) < 0.01f)
        {
            moveCoroutine = null;
            yield break;
        }

        while (tempoDecorrido < duracaoDoMovimento)
        {
            transform.position = Vector3.Lerp(startPos, endPos, tempoDecorrido / duracaoDoMovimento);
            tempoDecorrido += Time.deltaTime;
            yield return null;

        }
        transform.position = endPos;
        moveCoroutine = null; // Libera a referência ao terminar.
    }
}