using UnityEngine;
using System.Collections;

public class DefenseHandler : MonoBehaviour
{
    [Header("Configura��o de Parry")]
    [Tooltip("A janela de tempo (em segundos) no in�cio do block para conseguir um parry.")]
    public float parryWindow = 0.15f;

    private bool isBlocking = false;
    private bool canParry = false;
    public bool IsBlocking() => isBlocking;
    public bool CanParry() => canParry;
    private Coroutine blockCoroutine;


    private PlayerAnimatorController animatorController;

    void Awake()
    {
        animatorController = GetComponent<PlayerAnimatorController>();
    }

    public void StartBlock(SkillSO blockSkill)
    {
        if (isBlocking || blockSkill == null || blockSkill.combatActionToPerform != CombatSkillType.Block) return;

        isBlocking = true;
        blockCoroutine = StartCoroutine(BlockRoutine(blockSkill));
    }

    private IEnumerator BlockRoutine(SkillSO blockSkill)
    {
        animatorController.PlayState(PlayerAnimState.block);
        canParry = true;
        yield return new WaitForSeconds(blockSkill.block_ParryWindow); // Usa o par�metro do SkillSO
        canParry = false;
        // Mant�m o estado de block enquanto o bot�o estiver pressionado
        while (isBlocking)
        {
            yield return null;
        }
    }

    public void EndBlock()
    {
        if (!isBlocking) return;

        isBlocking = false;
        // A corrotina vai terminar naturalmente. O jogador volta para a anima��o de "parado".
        // O PlayerController/AnimatorController cuidar� da transi��o para a anima��o correta.
        Debug.Log("Parou de bloquear.");
    }

    // Esta fun��o ser� chamada por um inimigo ou proj�til quando atingir o jogador
    public void OnHitWhileDefending(SkillSO parrySkill)
    {
        if (canParry)
        {
            animatorController.PlayState(PlayerAnimState.parry);
            Debug.Log($"PARRY! Stun por {parrySkill.parry_StunDuration}s, Dano x{parrySkill.parry_CounterDamageMultiplier}");
            EndBlock();
        }
        else if (isBlocking)
        {
            // SUCESSO NO BLOCK
            Debug.Log("Bloqueio bem-sucedido.");
            // TODO: Reduzir stamina/barra de block, tocar efeito de fa�sca, etc.
        }
    }
}