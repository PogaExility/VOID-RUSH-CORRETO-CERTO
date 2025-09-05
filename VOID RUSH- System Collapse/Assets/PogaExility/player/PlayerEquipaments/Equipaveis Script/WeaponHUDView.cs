using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUDView : MonoBehaviour
{
    [SerializeField] private WeaponHandler weaponHandler;
    public Image weaponIcon;
    public TextMeshProUGUI ammoText;

    void Start()
    {
        if (weaponHandler == null) weaponHandler = FindFirstObjectByType<WeaponHandler>();
        if (weaponHandler != null)
        {
            weaponHandler.OnActiveWeaponChanged += UpdateView;
            UpdateView(weaponHandler.currentWeaponIndex);
        }
    }

    private void OnDestroy()
    {
        if (weaponHandler != null)
            weaponHandler.OnActiveWeaponChanged -= UpdateView;
    }

    void UpdateView(int activeIndex)
    {
        var activeWeaponSlot = weaponHandler.GetActiveWeaponSlot();
        if (activeWeaponSlot != null && activeWeaponSlot.item != null)
        {
            weaponIcon.enabled = true;
            weaponIcon.sprite = activeWeaponSlot.item.itemIcon;
            if (activeWeaponSlot.item.itemType == ItemType.Weapon)
            {
                ammoText.text = $"MUNIÇÃO: --/{activeWeaponSlot.item.magazineSize}";
            }
        }
        else
        {
            weaponIcon.enabled = false;
        }
    }
}