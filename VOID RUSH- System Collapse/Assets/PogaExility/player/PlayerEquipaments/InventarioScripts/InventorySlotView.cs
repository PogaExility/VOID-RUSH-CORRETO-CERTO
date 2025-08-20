using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InventorySlotView : MonoBehaviour
{
    private Image slotImage;

    [Header("Cores do Slot")]
    public Color emptyColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); // Cinza semi-transparente
    public Color occupiedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Cinza mais escuro

    void Awake()
    {
        slotImage = GetComponent<Image>();
        SetState(false); // Garante que o slot comece com a cor de "vazio"
    }

    // Define se o slot está ocupado ou vazio e muda a cor de acordo
    public void SetState(bool isOccupied)
    {
        if (slotImage != null)
        {
            slotImage.color = isOccupied ? occupiedColor : emptyColor;
        }
    }
}