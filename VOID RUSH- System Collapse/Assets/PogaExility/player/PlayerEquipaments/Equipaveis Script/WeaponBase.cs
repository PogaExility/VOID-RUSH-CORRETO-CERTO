// WeaponBase.cs - VERS�O CORRETA E COMPLETA
using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected ItemSO weaponData;

    public event Action OnWeaponStateChanged;

    public void Initialize(ItemSO data)
    {
        weaponData = data;
        InternalInitialize();
    }

    public abstract void Attack();

    protected void RaiseOnWeaponStateChanged()
    {
        OnWeaponStateChanged?.Invoke();
    }

    protected virtual void InternalInitialize()
    {
        // Implementa��o base vazia para as classes filhas sobrescreverem (override)
    }
}