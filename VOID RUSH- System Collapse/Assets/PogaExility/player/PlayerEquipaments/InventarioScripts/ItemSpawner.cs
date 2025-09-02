using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void SpawnItemInWorld(ItemSO item, Vector3 position, int amount)
    {
        if (item == null || item.itemPrefab == null) return;

        GameObject itemObject = Instantiate(item.itemPrefab, position, Quaternion.identity);
        ItemPickup pickupComponent = itemObject.GetComponent<ItemPickup>();
        if (pickupComponent == null) pickupComponent = itemObject.AddComponent<ItemPickup>();

        pickupComponent.itemData = item;
        pickupComponent.amount = amount;
    }
}