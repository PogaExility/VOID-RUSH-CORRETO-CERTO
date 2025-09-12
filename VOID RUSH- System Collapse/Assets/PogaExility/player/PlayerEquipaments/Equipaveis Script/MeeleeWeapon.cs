// NOME DO ARQUIVO: MeeleeWeapon.cs

using UnityEngine;
using System.Collections;

public class MeeleeWeapon : WeaponBase
{
    // --- Referências Injetadas ---
    private PlayerController playerController;
    private PlayerAnimatorController animatorController;
    private Transform attackPoint;

    // --- Estado do Combo ---
    private int comboStep = 0;
    private float lastAttackTime;
    private bool canContinueCombo = false;
    private Coroutine attackCoroutine;
    private GameObject currentSlashInstance;

    /// <summary>
    /// Função de inicialização específica para armas Meelee, chamada pelo WeaponHandler.
    /// </summary>
    public void InitializeMeelee(PlayerController pc, PlayerAnimatorController animCtrl)
    {
        this.playerController = pc;
        this.animatorController = animCtrl;
    }

    private void Update()
    {
        // GUARDA DE INICIALIZAÇÃO:
        // Se o PlayerController ainda não foi fornecido pelo WeaponHandler,
        // não executa nenhuma lógica de Update para evitar erros.
        if (playerController == null)
        {
            return;
        }

        // Se o jogador não está atacando e passou tempo suficiente desde o último golpe, reseta o combo.
        if (!playerController.IsAttacking && Time.time > lastAttackTime + weaponData.comboResetTime)
        {
            comboStep = 0;
        }
    }

    // DENTRO DE MeeleeWeapon.cs

    // DENTRO DE MeeleeWeapon.cs

    // DENTRO DE MeeleeWeapon.cs

    public override void Attack()
    {
        Transform attackPoint = WeaponHandler.Instance.GetAttackPoint();
        if (attackPoint == null) return;

        if (weaponData.comboSteps == null || weaponData.comboSteps.Count == 0)
        {
            Debug.LogError($"Arma '{weaponData.itemName}' sem combos configurados na lista 'comboSteps'!");
            return;
        }

        if (!canContinueCombo)
        {
            comboStep = 0;
        }

        // Pega o "pacote" de dados para o golpe atual
        ComboStepData currentStepData = weaponData.comboSteps[comboStep];

        playerController.IsAttacking = true;
        lastAttackTime = Time.time;
        canContinueCombo = false;

        // Executa os comandos lendo os dados do pacote "currentStepData"
        playerController.PerformLunge(currentStepData.lungeDistance, weaponData.lungeDuration);
        animatorController.PlayState(AnimatorTarget.PlayerBody, currentStepData.playerAnimationState);

        if (currentSlashInstance != null) Destroy(currentSlashInstance);

        if (currentStepData.slashEffectPrefab != null)
        {
            bool isFacingRight = playerController.movementScript.IsFacingRight();
            currentSlashInstance = Instantiate(currentStepData.slashEffectPrefab, attackPoint.position, attackPoint.rotation);

            // Lógica de flip corrigida
            if (!isFacingRight)
            {
                Vector3 localScale = currentSlashInstance.transform.localScale;
                localScale.x *= -1;
                currentSlashInstance.transform.localScale = localScale;
            }

            SlashEffect slashScript = currentSlashInstance.GetComponent<SlashEffect>();
            if (slashScript != null)
            {
                slashScript.Initialize(currentStepData.damage, currentStepData.knockbackPower, currentStepData.slashAnimationState);
            }
        }

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (this.isActiveAndEnabled)
        {
            // Passa o pacote de dados para a corrotina
            attackCoroutine = StartCoroutine(AttackFlowCoroutine(currentStepData));
        }

        // Avança para o próximo golpe do combo
        comboStep++;
        if (comboStep >= weaponData.comboSteps.Count)
        {
            comboStep = 0; // Volta ao início
        }
    }

    private IEnumerator AttackFlowCoroutine(ComboStepData currentStep)
    {
        // Espera pela janela de tempo definida no pacote de dados
        yield return new WaitForSeconds(currentStep.comboWindow);
        canContinueCombo = true;

        if (currentStep.playerAnimationClip == null)
        {
            Debug.LogError("O campo 'Player Animation Clip' não foi configurado no ItemSO para este golpe!");
            FinishAttack();
            yield break;
        }

        // Pega a duração do AnimationClip que está no pacote de dados
        float totalDuration = currentStep.playerAnimationClip.length;
        float remainingTime = totalDuration - currentStep.comboWindow;

        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        if (canContinueCombo)
        {
            FinishAttack();
        }
    }



    private void FinishAttack()
    {
        playerController.IsAttacking = false;
        canContinueCombo = false;
    }

    /// <summary>
    /// Interrompe e reseta completamente o estado de ataque. Chamado de fora.
    /// </summary>
    public void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        if (currentSlashInstance != null)
        {
            Destroy(currentSlashInstance);
        }

        if (playerController.IsAttacking)
        {
            playerController.CancelLunge();
            FinishAttack();
        }

        comboStep = 0;
    }
}