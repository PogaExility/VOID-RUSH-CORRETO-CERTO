// ARQUIVO NOVO: WeaponSlotsUIController.cs
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlotsUIController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste aqui os 3 objetos FILHOS que contêm o script ItemView dos seus slots de arma.")]
    [SerializeField] private List<ItemView> weaponItemViews;

    private WeaponHandler weaponHandler;

    void Start()
    {
        weaponHandler = WeaponHandler.Instance;
        if (weaponHandler == null)
        {
            Debug.LogError("WeaponSlotsUIController não encontrou o WeaponHandler!");
            return;
        }

        // Inscreve-se nos eventos para saber quando redesenhar
        weaponHandler.OnWeaponSlotsChanged += Redraw;
        InventoryManager.Instance.OnInventoryChanged += Redraw; // Para redesenhar ao pegar/soltar

        Redraw(); // Desenho inicial
    }

    private void OnDestroy()
    {
        if (weaponHandler != null)
        {
            weaponHandler.OnWeaponSlotsChanged -= Redraw;
        }
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= Redraw;
        }
    }

    private void Redraw()
    {
        for (int i = 0; i < weaponItemViews.Count; i++)
        {
            InventorySlot slotData = weaponHandler.GetWeaponSlot(i);
            ItemView view = weaponItemViews[i];

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