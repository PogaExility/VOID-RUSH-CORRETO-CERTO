using UnityEngine;

// Esta é uma classe "abstrata". Ela é um contrato.
// Toda arma no seu jogo DEVE ter estas funções.
public abstract class WeaponBase : MonoBehaviour
{
    protected ItemSO weaponData; // A "identidade" da arma (dano, cadência, etc.)

    // O WeaponHandler vai chamar esta função para dar a identidade à arma
    public void Initialize(ItemSO data)
    {
        weaponData = data;
    }

    // A única ordem que o WeaponHandler vai dar.
    public abstract void Attack();
}