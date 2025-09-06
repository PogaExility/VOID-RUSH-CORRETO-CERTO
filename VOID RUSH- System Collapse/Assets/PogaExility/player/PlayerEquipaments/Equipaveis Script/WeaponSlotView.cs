using UnityEngine;
using UnityEngine.EventSystems;

// AGORA ELE OUVE CLIQUES, N�O DROPS
public class WeaponSlotView : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("O �ndice deste slot de arma (0, 1, ou 2)")]
    public int weaponSlotIndex;

    // QUANDO � CLICADO
    public void OnPointerDown(PointerEventData eventData)
    {
        // Pega uma refer�ncia para o chefe das armas
        var weaponHandler = WeaponHandler.Instance;
        if (weaponHandler == null) return;

        // Pede para o chefe fazer a troca usando o item que est� "no mouse"
        weaponHandler.EquipItemFromMouse(this.weaponSlotIndex);
    }
}