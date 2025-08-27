using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class ItemPickup : MonoBehaviour, IInteractable // <-- Assina o contrato
{
    [Header("Item Data")]
    public ItemSO itemData;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;
    }

    // Como assinamos o contrato, somos obrigados a ter esta fun��o.
    // Esta � a fun��o que o PlayerController vai chamar.
    public void Interact()
    {
        // Encontra o InventoryManager e pede para ele pegar o item.
        InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager != null)
        {
            if (inventoryManager.PickupItem(itemData))
            {
                // Se o item foi pego com sucesso, o objeto do mundo � destru�do.
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Invent�rio cheio, n�o foi poss�vel pegar o item.");
                // Opcional: Adicionar um som de "erro" ou feedback visual.
            }
        }
    }
}