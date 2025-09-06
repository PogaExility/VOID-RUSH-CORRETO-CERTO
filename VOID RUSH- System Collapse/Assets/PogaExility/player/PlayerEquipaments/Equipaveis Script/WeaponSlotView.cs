using UnityEngine;
using UnityEngine.EventSystems;

// AGORA ELE OUVE CLIQUES, NÃO DROPS
public class WeaponSlotView : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("O índice deste slot de arma (0, 1, ou 2)")]
    public int weaponSlotIndex;

    // QUANDO É CLICADO
    public void OnPointerDown(PointerEventData eventData)
    {
        // Pega uma referência para o chefe das armas
        var weaponHandler = WeaponHandler.Instance;
        if (weaponHandler == null) return;

        // Pede para o chefe fazer a troca usando o item que está "no mouse"
        weaponHandler.EquipItemFromMouse(this.weaponSlotIndex);
    }
}