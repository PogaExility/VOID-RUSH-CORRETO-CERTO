using UnityEngine;
using System.Collections.Generic; // Necessário para usar List<T>

// A tag [System.Serializable] é o que permite que objetos desta classe
// apareçam e sejam editáveis no Inspector da Unity.
[System.Serializable]
public class SpriteAnimationVD
{
    [Tooltip("O nome que usaremos para chamar esta animação via script (ex: 'Patrulhando', 'Alerta').")]
    public string stateName;

    [Tooltip("A lista de sprites (frames) para esta animação, em ordem.")]
    public List<Sprite> frames = new List<Sprite>();

    [Tooltip("Quantos frames desta animação devem tocar por segundo.")]
    public int framesPerSecond = 10;

    [Tooltip("Marque se a animação deve repetir em loop.")]
    public bool loop = true;
}