using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemSO itemData;
    public int amount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager manager = other.GetComponentInChildren<InventoryManager>();
            if (manager != null && manager.TryAddItem(itemData, amount))
            {
                Destroy(gameObject);
            }
        }
    }
}