// TerrainTypeSO.cs - Versão Corrigida com Campo de Prefab
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainType", menuName = "NEXUS/Terrain Type", order = 0)]
public class TerrainTypeSO : ScriptableObject
{
    [Header("Propriedades do Terreno")]

    [Tooltip("O prefab que será instanciado para este tipo de terreno pelo gerador procedural.")]
    public GameObject terrainPrefab; // <<< A LINHA QUE FALTAVA

    [Tooltip("A Layer que este tipo de terreno ocupa.")]
    public LayerMask layer;

    [Tooltip("Quão 'escorregadio' é este terreno. 1 = atrito normal, 0 = sem atrito (gelo).")]
    [Range(0f, 1f)]
    public float friction = 1f;

    // Futuramente, podemos adicionar mais coisas aqui:
    // public GameObject dustParticles;
    // public AudioClip footstepSound;
}