// NOME DO ARQUIVO: PuzzleSequenciaController.cs

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class PuzzleSequenciaController : MonoBehaviour
{
    [Header("Configura��o da Sequ�ncia")]
    [Tooltip("Arraste aqui, NA ORDEM CORRETA, todos os objetos interativos que fazem parte do puzzle.")]
    [SerializeField] private List<ObjetoInterativo> sequenciaDeObjetos;

    [Header("Configura��o do Temporizador")]
    [Tooltip("Marque se este puzzle tem um limite de tempo.")]
    [SerializeField] private bool usarTimer = false;

    [Tooltip("Tempo em segundos para completar a sequ�ncia ap�s ativar o primeiro item.")]
    [SerializeField] private float tempoLimite = 10f;

    [Header("A��es do Puzzle")]
    [Tooltip("A��es a serem executadas quando a sequ�ncia � completada com sucesso.")]
    public UnityEvent aoCompletarPuzzle;

    [Tooltip("A��es a serem executadas quando a sequ�ncia falha (tempo esgota ou ordem errada).")]
    public UnityEvent aoFalharPuzzle;

    // --- Controle de Estado Interno ---
    private int indiceAtualDaSequencia = 0;
    private Coroutine timerCoroutine;
    private bool puzzleAtivo = false;

    /// <summary>
    /// Esta fun��o p�blica deve ser chamada pelo UnityEvent 'aoQuebrar' de CADA ObjetoInterativo da sequ�ncia.
    /// � assim que o objeto avisa ao c�rebro do puzzle que foi ativado.
    /// </summary>
    /// <param name="objetoAtivado">A refer�ncia do pr�prio objeto que foi ativado.</param>
    public void NotificarAtivacao(ObjetoInterativo objetoAtivado)
    {
        // Se o puzzle j� foi conclu�do ou o objeto recebido n�o faz parte da sequ�ncia, ignora.
        if (indiceAtualDaSequencia >= sequenciaDeObjetos.Count || !sequenciaDeObjetos.Contains(objetoAtivado))
        {
            return;
        }

        // Verifica se o objeto ativado � o esperado na sequ�ncia.
        if (sequenciaDeObjetos[indiceAtualDaSequencia] == objetoAtivado)
        {
            // Se for o primeiro objeto da sequ�ncia, inicia o puzzle.
            if (!puzzleAtivo)
            {
                IniciarPuzzle();
            }

            // Avan�a para o pr�ximo passo da sequ�ncia.
            indiceAtualDaSequencia++;
            Debug.Log($"Acertou o item {indiceAtualDaSequencia} de {sequenciaDeObjetos.Count}!");

            // Verifica se a sequ�ncia foi completada.
            if (indiceAtualDaSequencia >= sequenciaDeObjetos.Count)
            {
                CompletarPuzzle();
            }
        }
        else
        {
            // O jogador ativou um objeto fora de ordem.
            Debug.Log("Ordem errada! Resetando o puzzle.");
            FalharPuzzle();
        }
    }

    private void IniciarPuzzle()
    {
        puzzleAtivo = true;
        indiceAtualDaSequencia = 0; // Garante que come�a do zero.
        Debug.Log("Puzzle iniciado!");

        // Se usar timer, inicia a contagem regressiva.
        if (usarTimer)
        {
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(TimerCoroutine());
        }
    }

    private void CompletarPuzzle()
    {
        Debug.Log("Puzzle Completo!");
        puzzleAtivo = false;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        aoCompletarPuzzle.Invoke();
    }

    private void FalharPuzzle()
    {
        puzzleAtivo = false;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        aoFalharPuzzle.Invoke();

        // Reseta o progresso para a pr�xima tentativa.
        indiceAtualDaSequencia = 0;
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(tempoLimite);

        // Se o puzzle ainda estiver ativo ap�s o tempo, significa que o jogador falhou.
        if (puzzleAtivo)
        {
            Debug.Log("Tempo esgotado! Resetando o puzzle.");
            FalharPuzzle();
        }
    }
}