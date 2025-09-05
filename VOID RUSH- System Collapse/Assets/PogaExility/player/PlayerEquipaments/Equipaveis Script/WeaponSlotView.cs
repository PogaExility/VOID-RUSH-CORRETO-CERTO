using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSlotView : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("O índice deste slot de arma (0, 1, ou 2)")]
    public int weaponSlotIndex;

    private WeaponHandler weaponHandler;

    void Start()
    {
        // Pega a referência uma vez para não usar Find toda hora
        weaponHandler = FindFirstObjectByType<WeaponHandler>();
    }

    // A lógica agora é no CLIQUE, não no DROP
    public void OnPointerDown(PointerEventData eventData)
    {
        // Se não tem um WeaponHandler, não faz nada
        if (weaponHandler == null) return;

        // Pega o item que está "no mouse" no InventoryManager
        var itemNoMouse = InventoryManager.Instance.GetHeldItem();

        // Se o jogador está segurando um item com o mouse...
        if (itemNoMouse.item != null)
        {
            // ... Manda o WeaponHandler fazer a troca
            weaponHandler.EquipItemFromMouse(this.weaponSlotIndex);
        }
    }
}