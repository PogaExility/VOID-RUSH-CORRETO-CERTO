using UnityEngine;

// Esta � uma classe "abstrata". Ela � um contrato.
// Toda arma no seu jogo DEVE ter estas fun��es.
public abstract class WeaponBase : MonoBehaviour
{
    protected ItemSO weaponData; // A "identidade" da arma (dano, cad�ncia, etc.)

    // O WeaponHandler vai chamar esta fun��o para dar a identidade � arma
    public void Initialize(ItemSO data)
    {
        weaponData = data;
    }

    // A �nica ordem que o WeaponHandler vai dar.
    public abstract void Attack();
}