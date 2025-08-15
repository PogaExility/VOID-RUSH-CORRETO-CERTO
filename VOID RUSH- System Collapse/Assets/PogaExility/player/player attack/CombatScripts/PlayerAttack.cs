using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerStats), typeof(EnergyBarController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Referências")]
    public Transform attackPoint;
    private PlayerStats _playerStats;
    private EnergyBarController _energyBar;
    private PlayerAnimatorController _animatorController;

    [Header("Status de Combate")]
    private int _currentAmmo;
    private bool _isReloading = false;
    private int _comboCounter = 0;
    private float _lastAttackTime = 0f;
    private bool _isAttacking = false;

    [Header("Configurações Gerais")]
    public float comboResetTime = 1.5f;

    void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _energyBar = GetComponent<EnergyBarController>();
        // _animatorController = GetComponent<PlayerAnimatorController>();
    }

    public void PerformAttack(WeaponSO weapon)
    {
        if (_isReloading || _isAttacking) return;

        switch (weapon.weaponType)
        {
            case WeaponType.Melee:
                StartCoroutine(MeleeAttackCoroutine(weapon));
                break;
            case WeaponType.Firearm:
                HandleFirearmAttack(weapon);
                break;
            case WeaponType.Buster:
                HandleBusterAttack(weapon);
                break;
        }
    }

    private IEnumerator MeleeAttackCoroutine(WeaponSO weapon)
    {
        _isAttacking = true;
        if (Time.time - _lastAttackTime > comboResetTime) { _comboCounter = 0; }
        Debug.Log("Ataque Melee #" + (_comboCounter + 1));
        float attackDuration = 0.5f;
        yield return new WaitForSeconds(attackDuration);
        _lastAttackTime = Time.time;
        _comboCounter++;
        if (_comboCounter >= 3) { _comboCounter = 0; }
        _isAttacking = false;
    }

    private void HandleFirearmAttack(WeaponSO weapon)
    {
        if (_currentAmmo <= 0) { Debug.Log("Sem munição!"); return; }
        _currentAmmo--;
        Debug.Log("Tiro! Munição restante: " + _currentAmmo + "/" + weapon.magazineSize);
    }

    private void HandleBusterAttack(WeaponSO weapon)
    {
        if (_energyBar.HasEnoughEnergy(weapon.baseEnergyCost))
        {
            _energyBar.ConsumeEnergy(weapon.baseEnergyCost);
            Debug.Log("Tiro de Buster!");
        }
        else { Debug.Log("Energia insuficiente!"); }
    }

    public void Reload(WeaponSO weapon)
    {
        if (weapon == null || weapon.weaponType != WeaponType.Firearm || _isReloading) return;
        StartCoroutine(ReloadCoroutine(weapon.reloadTime, weapon.magazineSize));
    }

    private IEnumerator ReloadCoroutine(float reloadTime, int magazineSize)
    {
        _isReloading = true;
        Debug.Log("Recarregando...");
        yield return new WaitForSeconds(reloadTime);
        _currentAmmo = magazineSize;
        _isReloading = false;
        Debug.Log("Recarga completa!");
    }

    public void OnWeaponEquipped(WeaponSO weapon)
    {
        if (weapon != null && weapon.weaponType == WeaponType.Firearm)
        {
            _currentAmmo = weapon.magazineSize;
            _isReloading = false;
        }
    }

    // ====================================================================
    // FUNÇÕES DE ESTADO ADICIONADAS
    // ====================================================================
    public bool IsAttacking() => _isAttacking;
    public bool IsReloading() => _isReloading;
}