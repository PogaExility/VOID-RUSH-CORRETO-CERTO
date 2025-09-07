// ARQUIVO NOVO: MeleeWeapon.cs
using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    private float lastAttackTime = -999f;

    public override void Attack()
    {
        if (Time.time < lastAttackTime + weaponData.attackRate) return;
        lastAttackTime = Time.time;

        Debug.Log($"ATAQUE MELEE COM: {weaponData.itemName}");
        // Lógica de ataque de faca vai aqui
        RaiseOnWeaponStateChanged();
    }
}