using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemView : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;

    // >> A FUN��O QUE FALTAVA <<
    public void Initialize(int index)
    {
        // Se precisarmos do �ndice no futuro, ele estar� aqui.
        // Por agora, ela s� precisa existir para o c�digo compilar.
    }

    public void Render(ItemSO itemData, int count)
    {
        icon.sprite = itemData.itemIcon;
        countText.text = count > 1 ? count.ToString() : "";
    }
}