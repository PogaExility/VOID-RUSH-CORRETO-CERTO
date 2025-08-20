using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste aqui o objeto que contém o InventoryManager.")]
    public InventoryManager inventoryManager;
    [Tooltip("Arraste aqui o transform do jogador.")]
    public Transform playerTransform;

    [Header("Configuração de 'Jogar Fora'")]
    [Tooltip("A força com que o item é 'cuspido' para fora do inventário.")]
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
        // Cancela a inscrição para evitar erros
        if (inventoryManager != null)
        {
            inventoryManager.OnItemDropped -= SpawnItemInWorld;
        }
    }

    // Função chamada pelo evento quando um item é removido pela "lixeira"
    private void SpawnItemInWorld(ItemSO itemData)
    {
        if (itemData == null || itemData.itemPrefab == null)
        {
            Debug.LogWarning("Tentativa de dropar um item que não tem um prefab definido!");
            return;
        }

        // Define a posição de spawn na frente do jogador
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