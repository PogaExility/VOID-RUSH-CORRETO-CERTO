using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCanvasVD : MonoBehaviour
{
    [Header("Referências da UI")]
    [Tooltip("O painel principal que contém a imagem e o botão do tutorial.")]
    [SerializeField] private CanvasGroup painelTutorial;

    [Tooltip("O botão que o jogador usará para fechar o tutorial.")]
    [SerializeField] private Button botaoFechar;

    [Header("Configuração de Fade")]
    [Tooltip("A velocidade em segundos para o fade-in e fade-out.")]
    [SerializeField] private float tempoDeFade = 0.5f;

    private bool tutorialAtivo = false;

    void Awake()
    {
        // Garante que o painel comece invisível e não interativo.
        painelTutorial.alpha = 0f;
        painelTutorial.interactable = false;
        painelTutorial.blocksRaycasts = false;

        // Desativa a interatividade do botão no início.
        botaoFechar.interactable = false;

        // Adiciona a função de fechar ao clique do botão.
        botaoFechar.onClick.AddListener(FecharTutorial);

        gameObject.SetActive(false);
    }

    public void IniciarTutorial()
    {
        if (tutorialAtivo) return;

        tutorialAtivo = true;
        gameObject.SetActive(true);
        StartCoroutine(ExecutarSequenciaDeFade());
    }

    // MODIFICADO: A sequência agora é muito mais simples.
    private IEnumerator ExecutarSequenciaDeFade()
    {
        // 1. Congela o tempo do jogo.
        Time.timeScale = 0f;

        // 2. Executa o fade-in do painel principal (que inclui o botão).
        yield return StartCoroutine(Fade(painelTutorial, 1f, tempoDeFade));

        // 3. Torna o painel e o botão interativos.
        painelTutorial.interactable = true;
        botaoFechar.interactable = true;
    }

    private void FecharTutorial()
    {
        StartCoroutine(FadeOutECleanup());
    }

    private IEnumerator FadeOutECleanup()
    {
        // Desativa a interatividade imediatamente.
        painelTutorial.interactable = false;
        botaoFechar.interactable = false;

        // Executa o fade-out do painel.
        yield return StartCoroutine(Fade(painelTutorial, 0f, tempoDeFade));

        // Descongela o tempo do jogo.
        Time.timeScale = 1f;

        gameObject.SetActive(false);
        tutorialAtivo = false;
    }

    // A rotina de Fade continua a mesma, pois já usa unscaledDeltaTime.
    private IEnumerator Fade(CanvasGroup group, float alphaFinal, float duracao)
    {
        float tempoPassado = 0f;
        float alphaInicial = group.alpha;

        if (alphaFinal > 0)
        {
            group.blocksRaycasts = true;
        }

        while (tempoPassado < duracao)
        {
            tempoPassado += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(alphaInicial, alphaFinal, tempoPassado / duracao);
            yield return null;
        }

        group.alpha = alphaFinal;

        if (alphaFinal == 0)
        {
            group.blocksRaycasts = false;
        }
    }
}