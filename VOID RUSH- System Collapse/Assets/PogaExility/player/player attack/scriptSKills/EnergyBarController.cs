using UnityEngine;
using UnityEngine.UI;

public class EnergyBarController : MonoBehaviour
{
    [Header("Refer�ncia Visual")]
    [Tooltip("Arraste aqui a Imagem do 'Fill'.")]
    public Image fillImage;

    [Header("Configura��es de Energia")]
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
            Debug.LogError("ERRO CR�TICO: 'fillImage' n�o foi atribu�da no EnergyBarController!", this.gameObject);
            this.enabled = false;
            return;
        }
    }

    // A l�gica de regenera��o e atualiza��o visual agora est� toda no Update
    void Update()
    {
        // N�o faz nada at� ser inicializado pelo PlayerController
        if (!isInitialized) return;

        // 1. L�gica de Regenera��o
        regenDelayTimer += Time.deltaTime;
        if (regenDelayTimer >= regenDelay && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenPerSecond * Time.deltaTime;
            // Garante que n�o ultrapasse o m�ximo
            if (currentEnergy > maxEnergy)
            {
                currentEnergy = maxEnergy;
            }
        }

        // 2. L�gica Visual (SEMPRE ACONTECE)
        // Isso garante que o fill SEMPRE acompanha a energia.
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (maxEnergy > 0)
        {
            // Converte a energia (ex: 80 de 100) para um valor de preenchimento (0.8)
            // e aplica DIRETAMENTE, sem suaviza��o.
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

        // Reseta o timer do delay. A regenera��o para imediatamente.
        regenDelayTimer = 0f;

        // For�a uma atualiza��o visual imediata no mesmo frame do consumo
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