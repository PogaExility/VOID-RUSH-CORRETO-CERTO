// WeaponBase.cs - VERS�O COMPLETA E CORRIGIDA
using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected ItemSO weaponData;
    public event Action OnWeaponStateChanged;

    // A fun��o Initialize agora � VIRTUAL e aceita a muni��o salva.
    // O valor padr�o -1 � usado para armas que n�o t�m muni��o ou que est�o sendo criadas pela primeira vez.
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