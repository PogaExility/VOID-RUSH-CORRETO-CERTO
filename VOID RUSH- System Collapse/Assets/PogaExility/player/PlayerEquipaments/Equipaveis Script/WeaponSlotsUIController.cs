// WeaponSlotsUIController.cs - C�REBRO DA HOTBAR
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlotsUIController : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("Arraste aqui os 3 objetos FILHOS que cont�m o script ItemView dos seus slots de arma.")]
    [SerializeField] private List<ItemView> weaponItemViews;

    private WeaponHandler weaponHandler;

    void Start()
    {
        weaponHandler = WeaponHandler.Instance;
        if (weaponHandler == null)
        {
            Debug.LogError("WeaponSlotsUIController n�o encontrou o WeaponHandler! Verifique a Ordem de Execu��o.", this);
            return;
        }

        // Se inscreve nos eventos para saber quando redesenhar.
        weaponHandler.OnWeaponSlotsChanged += Redraw;
        // Tamb�m redesenha quando a arma ativa muda (para atualizar a muni��o).
        weaponHandler.OnActiveWeaponChanged += (index) => Redraw();

        Redraw(); // Desenho inicial
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
            ItemView view = weaponItemViews[i];
            if (view == null) continue; // Pula se um slot n�o foi configurado.

            InventorySlot slotData = weaponHandler.GetWeaponSlot(i);

            // A l�gica de Render do ItemView j� sabe como se desenhar.
            // Aqui, passamos os dados corretos: o item e a quantidade (que ser� 1 para armas).
            view.Render(slotData.item, slotData.count);
        }
    }
}