using UnityEngine;
using UnityEngine.EventSystems;

// OUVE O CLIQUE, N�O O ARRASTE
public class WeaponSlotView : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("O �ndice deste slot de arma (0, 1, ou 2)")]
    public int weaponSlotIndex;

    // QUANDO � CLICADO
    public void OnPointerDown(PointerEventData eventData)
    {
        // Pega uma refer�ncia para o chefe das armas
        var weaponHandler = FindFirstObjectByType<WeaponHandler>();
        if (weaponHandler == null) return;

        // Avisa o chefe para fazer a troca usando o item que est� "no mouse"
        weaponHandler.EquipItemFromMouse(this.weaponSlotIndex);
    }
}