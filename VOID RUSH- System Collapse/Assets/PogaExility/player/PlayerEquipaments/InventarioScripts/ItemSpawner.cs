using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("Arraste aqui o objeto que cont�m o InventoryManager.")]
    public InventoryManager inventoryManager;
    [Tooltip("Arraste aqui o transform do jogador.")]
    public Transform playerTransform;

    [Header("Configura��o de 'Jogar Fora'")]
    [Tooltip("A for�a com que o item � 'cuspido' para fora do invent�rio.")]
    public float dropForce = 3f;

    void Start()
    {
        // Se inscreve no evento do InventoryManager
        if (inventoryManager != null)
        {
            inventoryManager.OnItemDropped += SpawnItemInWorld;
        }
    }

    void OnDestroy()
    {
        // Cancela a inscri��o para evitar erros
        if (inventoryManager != null)
        {
            inventoryManager.OnItemDropped -= SpawnItemInWorld;
        }
    }

    // Fun��o chamada pelo evento quando um item � removido pela "lixeira"
    private void SpawnItemInWorld(ItemSO itemData)
    {
        if (itemData == null || itemData.itemPrefab == null)
        {
            Debug.LogWarning("Tentativa de dropar um item que n�o tem um prefab definido!");
            return;
        }

        // Define a posi��o de spawn na frente do jogador
        Vector3 spawnPosition = playerTransform.position + playerTransform.right * 1.5f; // 1.5 unidades na frente

        // Cria o objeto do item no mundo
        GameObject itemObject = Instantiate(itemData.itemPrefab, spawnPosition, Quaternion.identity);

        // Adiciona um impulso para que ele "pule" para fora
        Rigidbody2D rb = itemObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dropDirection = (playerTransform.right + Vector3.up).normalized;
            rb.AddForce(dropDirection * dropForce, ForceMode2D.Impulse);
        }
    }
}