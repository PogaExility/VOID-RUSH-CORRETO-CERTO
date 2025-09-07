// WeaponHUDView.cs - VERSÃO FINAL E CORRETA
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUDView : MonoBehaviour
{
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI ammoText;
    private WeaponHandler weaponHandler;

    void Start()
    {
        weaponHandler = WeaponHandler.Instance;
        if (weaponHandler != null)
        {
            // Se inscreve para ser atualizado sempre que a arma ativa mudar/atirar/recarregar.
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
        bool hasWeapon = activeWeaponSlot != null && activeWeaponSlot.item != null;

        weaponIcon.enabled = hasWeapon;
        ammoText.enabled = false; // Começa desativado por padrão.

        if (hasWeapon)
        {
            weaponIcon.sprite = activeWeaponSlot.item.itemIcon;

            // Pede a munição real da arma ativa.
            if (weaponHandler.TryGetActiveWeaponAmmo(out int current, out int max))
            {
                ammoText.enabled = true;
                ammoText.text = $"{current}/{max}";
            }
        }
    }
}