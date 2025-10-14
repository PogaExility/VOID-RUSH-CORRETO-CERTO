// NOME DO ARQUIVO: PuzzleSequenciaController.cs

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class PuzzleSequenciaController : MonoBehaviour
{
    [Header("Configuração da Sequência")]
    [Tooltip("Arraste aqui, NA ORDEM CORRETA, todos os objetos interativos que fazem parte do puzzle.")]
    [SerializeField] private List<ObjetoInterativo> sequenciaDeObjetos;

    [Header("Configuração do Temporizador")]
    [Tooltip("Marque se este puzzle tem um limite de tempo.")]
    [SerializeField] private bool usarTimer = false;

    [Tooltip("Tempo em segundos para completar a sequência após ativar o primeiro item.")]
    [SerializeField] private float tempoLimite = 10f;

    [Header("Ações do Puzzle")]
    [Tooltip("Ações a serem executadas quando a sequência é completada com sucesso.")]
    public UnityEvent aoCompletarPuzzle;

    [Tooltip("Ações a serem executadas quando a sequência falha (tempo esgota ou ordem errada).")]
    public UnityEvent aoFalharPuzzle;

    // --- Controle de Estado Interno ---
    private int indiceAtualDaSequencia = 0;
    private Coroutine timerCoroutine;
    private bool puzzleAtivo = false;

    /// <summary>
    /// Esta função pública deve ser chamada pelo UnityEvent 'aoQuebrar' de CADA ObjetoInterativo da sequência.
    /// É assim que o objeto avisa ao cérebro do puzzle que foi ativado.
    /// </summary>
    /// <param name="objetoAtivado">A referência do próprio objeto que foi ativado.</param>
    public void NotificarAtivacao(ObjetoInterativo objetoAtivado)
    {
        // Se o puzzle já foi concluído ou o objeto recebido não faz parte da sequência, ignora.
        if (indiceAtualDaSequencia >= sequenciaDeObjetos.Count || !sequenciaDeObjetos.Contains(objetoAtivado))
        {
            return;
        }

        // Verifica se o objeto ativado é o esperado na sequência.
        if (sequenciaDeObjetos[indiceAtualDaSequencia] == objetoAtivado)
        {
            // Se for o primeiro objeto da sequência, inicia o puzzle.
            if (!puzzleAtivo)
            {
                IniciarPuzzle();
            }

            // Avança para o próximo passo da sequência.
            indiceAtualDaSequencia++;
            Debug.Log($"Acertou o item {indiceAtualDaSequencia} de {sequenciaDeObjetos.Count}!");

            // Verifica se a sequência foi completada.
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
        indiceAtualDaSequencia = 0; // Garante que começa do zero.
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

        // Reseta o progresso para a próxima tentativa.
        indiceAtualDaSequencia = 0;
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(tempoLimite);

        // Se o puzzle ainda estiver ativo após o tempo, significa que o jogador falhou.
        if (puzzleAtivo)
        {
            Debug.Log("Tempo esgotado! Resetando o puzzle.");
            FalharPuzzle();
        }
    }
}