using UnityEngine;
using System.Collections;

public class DefenseHandler : MonoBehaviour
{
    [Header("Configuração de Parry")]
    [Tooltip("A janela de tempo (em segundos) no início do block para conseguir um parry.")]
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
        yield return new WaitForSeconds(blockSkill.block_ParryWindow); // Usa o parâmetro do SkillSO
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
            // TODO: Reduzir stamina/barra de block, tocar efeito de faísca, etc.
        }
    }
}