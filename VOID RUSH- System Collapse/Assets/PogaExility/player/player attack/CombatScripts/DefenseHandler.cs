using UnityEngine;
using System.Collections;

public class DefenseHandler : MonoBehaviour
{
    [Header("Configuração de Parry")]
    [Tooltip("A janela de tempo (em segundos) no início do block para conseguir um parry.")]
    public float parryWindow = 0.15f;

    private bool isBlocking = false;
    private bool canParry = false;
    private Coroutine blockCoroutine;

    private PlayerAnimatorController animatorController;

    void Awake()
    {
        animatorController = GetComponent<PlayerAnimatorController>();
    }

    public void StartBlock()
    {
        if (isBlocking) return;

        isBlocking = true;
        blockCoroutine = StartCoroutine(BlockRoutine());
        Debug.Log("Começou a bloquear.");
    }

    private IEnumerator BlockRoutine()
    {
        // Animação de Block
        animatorController.PlayState(PlayerAnimState.block);

        // Janela de Parry
        canParry = true;
        yield return new WaitForSeconds(parryWindow);
        canParry = false;

        // Mantém o estado de block enquanto o botão estiver pressionado
        while (isBlocking)
        {
            yield return null;
        }
    }

    public void EndBlock()
    {
        if (!isBlocking) return;

        isBlocking = false;
        // A corrotina vai terminar naturalmente. O jogador volta para a animação de "parado".
        // O PlayerController/AnimatorController cuidará da transição para a animação correta.
        Debug.Log("Parou de bloquear.");
    }

    // Esta função será chamada por um inimigo ou projétil quando atingir o jogador
    public void OnHitWhileDefending()
    {
        if (canParry)
        {
            // SUCESSO NO PARRY
            animatorController.PlayState(PlayerAnimState.parry);
            Debug.Log("PARCEIRO! Conseguiu o Parry!");
            // TODO: Aplicar efeito de parry (stun no inimigo, refletir projétil, etc.)

            // Termina o block imediatamente após o parry
            EndBlock();
        }
        else if (isBlocking)
        {
            // SUCESSO NO BLOCK
            Debug.Log("Bloqueio bem-sucedido.");
            // TODO: Reduzir stamina/barra de block, tocar efeito de faísca, etc.
        }
    }
}