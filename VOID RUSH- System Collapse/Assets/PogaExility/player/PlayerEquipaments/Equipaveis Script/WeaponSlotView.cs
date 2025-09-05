using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSlotView : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("O �ndice deste slot de arma (0, 1, ou 2)")]
    public int weaponSlotIndex;

    private WeaponHandler weaponHandler;

    void Start()
    {
        // Pega a refer�ncia uma vez para n�o usar Find toda hora
        weaponHandler = FindFirstObjectByType<WeaponHandler>();
    }

    // A l�gica agora � no CLIQUE, n�o no DROP
    public void OnPointerDown(PointerEventData eventData)
    {
        // Se n�o tem um WeaponHandler, n�o faz nada
        if (weaponHandler == null) return;

        // Pega o item que est� "no mouse" no InventoryManager
        var itemNoMouse = InventoryManager.Instance.GetHeldItem();

        // Se o jogador est� segurando um item com o mouse...
        if (itemNoMouse.item != null)
        {
            // ... Manda o WeaponHandler fazer a troca
            weaponHandler.EquipItemFromMouse(this.weaponSlotIndex);
        }
    }
}