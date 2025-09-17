// NOME DO ARQUIVO: MeeleeWeapon.cs - VERSÃO FINAL CORRIGIDA

using UnityEngine;
using System.Collections;

public class MeeleeWeapon : WeaponBase
{
    private PlayerController playerController;
    private PlayerAnimatorController animatorController;

    private int comboCounter = 0;
    private float timeSinceLastAttack = float.MaxValue;
    private bool attackBuffered = false; // Flag para registrar o input do jogador para o PRÓXIMO ataque
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

    // DENTRO DE MeeleeWeapon.cs

    public override void Attack()
    {
        // Se o jogador não está atacando E o cooldown do último ataque já passou...
        if (!playerController.IsAttacking && timeSinceLastAttack > weaponData.attackCooldown)
        {
            attackBuffered = false;
            StartAttack();
        }
        // ...senão, bufferiza o próximo clique para continuar o combo.
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
    // DENTRO DE MeeleeWeapon.cs

    private IEnumerator AttackFlowCoroutine(ComboStepData currentStep)
    {
        playerController.IsAttacking = true;
        timeSinceLastAttack = 0f;
        attackBuffered = false;

        playerController.movementScript.SetVelocity(0, playerController.movementScript.GetRigidbody().linearVelocity.y);

        float speedMultiplier = Mathf.Max(0.1f, currentStep.comboSpeed);
        animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, speedMultiplier);

        Transform attackPoint = WeaponHandler.Instance.GetAttackPoint();

        playerController.PerformLunge(currentStep.lungeDistance, currentStep.lungeSpeed);
        animatorController.PlayState(AnimatorTarget.PlayerBody, currentStep.playerAnimationState);

        // --- LÓGICA DE CORTE FINAL E SIMPLIFICADA ---
        if (currentSlashInstance != null) Destroy(currentSlashInstance);
        if (currentStep.slashEffectPrefab != null && attackPoint != null)
        {
            // 1. Instancia o corte como FILHO do attackPoint.
            // A posição, rotação e escala serão herdadas automaticamente.
            currentSlashInstance = Instantiate(currentStep.slashEffectPrefab, attackPoint);

            // 2. Inicializa o corte. Nenhuma manipulação de transform é necessária.
            SlashEffect slashScript = currentSlashInstance.GetComponent<SlashEffect>();
            if (slashScript != null)
            {
                slashScript.Initialize(currentStep.damage, currentStep.knockbackPower, currentStep.slashAnimationState);
                slashScript.SetSpeed(speedMultiplier);
            }
        }
        // --- FIM DA LÓGICA DE CORTE ---

        if (currentStep.playerAnimationClip == null)
        {
            Debug.LogError("Player Animation Clip não configurado no SO para o golpe " + comboCounter);
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
        FinishAttack(); // Chama a função de limpeza
        comboCounter = 0; // Reseta o combo
        attackBuffered = false;
        timeSinceLastAttack = float.MaxValue;
    }
}