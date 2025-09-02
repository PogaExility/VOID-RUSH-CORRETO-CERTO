using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InventorySlotView : MonoBehaviour
{
    public Image background;
    public Color validPreview = new Color(0f, 1f, 0f, 0.25f);
    public Color invalidPreview = new Color(1f, 0f, 0f, 0.25f);
    public Color idle = new Color(1f, 1f, 1f, 0f);

    public void SetPreview(bool canPlace)
    {
        if (!background) return;
        background.color = canPlace ? validPreview : invalidPreview;
    }

    void OnEnable()
    {
        if (background) background.color = idle;
    }
}
