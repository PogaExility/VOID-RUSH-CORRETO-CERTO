// WeaponBase.cs - VERSÃO COMPLETA E CORRIGIDA
using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected ItemSO weaponData;
    public event Action OnWeaponStateChanged;

    // A função Initialize agora é VIRTUAL e aceita a munição salva.
    // O valor padrão -1 é usado para armas que não têm munição ou que estão sendo criadas pela primeira vez.
    public virtual void Initialize(ItemSO data, int savedAmmo = -1)
    {
        weaponData = data;
    }

    public abstract void Attack();

    protected void RaiseOnWeaponStateChanged()
    {
        OnWeaponStateChanged?.Invoke();
    }
}