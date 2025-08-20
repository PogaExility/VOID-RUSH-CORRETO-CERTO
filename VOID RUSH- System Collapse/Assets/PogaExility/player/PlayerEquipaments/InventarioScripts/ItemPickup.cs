using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemSO itemData;

    // Usamos Start para garantir que isso rode para itens colocados na cena
    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        // Desliga a física para que o item fique "estático"
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;
    }
}