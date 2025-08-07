using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refer�ncias")]
    public SkillRelease skillRelease;
    public AdvancedPlayerMovement2D movementScript;
    public PlayerAnimatorController animatorController;
    public EnergyBarController energyBar;
    public SkillManager skillManager;

    [Header("Skills Prim�rias")]
    [Tooltip("A skill que define o comportamento do pulo do jogador.")]
    public SkillSO jumpSkill;
    [Tooltip("A skill que define o comportamento do dash do jogador.")]
    public SkillSO dashSkill;

    [Header("Skills de Slot")]
    public SkillSO skillSlot1;
    public SkillSO skillSlot2;

    void Start()
    {
        // Valida��o de refer�ncias para evitar erros
        if (skillRelease == null || movementScript == null || animatorController == null || energyBar == null)
        {
            Debug.LogError("ERRO CR�TICO: Uma ou mais refer�ncias n�o foram atribu�das no PlayerController!", this.gameObject);
            this.enabled = false;
            return;
        }

        energyBar.SetMaxEnergy(100f);
        if (skillManager != null)
        {
            if (skillSlot1 != null) skillManager.AddSkill(skillSlot1);
            if (skillSlot2 != null) skillManager.AddSkill(skillSlot2);
        }
    }

    void Update()
    {
        // --- LEITURA DE INPUTS ---

        // Pulo � tratado como uma skill prim�ria
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryActivateSkill(jumpSkill);
        }

        // Checa a tecla de ativa��o definida no pr�prio ScriptableObject da skill
        if (dashSkill != null && Input.GetKeyDown(dashSkill.activationKey))
        {
            TryActivateSkill(dashSkill);
        }

        if (skillSlot1 != null && Input.GetKeyDown(skillSlot1.activationKey))
        {
            TryActivateSkill(skillSlot1);
        }

        if (skillSlot2 != null && Input.GetKeyDown(skillSlot2.activationKey))
        {
            TryActivateSkill(skillSlot2);
        }

        UpdateAnimations();
    }

    private void TryActivateSkill(SkillSO skillToUse)
    {
        if (skillToUse == null) return;

        // Verifica se tem energia suficiente ANTES de tentar ativar
        if (energyBar.HasEnoughEnergy(skillToUse.energyCost))
        {
            // O SkillRelease vai tentar executar a skill e retornar true se conseguir.
            // Isso � importante para o pulo n�o gastar energia se j� estiver no ar sem pulos extras.
            if (skillRelease.ActivateSkill(skillToUse, movementScript, animatorController))
            {
                // S� consome a energia se a ativa��o foi bem-sucedida
                energyBar.ConsumeEnergy(skillToUse.energyCost);
            }
        }
        else
        {
            Debug.Log("SEM ENERGIA para usar " + skillToUse.skillName);
        }
    }

    private void UpdateAnimations()
    {
        bool isWallSliding = movementScript.IsWallSliding();
        bool isFalling = movementScript.GetVerticalVelocity() < -0.1f && !movementScript.IsGrounded() && !isWallSliding;
        bool isRunning = movementScript.GetHorizontalInput() != 0 && movementScript.IsGrounded();
        bool isIdle = !isRunning && !isFalling && !isWallSliding && movementScript.IsGrounded();

        animatorController.UpdateAnimator(isIdle, isRunning, isFalling, isWallSliding);
    }
}