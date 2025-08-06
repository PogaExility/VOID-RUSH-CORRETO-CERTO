// BodyPartSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBodyPart", menuName = "NEXUS/Body Part")]
public class BodyPartSO : ScriptableObject
{
    public string partName; // Ex: "Bra�o Padr�o"

    [Header("Anima��es em Sprite")]
    public Sprite idleSprite;
    public Sprite[] walkCycle;
    public Sprite jumpSprite;
    public Sprite fallSprite;
    public Sprite wallSlideSprite;
    // etc...
}