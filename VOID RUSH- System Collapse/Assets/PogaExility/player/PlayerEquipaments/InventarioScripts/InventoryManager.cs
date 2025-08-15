using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Refer�ncias de Equipamento")]
    [Tooltip("Arma corpo a corpo equipada atualmente.")]
    public WeaponSO equippedMeleeWeapon;

    [Tooltip("Arma de fogo equipada atualmente.")]
    public WeaponSO equippedFirearm;

    [Tooltip("Buster equipado atualmente.")]
    public WeaponSO equippedBuster;

    [Header("Invent�rio (Maleta)")]
    [Tooltip("A lista de todos os itens que o jogador est� carregando.")]
    public List<ItemSO> inventoryItems = new List<ItemSO>();

    // Futuramente, esta classe tamb�m ter� a l�gica para o sistema de grid.

    // --- Fun��es de Gerenciamento ---

    public void AddItem(ItemSO itemToAdd)
    {
        // Por enquanto, apenas adiciona o item � lista.
        // Futuramente, aqui entraria a l�gica de verificar espa�o no grid.
        inventoryItems.Add(itemToAdd);
        Debug.Log("Adicionado ao invent�rio: " + itemToAdd.itemName);
    }

    public void RemoveItem(ItemSO itemToRemove)
    {
        if (inventoryItems.Contains(itemToRemove))
        {
            inventoryItems.Remove(itemToRemove);
            Debug.Log("Removido do invent�rio: " + itemToRemove.itemName);
        }
    }

    // A UI do invent�rio vai chamar esta fun��o quando o jogador clicar em "Equipar".
    public void EquipWeapon(WeaponSO weaponToEquip)
    {
        // Verifica se o jogador realmente possui esta arma no invent�rio.
        if (!inventoryItems.Contains(weaponToEquip))
        {
            Debug.LogWarning("Tentativa de equipar a arma '" + weaponToEquip.itemName + "' que n�o est� no invent�rio.");
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

    // Fun��o para desequipar uma arma, se necess�rio.
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