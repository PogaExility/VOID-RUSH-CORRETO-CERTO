using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [Tooltip("O ScriptableObject que define este item.")]
    public ItemSO itemData;

    // A �nica fun��o deste script � garantir que o item no mundo
    // tenha a f�sica correta para ser detectado pelo jogador.
    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        // Garante que o item seja est�tico e possa ser atravessado (trigger).
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;
    }
}