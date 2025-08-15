// File: UnitDataSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "GameData/Simple Unit Data")]
public class DataSO : ScriptableObject
{
    [Header("Display Information")]
    [Tooltip("The name of the unit to be displayed in the UI.")]
    public string unitName = "Default Unit Name";

    [Tooltip("The icon or portrait sprite for this unit to be displayed in the UI.")]
    public Sprite icon;

    // Você pode adicionar outros campos aqui no futuro se precisar, como:
    // public string description;
    // public GameObject unitPrefab; // Se este SO também fosse responsável pelo prefab do jogo
    // public float maxHealth;
    // etc.
    // Mas para o seu pedido atual, apenas nome e ícone são o foco.
}