// WeaponSlotsUIController.cs - VERS�O CORRETA E SIMPLIFICADA
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlotsUIController : MonoBehaviour
{
    [SerializeField] private List<WeaponItemView> weaponItemViews;
    private WeaponHandler weaponHandler;

    void Start()
    {
        weaponHandler = WeaponHandler.Instance;
        if (weaponHandler == null) return;

        // Se inscreve nos eventos corretos.
        weaponHandler.OnWeaponSlotsChanged += Redraw;
        weaponHandler.OnActiveWeaponChanged += (index) => Redraw();
        Redraw();
    }

    private void OnDestroy()
    {
        if (weaponHandler == null) return;
        weaponHandler.OnWeaponSlotsChanged -= Redraw;
        weaponHandler.OnActiveWeaponChanged -= (index) => Redraw();
    }

    private void Redraw()
    {
        for (int i = 0; i < weaponItemViews.Count; i++)
        {
            InventorySlot slotData = weaponHandler.GetWeaponSlot(i);
            WeaponItemView view = weaponItemViews[i];
            if (view == null) continue;

            // Se o slot n�o tem item, manda null para o Render esconder tudo.
            if (slotData == null || slotData.item == null)
            {
                view.Render(null, -1);
                continue;
            }

            // Se o slot (i) � o da arma ATIVA, pede a muni��o real.
            if (i == weaponHandler.currentWeaponIndex && weaponHandler.TryGetActiveWeaponAmmo(out int current, out int max))
            {
                view.Render(slotData.item, current);
            }
            else // Sen�o, � arma inativa. Manda -1.
            {
                view.Render(slotData.item, -1);
            }
        }
    }
}