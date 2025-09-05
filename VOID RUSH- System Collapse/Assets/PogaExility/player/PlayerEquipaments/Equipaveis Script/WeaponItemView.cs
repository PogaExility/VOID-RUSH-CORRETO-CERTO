using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponItemView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI infoText;

    public void Render(ItemSO weapon)
    {
        if (weapon == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        icon.sprite = weapon.itemIcon;

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