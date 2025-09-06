using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemView : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;

    // >> A FUNÇÃO QUE FALTAVA <<
    public void Initialize(int index)
    {
        // Se precisarmos do índice no futuro, ele estará aqui.
        // Por agora, ela só precisa existir para o código compilar.
    }

    public void Render(ItemSO itemData, int count)
    {
        icon.sprite = itemData.itemIcon;
        countText.text = count > 1 ? count.ToString() : "";
    }
}