// NOME DO ARQUIVO: MeeleeWeapon.cs

using UnityEngine;
using System.Collections;

public class MeeleeWeapon : WeaponBase
{
    // --- Refer�ncias Injetadas ---
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
    /// Fun��o de inicializa��o espec�fica para armas Meelee, chamada pelo WeaponHandler.
    /// </summary>
    public void InitializeMeelee(PlayerController pc, PlayerAnimatorController animCtrl)
    {
        this.playerController = pc;
        this.animatorController = animCtrl;
    }

    private void Update()
    {
        // GUARDA DE INICIALIZA��O:
        // Se o PlayerController ainda n�o foi fornecido pelo WeaponHandler,
        // n�o executa nenhuma l�gica de Update para evitar erros.
        if (playerController == null)
        {
            return;
        }

        // Se o jogador n�o est� atacando e passou tempo suficiente desde o �ltimo golpe, reseta o combo.
        if (!playerController.IsAttacking && Time.time > lastAttackTime + weaponData.comboResetTime)
        {
            comboStep = 0;
        }
    }

    public override void Attack()
    {
        // Pega o AttackPoint diretamente do WeaponHandler no momento do ataque.
        Transform attackPoint = WeaponHandler.Instance.GetAttackPoint();
        if (attackPoint == null)
        {
            Debug.LogError("MeeleeWeapon n�o conseguiu encontrar o AttackPoint atrav�s do WeaponHandler.");
            return;
        }

        // --- A L�GICA DO COMBO CONTINUA EXATAMENTE A MESMA DAQUI PARA BAIXO ---

        // Valida��es Iniciais
        if (weaponData.comboSteps == null || weaponData.comboSteps.Count == 0)
        {
            Debug.LogError($"A arma '{weaponData.itemName}' n�o tem nenhum combo configurado no ItemSO!");
            return;
        }

        // Determina o passo do combo
        if (canContinueCombo && comboStep < weaponData.comboSteps.Count) { }
        else { comboStep = 0; }

        ComboStepData currentStepData = weaponData.comboSteps[comboStep];

        // Orquestra��o
        playerController.IsAttacking = true;
        lastAttackTime = Time.time;
        canContinueCombo = false;

        playerController.PerformLunge(currentStepData.lungeDistance, weaponData.lungeDuration);
        animatorController.PlayState(AnimatorTarget.PlayerBody, currentStepData.comboBodyAnimation);

        if (currentSlashInstance != null)
        {
            Destroy(currentSlashInstance);
        }
        if (currentStepData.slashEffectPrefab != null)
        {
            currentSlashInstance = Instantiate(currentStepData.slashEffectPrefab, attackPoint.position, attackPoint.rotation);

            if (!playerController.movementScript.IsFacingRight())
            {
                currentSlashInstance.transform.localScale = new Vector3(
                    -currentSlashInstance.transform.localScale.x,
                    currentSlashInstance.transform.localScale.y,
                    currentSlashInstance.transform.localScale.z);
            }

            SlashEffect slashScript = currentSlashInstance.GetComponent<SlashEffect>();
            if (slashScript != null)
            {
                slashScript.Initialize(currentStepData.damage, currentStepData.knockbackPower, currentStepData.slashAnimation);
            }
        }

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        if (this.isActiveAndEnabled)
        {
            attackCoroutine = StartCoroutine(AttackFlowCoroutine(currentStepData));
        }

        comboStep++;
    }


    private IEnumerator AttackFlowCoroutine(ComboStepData currentStep)
    {
        // Espera pela janela de tempo para poder continuar o combo
        yield return new WaitForSeconds(currentStep.comboWindow);
        canContinueCombo = true;

        // Espera o resto da anima��o do jogador terminar
        float remainingTime = currentStep.comboBodyAnimation.length - currentStep.comboWindow;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // Se o combo n�o foi continuado, finaliza o estado de ataque.
        if (canContinueCombo)
        {
            FinishAttack();
        }
    }

    /// <summary>
    /// Limpa o estado de ataque do jogador.
    /// </summary>
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