using UnityEngine;
using UnityEngine.UI;

public class EnergyBarController : MonoBehaviour
{
    [Header("Referência Visual")]
    [Tooltip("Arraste aqui a Imagem do 'Fill'. O script vai falhar se estiver vazio.")]
    public Image fillImage;

    [Header("Configurações de Energia")]
    public float maxEnergy = 100f;
    public float energyRegenPerSecond = 20f;
    public float regenDelay = 1.5f;

    // --- Controle Interno ---
    private float currentEnergy;
    private float regenDelayTimer;

    void Awake()
    {
        if (fillImage == null)
        {
            Debug.LogError("ERRO CRÍTICO: 'fillImage' não foi atribuída no EnergyBarController!", this.gameObject);
            this.enabled = false;
            return;
        }
    }

    void Update()
    {
        regenDelayTimer += Time.deltaTime;
        if (regenDelayTimer >= regenDelay && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenPerSecond * Time.deltaTime;
            if (currentEnergy > maxEnergy)
            {
                currentEnergy = maxEnergy;
            }
        }
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (maxEnergy > 0)
        {
            fillImage.fillAmount = currentEnergy / maxEnergy;
        }
    }

    public bool HasEnoughEnergy(float amountToConsume)
    {
        return currentEnergy >= amountToConsume;
    }

    public void ConsumeEnergy(float amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        regenDelayTimer = 0f;
    }

    public void SetMaxEnergy(float newMax)
    {
        maxEnergy = newMax;
        currentEnergy = maxEnergy;
        regenDelayTimer = regenDelay;
        UpdateVisuals();
    }

    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }
}