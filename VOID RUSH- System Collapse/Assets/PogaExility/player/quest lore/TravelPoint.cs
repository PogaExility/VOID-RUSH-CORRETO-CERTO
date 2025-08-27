using UnityEngine;

public class TravelPoint : MonoBehaviour
{
    [Header("Refer�ncias de UI")]
    [Tooltip("Arraste o Panel que funciona como o mapa de viagem.")]
    public GameObject travelMapPanel;

    // A fun��o de intera��o abre o mapa de viagem.
    public void Interact()
    {
        if (travelMapPanel != null)
        {
            travelMapPanel.SetActive(true);
        }
    }
}