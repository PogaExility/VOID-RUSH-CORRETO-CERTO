// ARQUIVO: EnergyBarController.cs
using UnityEngine;
using UnityEngine.UI;

public class EnergyBarController : MonoBehaviour
{
    public Slider energySlider;
    private float maxEnergy;
    private float currentEnergy;

    // >> FUNÇÃO ADICIONADA <<
    /// <summary>
    /// Verifica se a energia atual é suficiente para cobrir um custo.
    /// </summary>
    /// <param name="amountToConsume">A quantidade de energia necessária.</param>
    /// <returns>True se tiver energia suficiente, False caso contrário.</returns>
    public bool HasEnoughEnergy(float amountToConsume)
    {
        return currentEnergy >= amountToConsume;
    }

    public void SetMaxEnergy(float max)
    {
        maxEnergy = max;
        currentEnergy = maxEnergy;
        UpdateUI();
    }

    public void ConsumeEnergy(float amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        UpdateUI();
    }

    public void RecoverEnergy(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (energySlider != null)
        {
            energySlider.value = currentEnergy / maxEnergy;
        }
    }
}