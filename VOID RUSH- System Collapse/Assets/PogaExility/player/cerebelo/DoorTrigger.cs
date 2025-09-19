using UnityEngine;

/// <summary>
/// Este script simples ativa o modo de câmera livre (sem limites) quando o jogador
/// entra em seu trigger. Deve ser colocado em GameObjects que funcionam como portas ou
/// zonas de transição entre salas.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DoorTrigger : MonoBehaviour
{
    private void Awake()
    {
        // Garante que o collider deste objeto seja sempre um trigger para evitar colisões físicas.
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou no trigger é o jogador.
        if (other.CompareTag("Player"))
        {
            // Chama o método estático para colocar a câmera em modo "Follow".
            // Isso efetivamente "mata" o collider da sala atual.
            RoomBoundary.SetFollowMode();

            Debug.Log($"Jogador tocou a porta '{gameObject.name}'. Ativando modo de câmera livre.");
        }
    }
}