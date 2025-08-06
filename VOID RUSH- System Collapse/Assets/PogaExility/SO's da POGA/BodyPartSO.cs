// BodyPartSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBodyPart", menuName = "NEXUS/Body Part")]
public class BodyPartSO : ScriptableObject
{
    public string partName; // Ex: "Braço Padrão"

    [Header("Animações em Sprite")]
    public Sprite idleSprite;
    public Sprite[] walkCycle;
    public Sprite jumpSprite;
    public Sprite fallSprite;
    public Sprite wallSlideSprite;
    // etc...
}