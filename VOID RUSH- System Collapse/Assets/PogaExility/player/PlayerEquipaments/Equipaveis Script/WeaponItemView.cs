// WeaponItemView.cs - VERSÃO FINAL E CORRETA
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponItemView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI infoText;

    // Recebe a arma e a munição ATUAL se for a arma ativa.
    // Se currentAmmo for -1, significa que a arma está inativa.
    public void Render(ItemSO weapon, int currentAmmo)
    {
        if (weapon == null)
        {
            // Esconde ícone e texto se não houver arma.
            icon.enabled = false;
            infoText.enabled = false;
            return;
        }

        // Mostra o ícone da arma.
        icon.enabled = true;
        icon.sprite = weapon.itemIcon;

        // Lógica de texto SÓ para armas Ranger.
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
        else // Para armas Melee, esconde o texto de munição.
        {
            infoText.enabled = false;
        }
    }
}