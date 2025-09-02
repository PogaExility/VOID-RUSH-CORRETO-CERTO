using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    // Singleton simples e compatível com Unity 2021
    public static ItemSpawner Instance { get; private set; }

    [Header("Impulso ao dropar")]
    [Tooltip("Força inicial aplicada ao item ao ser droppado (AddForce).")]
    public float dropImpulse = 4f;

    [Tooltip("Direção padrão do impulso se nenhuma for passada.")]
    public Vector2 defaultImpulseDir = new Vector2(1f, 0.6f);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Spawna um item no mundo, já com ItemPickup, Collider2D (trigger) e Rigidbody2D DINÂMICO.
    /// </summary>
    /// <param name="item">ItemSO do item</param>
    /// <param name="position">Posição de spawn</param>
    /// <param name="amount">Quantidade (para stack / contador do pickup)</param>
    /// <param name="impulseDir">Direção opcional do impulso inicial</param>
    public void SpawnItemInWorld(ItemSO item, Vector3 position, int amount, Vector2? impulseDir = null)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemSpawner] ItemSO nulo.");
            return;
        }

        // Prefab preferencial do teu ItemSO (ajuste o campo conforme o teu scriptable)
        GameObject prefab = item.itemPrefab; // se você usa 'worldPickupPrefab', troque aqui
        GameObject go;

        if (prefab != null)
        {
            go = Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            // Fallback seguro se o SO não tiver prefab setado
            go = new GameObject($"Pickup_{item.itemName}");
            go.transform.position = position;

            // Sprite pra enxergar algo
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = item.itemIcon;
        }

        // Garante ItemPickup com dados corretos
        var pickup = go.GetComponent<ItemPickup>();
        if (!pickup) pickup = go.AddComponent<ItemPickup>();
        pickup.itemData = item;
        pickup.amount = Mathf.Max(1, amount);

        // Collider2D como Trigger (pra Interact/E funcionar via trigger no Player)
        var col = go.GetComponent<Collider2D>();
        if (!col) col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // Rigidbody2D DINÂMICO (sem kinematic)
        var rb = go.GetComponent<Rigidbody2D>();
        if (!rb) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
    

        // Impulso inicial
        Vector2 dir = impulseDir ?? defaultImpulseDir;
        if (dir.sqrMagnitude > 0.0001f)
            rb.AddForce(dir.normalized * dropImpulse, ForceMode2D.Impulse);
    }
}
