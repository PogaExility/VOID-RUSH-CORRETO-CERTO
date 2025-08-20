using UnityEngine;

// Este enum define as categorias gerais de itens.
// Podemos adicionar mais no futuro (ex: 'Recurso', 'Tesouro').
public enum ItemType
{
    Weapon,
    Consumable,
    KeyItem
}

// O [CreateAssetMenu] nos permite criar "inst�ncias" deste item no menu da Unity.
// Vamos usar isso para os itens filhos, como as armas.
[CreateAssetMenu(fileName = "NewItem", menuName = "NEXUS/Itens/Item Base", order = 0)]
public class ItemSO : ScriptableObject
{
    [Header("Informa��es Gerais do Item")]
    [Tooltip("O nome do item que aparecer� na UI.")]
    public string itemName;

    [Tooltip("O �cone que ser� mostrado no invent�rio.")]
    public Sprite itemIcon;

    [Header("Configura��o do Invent�rio (Grid)")]
    [Tooltip("Quantos 'quadrados' o item ocupa na horizontal.")]
    [Range(1, 5)] // Limita o valor no Inspector para evitar erros
    public int width = 1;

    [Tooltip("Quantos 'quadrados' o item ocupa na vertical.")]
    [Range(1, 5)]
    public int height = 1;

    [Header("Categoria do Item")]
    [Tooltip("Define o tipo geral do item para organiza��o.")]
    public ItemType itemType;

}
