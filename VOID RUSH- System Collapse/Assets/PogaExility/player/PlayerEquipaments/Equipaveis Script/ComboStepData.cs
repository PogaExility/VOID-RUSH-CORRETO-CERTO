using UnityEngine;

[System.Serializable]
public class ComboStepData
{
    public PlayerAnimState comboBodyAnimation; // << MUDANÇA AQUI
    public ProjectileAnimState slashAnimation; // << MUDANÇA AQUI

    public GameObject slashEffectPrefab;
    public float damage;
    public float knockbackPower;
    public float lungeDistance;
    public float comboWindow = 0.5f;
}