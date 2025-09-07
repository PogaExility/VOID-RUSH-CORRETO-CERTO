// ItemView.cs - VERSÃO SIMPLES E CORRETA (PARA O INVENTÁRIO)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;

    public void Initialize(int index) { }

    // Esta função SÓ se preocupa com a contagem de stack.
    public void Render(ItemSO itemData, int count)
    {
        if (itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        icon.sprite = itemData.itemIcon;

        // Mostra a contagem apenas se for maior que 1.
        if (count > 1)
        {
            countText.enabled = true;
            countText.text = count.ToString();
        }
        else
        {
            countText.enabled = false;
        }
    }
}