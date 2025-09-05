using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponItemView : MonoBehaviour
{
    // ARRASTE AS REFER�NCIAS DESTE PREFAB NO INSPECTOR
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI infoText; // Para muni��o, etc.

    public void Render(ItemSO weapon)
    {
        if (weapon == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        icon.sprite = weapon.itemIcon;

        // L�gica para mostrar a informa��o certa
        switch (weapon.weaponType)
        {
            case WeaponType.Ranger:
                infoText.text = $"MUNI��O: --/{weapon.magazineSize}";
                break;
            default:
                infoText.text = "";
                break;
        }
    }
}