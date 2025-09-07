// WeaponItemView.cs - VERS�O FINAL E CORRETA
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponItemView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI infoText;

    // Recebe a arma e a muni��o ATUAL se for a arma ativa.
    // Se currentAmmo for -1, significa que a arma est� inativa.
    public void Render(ItemSO weapon, int currentAmmo)
    {
        if (weapon == null)
        {
            // Esconde �cone e texto se n�o houver arma.
            icon.enabled = false;
            infoText.enabled = false;
            return;
        }

        // Mostra o �cone da arma.
        icon.enabled = true;
        icon.sprite = weapon.itemIcon;

        // L�gica de texto S� para armas Ranger.
        if (weapon.weaponType == WeaponType.Ranger)
        {
            infoText.enabled = true;
            if (currentAmmo >= 0) // Arma ativa
            {
                infoText.text = $"{currentAmmo}/{weapon.magazineSize}";
            }
            else // Arma inativa
            {
                infoText.text = $"--/{weapon.magazineSize}";
            }
        }
        else // Para armas Melee, esconde o texto de muni��o.
        {
            infoText.enabled = false;
        }
    }
}