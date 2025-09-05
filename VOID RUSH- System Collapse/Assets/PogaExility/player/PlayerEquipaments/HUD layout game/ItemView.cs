using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemView : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;

    public void Render(ItemSO itemData, int count)
    {
        icon.sprite = itemData.itemIcon;
        countText.text = count > 1 ? count.ToString() : "";
    }
}