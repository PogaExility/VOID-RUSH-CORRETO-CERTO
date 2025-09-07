// ARQUIVO NOVO: AmmoSlotView.cs
using UnityEngine;
using UnityEngine.EventSystems;

// Este script � o "ouvido" do slot de muni��o.
// Ele implementa IPointerDownHandler para poder detectar cliques.
public class AmmoSlotView : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("O �ndice deste slot de muni��o (0 a 3). Configure no Inspector!")]
    public int ammoSlotIndex;

    // Esta fun��o � chamada AUTOMATICAMENTE pelo sistema de Eventos da Unity quando o slot � clicado.
    public void OnPointerDown(PointerEventData eventData)
    {
        // Se o WeaponHandler existir...
        if (WeaponHandler.Instance != null)
        {
            // ...ele avisa o WeaponHandler para fazer a troca, passando seu pr�prio �ndice.
            WeaponHandler.Instance.EquipAmmoFromMouse(this.ammoSlotIndex);
        }
    }
}