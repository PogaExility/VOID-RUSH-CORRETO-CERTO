// TerrainTypeSO.cs - Vers�o Corrigida com Campo de Prefab
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainType", menuName = "NEXUS/Terrain Type", order = 0)]
public class TerrainTypeSO : ScriptableObject
{
    [Header("Propriedades do Terreno")]

    [Tooltip("O prefab que ser� instanciado para este tipo de terreno pelo gerador procedural.")]
    public GameObject terrainPrefab; // <<< A LINHA QUE FALTAVA

    [Tooltip("A Layer que este tipo de terreno ocupa.")]
    public LayerMask layer;

    [Tooltip("Qu�o 'escorregadio' � este terreno. 1 = atrito normal, 0 = sem atrito (gelo).")]
    [Range(0f, 1f)]
    public float friction = 1f;

    // Futuramente, podemos adicionar mais coisas aqui:
    // public GameObject dustParticles;
    // public AudioClip footstepSound;
}