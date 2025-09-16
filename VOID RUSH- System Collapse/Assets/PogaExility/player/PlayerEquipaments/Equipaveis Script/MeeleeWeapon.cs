// NOME DO ARQUIVO: MeeleeWeapon.cs - VERS�O FINAL CORRIGIDA

using UnityEngine;
using System.Collections;

public class MeeleeWeapon : WeaponBase
{
    private PlayerController playerController;
    private PlayerAnimatorController animatorController;

    private int comboCounter = 0;
    private float timeSinceLastAttack = float.MaxValue;
    private bool attackBuffered = false; // Flag para registrar o input do jogador para o PR�XIMO ataque
    private Coroutine attackCoroutine;
    private GameObject currentSlashInstance;

    public void InitializeMeelee(PlayerController pc, PlayerAnimatorController animCtrl)
    {
        this.playerController = pc;
        this.animatorController = animCtrl; 
    }

    private void Update()
    {
        if (playerController == null) return;
        if (!playerController.IsAttacking)
        {
            timeSinceLastAttack += Time.deltaTime;
        }
        if (timeSinceLastAttack > weaponData.comboResetTime)
        {
            comboCounter = 0;
            attackBuffered = false; // Limpa o buffer se o combo resetar
        }
    }

    public override void Attack()
    {
        // Se o jogador n�o est� atacando, inicia o combo
        if (!playerController.IsAttacking)
        {
            // Consome o buffer e inicia o ataque
            attackBuffered = false;
            StartAttack();
        }
        // Se o jogador J� est� atacando, bufferiza o pr�ximo clique
        else
        {
            attackBuffered = true;
        }
    }

    private void StartAttack()
    {
        if (weaponData.comboSteps == null || weaponData.comboSteps.Count == 0) return;
        if (comboCounter >= weaponData.comboSteps.Count) comboCounter = 0;

        ComboStepData currentStepData = weaponData.comboSteps[comboCounter];
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackFlowCoroutine(currentStepData));
    }
    private IEnumerator AttackFlowCoroutine(ComboStepData currentStep)
    {
        playerController.IsAttacking = true;
        timeSinceLastAttack = 0f;

        float speedMultiplier = Mathf.Max(0.1f, currentStep.comboSpeed);
        animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, speedMultiplier);

        Transform attackPoint = WeaponHandler.Instance.GetAttackPoint();
        bool isFacingRight = playerController.movementScript.IsFacingRight();
        playerController.PerformLunge(currentStep.lungeDistance, weaponData.lungeDuration);
        animatorController.PlayState(AnimatorTarget.PlayerBody, currentStep.playerAnimationState);

        if (currentSlashInstance != null) Destroy(currentSlashInstance);
        if (currentStep.slashEffectPrefab != null && attackPoint != null)
        {
            // Instancia o corte na posi��o e rota��o GLOBAIS do AttackPoint, mas SEM parentesco.
            // Isso evita problemas de dupla rota��o ou escala.
            currentSlashInstance = Instantiate(currentStep.slashEffectPrefab, attackPoint);

            // Agora que o corte est� "solto" no mundo, sua rota��o � a correta, herdada
            // do attackPoint no momento da cria��o. N�o precisamos fazer mais nada para o flip.

            SlashEffect slashScript = currentSlashInstance.GetComponent<SlashEffect>();
            if (slashScript != null)
            {
                slashScript.Initialize(currentStep.damage, currentStep.knockbackPower, currentStep.slashAnimationState);
                slashScript.SetSpeed(speedMultiplier);
            }
        }

        if (currentStep.playerAnimationClip == null)
        {
            Debug.LogError("Player Animation Clip n�o configurado no SO para o golpe " + comboCounter);
            FinishAttack();
            yield break;
        }
        float animationDuration = currentStep.playerAnimationClip.length / speedMultiplier;
        yield return new WaitForSeconds(animationDuration);

        comboCounter++;
        FinishAttack();

        if (attackBuffered)
        {
            attackBuffered = false;
            StartAttack();
        }
    }

    private void FinishAttack()
    {
        playerController.IsAttacking = false;
        animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, 1f);
    }

    public void CancelAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (currentSlashInstance != null) Destroy(currentSlashInstance);
        if (playerController.IsAttacking)
        {
            playerController.CancelLunge();
        }
        FinishAttack(); // Chama a fun��o de limpeza
        comboCounter = 0; // Reseta o combo
        attackBuffered = false;
        timeSinceLastAttack = float.MaxValue;
    }
}