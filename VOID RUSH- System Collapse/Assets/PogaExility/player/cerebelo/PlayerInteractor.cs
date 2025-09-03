using UnityEngine;

// Coloque este componente no seu objeto Player
[RequireComponent(typeof(Collider2D))]
public class PlayerInteractor : MonoBehaviour
{
    // Crie um campo público para arrastar a referência
    [SerializeField] private InventoryManager inventoryManager;

    private void Start()
    {
        // Agora apenas verificamos se a referência foi atribuída no Inspector.
        if (inventoryManager == null)
        {
            // Se você esqueceu, tentamos encontrar o manager na cena como um plano B.
            inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (inventoryManager == null)
            {
                Debug.LogError("Referência do InventoryManager não foi atribuída no PlayerInteractor E não foi encontrado na cena!", this);
                enabled = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<ItemPickup>(out var pickup))
        {
            int amountLeft = inventoryManager.TryAddItem(pickup.itemData, pickup.amount);

            if (amountLeft == 0)
            {
                Destroy(other.gameObject);
            }
            else
            {
                pickup.amount = amountLeft;
                Debug.Log("Inventário cheio! Não foi possível pegar todos os itens.");
            }
        }
    }
}