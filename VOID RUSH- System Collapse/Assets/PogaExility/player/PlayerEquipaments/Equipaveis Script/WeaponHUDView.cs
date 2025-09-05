using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUDView : MonoBehaviour
{
    [Header("REFERÊNCIAS DA UI")]
    public Image weaponIcon;
    public TextMeshProUGUI ammoText;
    public Slider cooldownSlider;

    [Header("REFERÊNCIAS DO JOGO")]
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

    // A FUNÇÃO COMPLETA E CORRETA
    void UpdateView(int activeIndex)
    {
        if (weaponIcon == null) return;

        var activeWeaponSlot = weaponHandler.GetActiveWeaponSlot();
        if (activeWeaponSlot != null && activeWeaponSlot.item != null)
        {
            weaponIcon.enabled = true;
            weaponIcon.sprite = activeWeaponSlot.item.itemIcon;

            // Garante que o texto de munição exista antes de usá-lo
            if (ammoText != null)
            {
                if (activeWeaponSlot.item.itemType == ItemType.Weapon && activeWeaponSlot.item.weaponType == WeaponType.Ranger)
                {
                    ammoText.enabled = true;
                    ammoText.text = $"MUNIÇÃO: --/{activeWeaponSlot.item.magazineSize}";
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