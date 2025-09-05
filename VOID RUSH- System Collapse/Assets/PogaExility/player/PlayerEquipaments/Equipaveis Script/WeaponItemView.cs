using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponItemView : MonoBehaviour
{
    // ARRASTE AS REFERÊNCIAS DESTE PREFAB NO INSPECTOR
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI infoText; // Para munição, etc.

    public void Render(ItemSO weapon)
    {
        if (weapon == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        icon.sprite = weapon.itemIcon;

        // Lógica para mostrar a informação certa
        switch (weapon.weaponType)
        {
            case WeaponType.Ranger:
                infoText.text = $"MUNIÇÃO: --/{weapon.magazineSize}";
                break;
            default:
                infoText.text = "";
                break;
        }
    }
}