// NOME DO ARQUIVO: MeeleeWeapon.cs - VERSÃO FINAL CORRIGIDA

using UnityEngine;
using System.Collections;

public class MeeleeWeapon : WeaponBase
{
    private PlayerController playerController;
    private PlayerAnimatorController animatorController;
    private AudioSource playerAudioSource;

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

        playerAudioSource = playerController.GetComponent<AudioSource>();
        if (currentStep.slashSound != null && playerAudioSource != null)
        {
            playerAudioSource.PlayOneShot(currentStep.slashSound);
        }

        // --- MUDANÇA 1: REMOVIDO O COMANDO DE PARAR O PLAYER ---
        // playerController.movementScript.SetVelocity(0, playerController.movementScript.GetRigidbody().linearVelocity.y);

        float speedMultiplier = Mathf.Max(0.1f, currentStep.comboSpeed);
        animatorController.SetAnimatorSpeed(AnimatorTarget.PlayerBody, speedMultiplier);

        Transform attackPoint = WeaponHandler.Instance.GetAttackPoint();

        // --- MUDANÇA 2: (Opcional) DESATIVAR O LUNGE ---
        // Se você quer controle total de andar, o Lunge automático atrapalha.
        // Se quiser o impulso, descomente a linha abaixo.
        // playerController.PerformLunge(currentStep.lungeDistance, currentStep.lungeSpeed);

        animatorController.PlayState(AnimatorTarget.PlayerBody, currentStep.playerAnimationState);

        // --- LÓGICA DE CORTE ---
        if (currentSlashInstance != null) Destroy(currentSlashInstance);
        if (currentStep.slashEffectPrefab != null && attackPoint != null)
        {
            currentSlashInstance = Instantiate(currentStep.slashEffectPrefab, attackPoint);

            SlashEffect slashScript = currentSlashInstance.GetComponent<SlashEffect>();
            if (slashScript != null)
            {
                Vector2 knockbackDirection = GetKnockbackDirectionVector(currentStep.knockbackDirection);
                slashScript.Initialize(currentStep.damage, currentStep.knockbackPower, knockbackDirection, currentStep.slashAnimationState);
                slashScript.SetSpeed(speedMultiplier);
            }

            // Som do corte (Slash)
            if (currentStep.slashSound != null && playerAudioSource != null)
            {
                playerAudioSource.PlayOneShot(currentStep.slashSound);
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
    /// <summary>
    /// Converte a direção de knockback do Enum em um vetor Vector2,
    /// ajustando para a direção em que o jogador está virado.
    /// </summary>
    /// <param name="direction">A direção definida no ComboStepData.</param>
    /// <returns>Um vetor de direção normalizado.</returns>
    private Vector2 GetKnockbackDirectionVector(MeeleeKnockbackDirection direction)
    {
        Vector2 knockbackDir = Vector2.zero;

        switch (direction)
        {
            case MeeleeKnockbackDirection.Frente:
                knockbackDir = Vector2.right;
                break;
            case MeeleeKnockbackDirection.Cima:
                knockbackDir = Vector2.up;
                break;
            case MeeleeKnockbackDirection.CimaDiagonal:
                knockbackDir = new Vector2(1, 1);
                break;
            case MeeleeKnockbackDirection.Baixo:
                knockbackDir = Vector2.down;
                break;
            case MeeleeKnockbackDirection.BaixoDiagonal:
                knockbackDir = new Vector2(1, -1);
                break;
        }

        // A FORMA CORRETA E DIRETA:
        // Se o movementScript NÃO estiver virado para a direita (!IsFacingRight), inverta o knockback.
        if (!playerController.movementScript.IsFacingRight())
        {
            if (direction == MeeleeKnockbackDirection.Frente ||
                direction == MeeleeKnockbackDirection.CimaDiagonal ||
                direction == MeeleeKnockbackDirection.BaixoDiagonal)
            {
                knockbackDir.x *= -1;
            }
        }

        return knockbackDir.normalized;
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