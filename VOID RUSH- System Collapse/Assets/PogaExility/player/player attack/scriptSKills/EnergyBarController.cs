using UnityEngine;
using UnityEngine.UI;

public class EnergyBarController : MonoBehaviour
{
    [Header("Referência Visual")]
    [Tooltip("Arraste aqui a Imagem do 'Fill'.")]
    public Image fillImage;

    [Header("Configurações de Energia")]
    public float maxEnergy = 100f;
    public float energyRegenPerSecond = 20f;
    public float regenDelay = 1.5f;

    // --- Controle Interno ---
    private float currentEnergy;
    private float regenDelayTimer;
    private bool isInitialized = false;

    void Awake()
    {
        if (fillImage == null)
        {
            Debug.LogError("ERRO CRÍTICO: 'fillImage' não foi atribuída no EnergyBarController!", this.gameObject);
            this.enabled = false;
            return;
        }
    }

    // A lógica de regeneração e atualização visual agora está toda no Update
    void Update()
    {
        // Não faz nada até ser inicializado pelo PlayerController
        if (!isInitialized) return;

        // 1. Lógica de Regeneração
        regenDelayTimer += Time.deltaTime;
        if (regenDelayTimer >= regenDelay && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenPerSecond * Time.deltaTime;
            // Garante que não ultrapasse o máximo
            if (currentEnergy > maxEnergy)
            {
                currentEnergy = maxEnergy;
            }
        }

        // 2. Lógica Visual (SEMPRE ACONTECE)
        // Isso garante que o fill SEMPRE acompanha a energia.
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (maxEnergy > 0)
        {
            // Converte a energia (ex: 80 de 100) para um valor de preenchimento (0.8)
            // e aplica DIRETAMENTE, sem suavização.
            float fillValue = currentEnergy / maxEnergy;
            fillImage.fillAmount = fillValue;
        }
    }

    public bool HasEnoughEnergy(float amountToConsume)
    {
        return currentEnergy >= amountToConsume;
    }

    public void ConsumeEnergy(float amount)
    {
        if (!isInitialized) return;

        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;

        // Reseta o timer do delay. A regeneração para imediatamente.
        regenDelayTimer = 0f;

        // Força uma atualização visual imediata no mesmo frame do consumo
        UpdateVisuals();
    }

    public void SetMaxEnergy(float newMax)
    {
        maxEnergy = newMax;
        currentEnergy = maxEnergy;
        regenDelayTimer = regenDelay; // Inicia pronto para regenerar
        isInitialized = true; // Marca como pronto para o Update funcionar
        UpdateVisuals(); // Define o visual inicial para 100%
    }

    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }
}