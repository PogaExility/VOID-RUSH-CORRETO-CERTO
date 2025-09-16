using UnityEngine;

[System.Serializable]
public class ComboStepData
{
    public PlayerAnimState comboBodyAnimation; // << MUDAN�A AQUI
    public ProjectileAnimState slashAnimation; // << MUDAN�A AQUI

    public GameObject slashEffectPrefab;
    public float damage;
    public float knockbackPower;
    public float lungeDistance;
    public float comboWindow = 0.5f;
}