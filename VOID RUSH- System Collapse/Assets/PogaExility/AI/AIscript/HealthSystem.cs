using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AIController))]
public class HealthSystem : MonoBehaviour
{
    // Referências
    private AIController controller;
    private EnemySO enemyData;

    // Estado Interno
    private float currentHealth;
    private bool isInvincible = false; // Para evitar dano em frames consecutivos

    #region Inicialização

    /// <summary>
    /// Método de inicialização chamado pelo AIController.
    /// </summary>
    public void Initialize(AIController ownerController)
    {
        this.controller = ownerController;
        this.enemyData = ownerController.enemyData;
        this.currentHealth = enemyData.maxHealth;
    }

    #endregion

    #region Lógica de Dano e Knockback

    /// <summary>
    /// O ponto de entrada principal para causar dano a esta IA.
    /// </summary>
    public void TakeDamage(float baseDamage, Vector2 attackDirection, float incomingKnockbackPower)
    {
        if (currentHealth <= 0 || isInvincible) return;

        // --- CÁLCULO DE DANO ---
        float finalDamage = baseDamage - enemyData.defense;
        finalDamage = Mathf.Max(0, finalDamage); // Garante que o dano não seja negativo.
        currentHealth -= finalDamage;

        Debug.Log($"<color=orange>{gameObject.name} tomou {finalDamage} de dano. Vida restante: {currentHealth}</color>");

        // --- CÁLCULO E APLICAÇÃO DE KNOCKBACK ---
        float finalKnockback = incomingKnockbackPower - enemyData.knockbackResistance;
        if (finalKnockback > 0)
        {
            // Pede ao Controller para iniciar a rotina de knockback,
            // garantindo que a lógica de estados seja pausada corretamente.
            controller.TriggerKnockback(attackDirection, finalKnockback);
        }

        // --- VERIFICAÇÃO DE MORTE E FEEDBACK ---
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Notifica o cérebro que a morte ocorreu.
            controller.OnDeath();
        }
        else
        {
            // Inicia um curto período de invencibilidade para evitar dano massivo instantâneo.
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.2f); // Tempo curto de "iframes"
        isInvincible = false;
    }

    #endregion
}