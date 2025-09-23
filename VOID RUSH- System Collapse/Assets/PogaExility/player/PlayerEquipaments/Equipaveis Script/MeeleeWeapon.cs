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

    // DENTRO DE MeeleeWeapon.cs

    public override void Attack()
    {
        // Se o jogador n�o est� atacando E o cooldown do �ltimo ataque j� passou...
        if (!playerController.IsAttacking && timeSinceLastAttack > weaponData.attackCooldown)
        {
            attackBuffered = false;
            StartAttack();
        }
        // ...sen�o, bufferiza o pr�ximo clique para continuar o combo.
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

        // --- L�GICA DE CORTE MODIFICADA ---
        if (currentSlashInstance != null) Destroy(currentSlashInstance);
        if (currentStep.slashEffectPrefab != null && attackPoint != null)
        {
            // 1. Instancia o corte como FILHO do attackPoint.
            currentSlashInstance = Instantiate(currentStep.slashEffectPrefab, attackPoint);

            // 2. Inicializa o corte com a nova dire��o de knockback.
            SlashEffect slashScript = currentSlashInstance.GetComponent<SlashEffect>();
            if (slashScript != null)
            {
                // Pega a dire��o do knockback do SO e a converte em um vetor, j� considerando o flip do jogador.
                Vector2 knockbackDirection = GetKnockbackDirectionVector(currentStep.knockbackDirection);

                // Passa o vetor de dire��o pr�-calculado para o efeito de corte.
                // (Isto causar� um erro at� atualizarmos o SlashEffect.cs)
                slashScript.Initialize(currentStep.damage, currentStep.knockbackPower, knockbackDirection, currentStep.slashAnimationState);
                slashScript.SetSpeed(speedMultiplier);
            }
        }
        // --- FIM DA L�GICA DE CORTE ---

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
    /// <summary>
    /// Converte a dire��o de knockback do Enum em um vetor Vector2,
    /// ajustando para a dire��o em que o jogador est� virado.
    /// </summary>
    /// <param name="direction">A dire��o definida no ComboStepData.</param>
    /// <returns>Um vetor de dire��o normalizado.</returns>
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
        // Se o movementScript N�O estiver virado para a direita (!IsFacingRight), inverta o knockback.
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
        FinishAttack(); // Chama a fun��o de limpeza
        comboCounter = 0; // Reseta o combo
        attackBuffered = false;
        timeSinceLastAttack = float.MaxValue;
    }
}