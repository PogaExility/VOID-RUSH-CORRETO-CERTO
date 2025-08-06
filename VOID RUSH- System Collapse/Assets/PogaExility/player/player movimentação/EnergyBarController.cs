using UnityEngine;
using UnityEngine.UI;

public class EnergyBarController : MonoBehaviour
{
    public Slider energySlider;
    private float maxEnergy;
    private float currentEnergy;

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
