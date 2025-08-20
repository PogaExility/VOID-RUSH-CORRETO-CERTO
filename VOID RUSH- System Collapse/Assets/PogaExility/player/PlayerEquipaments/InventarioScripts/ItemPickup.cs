using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [Tooltip("O ScriptableObject do item que este objeto representa.")]
    public ItemSO itemData;

    // Garante que o Collider2D seja um Trigger para não bloquear o jogador
    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }
}