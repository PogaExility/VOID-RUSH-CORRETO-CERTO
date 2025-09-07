using System.Collections.Generic;
using UnityEngine;

// Este script gerencia a atualiza��o visual dos slots de muni��o.
public class AmmoHUDController : MonoBehaviour
{
    [Header("Refer�ncias")]
    [SerializeField] private WeaponHandler weaponHandler;

    // Arraste aqui os 4 objetos de ItemView que ficam DENTRO dos seus slots de muni��o.
    [SerializeField] private List<ItemView> ammoSlotViews;

    void Start()
    {
        if (weaponHandler == null)
        {
            weaponHandler = WeaponHandler.Instance;
        }

        // Inscreve-se nos eventos para saber quando redesenhar
        weaponHandler.OnAmmoSlotsChanged += Redraw;
        InventoryManager.Instance.OnInventoryChanged += Redraw; // Para redesenhar ao pegar/soltar

        // Desenho inicial
        Redraw();
    }

    private void OnDestroy()
    {
        if (weaponHandler != null)
        {
            weaponHandler.OnAmmoSlotsChanged -= Redraw;
        }
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= Redraw;
        }
    }

    private void Redraw()
    {
        for (int i = 0; i < ammoSlotViews.Count; i++)
        {
            if (i < 4) // Garante que n�o vamos passar do limite
            {
                InventorySlot slotData = weaponHandler.GetAmmoSlot(i);
                ItemView view = ammoSlotViews[i];

                if (slotData != null && slotData.item != null)
                {
                    view.gameObject.SetActive(true);
                    view.Render(slotData.item, slotData.count);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }
    }
}