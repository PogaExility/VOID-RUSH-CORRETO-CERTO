// NOME DO ARQUIVO: ComboStepData.cs
[System.Serializable]
public class ComboStepData
{
    // Usaremos ENUMS para a performance dos Hashes
    public PlayerAnimState playerAnimationState;
    public ProjectileAnimState slashAnimationState;

    // E o AnimationClip para pegar a duração
    public UnityEngine.AnimationClip playerAnimationClip;

    public UnityEngine.GameObject slashEffectPrefab;
    public float damage;
    public float knockbackPower;
    public float lungeDistance;
    public float lungeSpeed;
    public float comboWindow = 0.5f;
    public float comboSpeed = 1f;
}