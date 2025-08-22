using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InventorySlotView : MonoBehaviour
{
    private Image slotImage;

    [Header("Cores do Slot")]
    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public Color occupiedColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    public Color previewValidColor = new Color(0f, 1f, 0f, 0.3f);
    public Color previewInvalidColor = new Color(1f, 0f, 0f, 0.3f);

    public bool isOccupied = false;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        ResetColor();
    }

    public void SetState(bool occupied)
    {
        isOccupied = occupied;
        ResetColor();
    }

    public void SetPreviewColor(bool isValid)
    {
        slotImage.color = isValid ? previewValidColor : previewInvalidColor;
    }

    public void ResetColor()
    {
        slotImage.color = isOccupied ? occupiedColor : emptyColor;
    }
}