using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Referências")]
    public InventoryManager inventoryManager;
    public Transform playerTransform;

    [Header("Configuração")]
    public float dropForce = 3f;

    void Awake()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnItemDropped += SpawnItemInWorld;
        }
    }

    void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnItemDropped -= SpawnItemInWorld;
        }
    }

    private void SpawnItemInWorld(ItemSO itemData)
    {
        if (itemData == null || itemData.itemPrefab == null) return;

        Vector3 spawnPosition = playerTransform.position + playerTransform.right * 1.5f;
        GameObject itemObject = Instantiate(itemData.itemPrefab, spawnPosition, Quaternion.identity);

        // --- LIGA A FÍSICA ---
        Rigidbody2D rb = itemObject.GetComponent<Rigidbody2D>();
        Collider2D itemCollider = itemObject.GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; // LIGA A GRAVIDADE
            Vector2 dropDirection = (playerTransform.right + Vector3.up).normalized;
            rb.AddForce(dropDirection * dropForce, ForceMode2D.Impulse);
        }

        if (itemCollider != null)
        {
            itemCollider.isTrigger = false; // TORNA SÓLIDO
        }
    }
}