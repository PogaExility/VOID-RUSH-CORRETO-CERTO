using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUDView : MonoBehaviour
{
    [Header("REFER�NCIAS DA UI")]
    public Image weaponIcon;
    public TextMeshProUGUI ammoText;
    public Slider cooldownSlider;

    [Header("REFER�NCIAS DO JOGO")]
    [SerializeField] private WeaponHandler weaponHandler;

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

    // A FUN��O COMPLETA E CORRETA
    void UpdateView(int activeIndex)
    {
        if (weaponIcon == null) return;

        var activeWeaponSlot = weaponHandler.GetActiveWeaponSlot();
        if (activeWeaponSlot != null && activeWeaponSlot.item != null)
        {
            weaponIcon.enabled = true;
            weaponIcon.sprite = activeWeaponSlot.item.itemIcon;

            // Garante que o texto de muni��o exista antes de us�-lo
            if (ammoText != null)
            {
                if (activeWeaponSlot.item.itemType == ItemType.Weapon && activeWeaponSlot.item.weaponType == WeaponType.Ranger)
                {
                    ammoText.enabled = true;
                    ammoText.text = $"MUNI��O: --/{activeWeaponSlot.item.magazineSize}";
                }
                else
                {
                    ammoText.enabled = false;
                }
            }
        }
        else
        {
            weaponIcon.enabled = false;
            if (ammoText != null) ammoText.enabled = false;
        }
    }
}