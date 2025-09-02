// FILE: ItemPickup.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    public ItemSO itemData;
    public int amount = 1;

    void Reset()
    {
        // Garanta que o collider é Trigger para acionar OnTriggerEnter2D no Player
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }
}
