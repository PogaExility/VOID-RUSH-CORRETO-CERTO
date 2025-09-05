using UnityEngine;
using UnityEngine.EventSystems;

// OUVE O CLIQUE, NÃO O ARRASTE
public class WeaponSlotView : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("O índice deste slot de arma (0, 1, ou 2)")]
    public int weaponSlotIndex;

    // QUANDO É CLICADO
    public void OnPointerDown(PointerEventData eventData)
    {
        // Pega uma referência para o chefe das armas
        var weaponHandler = FindFirstObjectByType<WeaponHandler>();
        if (weaponHandler == null) return;

        // Avisa o chefe para fazer a troca usando o item que está "no mouse"
        weaponHandler.EquipItemFromMouse(this.weaponSlotIndex);
    }
}