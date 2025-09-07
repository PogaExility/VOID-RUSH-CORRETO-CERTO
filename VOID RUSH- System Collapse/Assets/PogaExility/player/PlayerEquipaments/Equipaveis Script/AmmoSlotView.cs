// ARQUIVO NOVO: AmmoSlotView.cs
using UnityEngine;
using UnityEngine.EventSystems;

// Este script é o "ouvido" do slot de munição.
// Ele implementa IPointerDownHandler para poder detectar cliques.
public class AmmoSlotView : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("O índice deste slot de munição (0 a 3). Configure no Inspector!")]
    public int ammoSlotIndex;

    // Esta função é chamada AUTOMATICAMENTE pelo sistema de Eventos da Unity quando o slot é clicado.
    public void OnPointerDown(PointerEventData eventData)
    {
        // Se o WeaponHandler existir...
        if (WeaponHandler.Instance != null)
        {
            // ...ele avisa o WeaponHandler para fazer a troca, passando seu próprio índice.
            WeaponHandler.Instance.EquipAmmoFromMouse(this.ammoSlotIndex);
        }
    }
}