using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Referências de Equipamento")]
    [Tooltip("Arma corpo a corpo equipada atualmente.")]
    public WeaponSO equippedMeleeWeapon;

    [Tooltip("Arma de fogo equipada atualmente.")]
    public WeaponSO equippedFirearm;

    [Tooltip("Buster equipado atualmente.")]
    public WeaponSO equippedBuster;

    [Header("Inventário (Maleta)")]
    [Tooltip("A lista de todos os itens que o jogador está carregando.")]
    public List<ItemSO> inventoryItems = new List<ItemSO>();

    // Futuramente, esta classe também terá a lógica para o sistema de grid.

    // --- Funções de Gerenciamento ---

    public void AddItem(ItemSO itemToAdd)
    {
        // Por enquanto, apenas adiciona o item à lista.
        // Futuramente, aqui entraria a lógica de verificar espaço no grid.
        inventoryItems.Add(itemToAdd);
        Debug.Log("Adicionado ao inventário: " + itemToAdd.itemName);
    }

    public void RemoveItem(ItemSO itemToRemove)
    {
        if (inventoryItems.Contains(itemToRemove))
        {
            inventoryItems.Remove(itemToRemove);
            Debug.Log("Removido do inventário: " + itemToRemove.itemName);
        }
    }

    // A UI do inventário vai chamar esta função quando o jogador clicar em "Equipar".
    public void EquipWeapon(WeaponSO weaponToEquip)
    {
        // Verifica se o jogador realmente possui esta arma no inventário.
        if (!inventoryItems.Contains(weaponToEquip))
        {
            Debug.LogWarning("Tentativa de equipar a arma '" + weaponToEquip.itemName + "' que não está no inventário.");
            return;
        }

        // Coloca a arma no slot de equipamento correto com base no seu tipo.
        switch (weaponToEquip.weaponType)
        {
            case WeaponType.Melee:
                equippedMeleeWeapon = weaponToEquip;
                Debug.Log("Arma corpo a corpo equipada: " + weaponToEquip.itemName);
                break;
            case WeaponType.Firearm:
                equippedFirearm = weaponToEquip;
                Debug.Log("Arma de fogo equipada: " + weaponToEquip.itemName);
                break;
            case WeaponType.Buster:
                equippedBuster = weaponToEquip;
                Debug.Log("Buster equipado: " + weaponToEquip.itemName);
                break;
        }
    }

    // Função para desequipar uma arma, se necessário.
    public void UnequipWeapon(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Melee:
                equippedMeleeWeapon = null;
                break;
            case WeaponType.Firearm:
                equippedFirearm = null;
                break;
            case WeaponType.Buster:
                equippedBuster = null;
                break;
        }
    }
}