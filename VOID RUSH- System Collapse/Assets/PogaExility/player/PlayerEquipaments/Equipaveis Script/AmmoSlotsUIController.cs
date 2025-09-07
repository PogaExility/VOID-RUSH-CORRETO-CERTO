// ARQUIVO NOVO: AmmoSlotsUIController.cs
using System.Collections.Generic;
using UnityEngine;

public class AmmoSlotsUIController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste aqui os 4 objetos FILHOS que contêm o script ItemView dos seus slots de munição.")]
    [SerializeField] private List<ItemView> ammoItemViews;

    private WeaponHandler weaponHandler;

    void Start()
    {
        weaponHandler = WeaponHandler.Instance;
        if (weaponHandler == null)
        {
            Debug.LogError("AmmoSlotsUIController não encontrou o WeaponHandler!");
            return;
        }

        // Inscreve-se nos eventos para saber quando redesenhar
        weaponHandler.OnAmmoSlotsChanged += Redraw;
        InventoryManager.Instance.OnInventoryChanged += Redraw; // Para redesenhar ao pegar/soltar

        Redraw(); // Desenho inicial
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
        for (int i = 0; i < ammoItemViews.Count; i++)
        {
            InventorySlot slotData = weaponHandler.GetAmmoSlot(i);
            ItemView view = ammoItemViews[i];

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