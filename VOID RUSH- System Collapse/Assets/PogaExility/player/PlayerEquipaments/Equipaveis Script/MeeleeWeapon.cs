// NOME DO ARQUIVO: MeeleeWeapon.cs - VERSÃO REVISADA E CORRIGIDA

using UnityEngine;
using System.Collections;

public class MeeleeWeapon : WeaponBase
{
    // --- Referências ---
    private PlayerController playerController;
    private PlayerAnimatorController animatorController;

    // --- Estado do Combo ---
    private int comboCounter = 0; // Usaremos um nome mais claro que comboStep
    private float timeSinceLastAttack = float.MaxValue; // Começa alto para permitir o primeiro ataque
    private bool attackRequested = false; // Flag para registrar o input do jogador
    private Coroutine attackCoroutine;
    private GameObject currentSlashInstance;

    public void InitializeMeelee(PlayerController pc, PlayerAnimatorController animCtrl)
    {
        this.playerController = pc;
        this.animatorController = animCtrl;
    }

    private void Update()
    {
        // GUARDA DE INICIALIZAÇÃO:
        // Se o script ainda não foi inicializado pelo WeaponHandler, não faz nada.
        if (playerController == null)
        {
            return;
        }

        // O Update agora só gerencia o tempo para resetar o combo.
        if (!playerController.IsAttacking)
        {
            timeSinceLastAttack += Time.deltaTime;
        }

        // Se o tempo de reset foi excedido, volta para o primeiro golpe.
        if (timeSinceLastAttack > weaponData.comboResetTime)
        {
            comboCounter = 0;
        }
    }

    // A função Attack agora só registra a intenção de atacar.
    public override void Attack()
    {
        attackRequested = true;
    }

    // Usaremos FixedUpdate para a lógica de combate para evitar race conditions com o input.
    void FixedUpdate()
    {
        // Se o jogador não pediu um ataque, não faz nada.
        if (!attackRequested)
        {
            return;
        }

        attackRequested = false; // Consome o pedido de ataque

        // Se o jogador já está no meio de um ataque, não permite iniciar outro.
        if (playerController.IsAttacking)
        {
            return;
        }

        // --- LÓGICA PRINCIPAL DO ATAQUE ---

        // Pega o "pacote" de dados para o golpe atual.
        ComboStepData currentStepData = weaponData.comboSteps[comboCounter];

        // Inicia a corrotina que controla todo o fluxo do ataque.
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackFlowCoroutine(currentStepData));
    }


    private IEnumerator AttackFlowCoroutine(ComboStepData currentStep)
    {
        // --- FASE 1: INÍCIO DO ATAQUE ---
        playerController.IsAttacking = true;
        timeSinceLastAttack = 0f; // Reseta o timer do combo

        // Pega as referências necessárias ANTES de qualquer coisa.
        Transform attackPoint = WeaponHandler.Instance.GetAttackPoint();
        bool isFacingRight = playerController.movementScript.IsFacingRight();

        // Comanda as ações do jogador.
        playerController.PerformLunge(currentStep.lungeDistance, weaponData.lungeDuration);
        animatorController.PlayState(AnimatorTarget.PlayerBody, currentStep.playerAnimationState);

        // --- FASE 2: EFEITO VISUAL ---
        if (currentSlashInstance != null) Destroy(currentSlashInstance);
        if (currentStep.slashEffectPrefab != null && attackPoint != null)
        {
            // Instancia o corte como FILHO do attackPoint
            currentSlashInstance = Instantiate(currentStep.slashEffectPrefab, attackPoint);

            // A lógica de flip não precisa mudar, pois ela manipula a escala local do objeto filho.
            if (!isFacingRight)
            {
                currentSlashInstance.transform.Rotate(0f, 180f, 0f);
            }

            SlashEffect slashScript = currentSlashInstance.GetComponent<SlashEffect>();
            if (slashScript != null)
            {
                slashScript.Initialize(currentStep.damage, currentStep.knockbackPower, currentStep.slashAnimationState);
            }
        }

        // --- FASE 3: DURAÇÃO E CONCLUSÃO ---
        if (currentStep.playerAnimationClip == null)
        {
            Debug.LogError("O campo 'Player Animation Clip' não foi configurado no ItemSO para este golpe!");
            playerController.IsAttacking = false; // Libera o jogador
            yield break;
        }

        // Espera a animação inteira terminar.
        yield return new WaitForSeconds(currentStep.playerAnimationClip.length);

        // Libera o jogador para a próxima ação.
        playerController.IsAttacking = false;

        // Prepara para o próximo golpe do combo.
        comboCounter++;
        if (comboCounter >= weaponData.comboSteps.Count)
        {
            comboCounter = 0; // Volta ao início
        }
    }

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
            playerController.IsAttacking = false;
        }

        comboCounter = 0;
        timeSinceLastAttack = float.MaxValue;
    }
}