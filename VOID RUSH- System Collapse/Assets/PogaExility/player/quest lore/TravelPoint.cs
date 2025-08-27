using UnityEngine;

public class TravelPoint : MonoBehaviour
{
    [Header("Referências de UI")]
    [Tooltip("Arraste o Panel que funciona como o mapa de viagem.")]
    public GameObject travelMapPanel;

    // A função de interação abre o mapa de viagem.
    public void Interact()
    {
        if (travelMapPanel != null)
        {
            travelMapPanel.SetActive(true);
        }
    }
}