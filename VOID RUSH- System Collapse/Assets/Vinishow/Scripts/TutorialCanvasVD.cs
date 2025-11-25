using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCanvasVD : MonoBehaviour
{
    [Header("Referências da UI")]
    [Tooltip("O painel principal que contém a imagem do tutorial.")]
    [SerializeField] private CanvasGroup painelTutorial;

    [Tooltip("O botão que o jogador usará para fechar o tutorial.")]
    [SerializeField] private Button botaoFechar;

    [Header("Configuração de Fade")]
    [Tooltip("A velocidade em segundos para o fade-in e fade-out.")]
    [SerializeField] private float tempoDeFade = 0.5f;

    // Controle para evitar que o tutorial seja ativado múltiplas vezes.
    private bool tutorialAtivo = false;
    private CanvasGroup canvasGroupBotao;

    void Awake()
    {
        // Garante que o Canvas comece invisível e desativado.
        painelTutorial.alpha = 0f;
        painelTutorial.interactable = false;
        painelTutorial.blocksRaycasts = false;

        // Pega o CanvasGroup do botão e o configura.
        canvasGroupBotao = botaoFechar.GetComponent<CanvasGroup>();
        if (canvasGroupBotao == null)
        {
            Debug.LogWarning("O botão de fechar não tem um componente CanvasGroup! Adicionando um.", botaoFechar);
            canvasGroupBotao = botaoFechar.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroupBotao.alpha = 0f;
        canvasGroupBotao.interactable = false;

        // Adiciona a função de fechar ao clique do botão.
        botaoFechar.onClick.AddListener(FecharTutorial);

        // Desativa o objeto do canvas para não atrapalhar no início.
        gameObject.SetActive(false);
    }

    // Esta é a função pública que o gatilho vai chamar.
    public void IniciarTutorial()
    {
        if (tutorialAtivo) return;

        tutorialAtivo = true;

        // Ativa o objeto do Canvas para que ele possa ser visto.
        gameObject.SetActive(true);

        // Inicia a sequência de animação.
        StartCoroutine(ExecutarSequenciaDeFade());
    }

    private IEnumerator ExecutarSequenciaDeFade()
    {
        // 1. Congela o tempo do jogo.
        Time.timeScale = 0f;

        // 2. Executa o fade-in do painel principal e espera ele terminar.
        yield return StartCoroutine(Fade(painelTutorial, 1f, tempoDeFade));

        // 3. Executa o fade-in do botão e espera ele terminar.
        yield return StartCoroutine(Fade(canvasGroupBotao, 1f, tempoDeFade));

        // 4. Torna o botão clicável.
        canvasGroupBotao.interactable = true;
    }

    private void FecharTutorial()
    {
        // Inicia a rotina para fechar e limpar tudo.
        StartCoroutine(FadeOutECleanup());
    }

    private IEnumerator FadeOutECleanup()
    {
        // Desativa a interatividade do botão imediatamente.
        canvasGroupBotao.interactable = false;

        // Executa o fade-out do painel principal (que também afetará o botão).
        yield return StartCoroutine(Fade(painelTutorial, 0f, tempoDeFade));

        // Descongela o tempo do jogo.
        Time.timeScale = 1f;

        // Desativa o objeto do Canvas.
        gameObject.SetActive(false);
        tutorialAtivo = false;
    }

    // Rotina genérica para fazer o fade de qualquer CanvasGroup.
    private IEnumerator Fade(CanvasGroup group, float alphaFinal, float duracao)
    {
        float tempoPassado = 0f;
        float alphaInicial = group.alpha;

        // Habilita a detecção de cliques no início do fade-in.
        if (alphaFinal > 0)
        {
            group.blocksRaycasts = true;
        }

        while (tempoPassado < duracao)
        {
            // Usamos Time.unscaledDeltaTime porque o tempo normal (deltaTime) está congelado!
            tempoPassado += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(alphaInicial, alphaFinal, tempoPassado / duracao);
            yield return null; // Espera o próximo frame.
        }

        group.alpha = alphaFinal; // Garante que o valor final seja exato.

        // Desabilita a detecção de cliques no final do fade-out.
        if (alphaFinal == 0)
        {
            group.blocksRaycasts = false;
        }
    }
}