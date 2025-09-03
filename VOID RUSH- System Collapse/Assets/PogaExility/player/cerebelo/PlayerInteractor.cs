using UnityEngine;

// Coloque este componente no seu objeto Player
[RequireComponent(typeof(Collider2D))]
public class PlayerInteractor : MonoBehaviour
{
    // Crie um campo p�blico para arrastar a refer�ncia
    [SerializeField] private InventoryManager inventoryManager;

    private void Start()
    {
        // Agora apenas verificamos se a refer�ncia foi atribu�da no Inspector.
        if (inventoryManager == null)
        {
            // Se voc� esqueceu, tentamos encontrar o manager na cena como um plano B.
            inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (inventoryManager == null)
            {
                Debug.LogError("Refer�ncia do InventoryManager n�o foi atribu�da no PlayerInteractor E n�o foi encontrado na cena!", this);
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
                Debug.Log("Invent�rio cheio! N�o foi poss�vel pegar todos os itens.");
            }
        }
    }
}