using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(RectTransform))]
public class InventoryItemView : MonoBehaviour
{
    private Image itemImage;
    private RectTransform rectTransform;

    void Awake()
    {
        itemImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // Configura o pivô e as âncoras para o canto superior esquerdo
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
    }

    // Configura a aparência deste objeto
    public void Render(ItemSO item, float cellSize)
    {
        itemImage.sprite = item.itemIcon;
        rectTransform.sizeDelta = new Vector2(item.width * cellSize, item.height * cellSize);
    }
}