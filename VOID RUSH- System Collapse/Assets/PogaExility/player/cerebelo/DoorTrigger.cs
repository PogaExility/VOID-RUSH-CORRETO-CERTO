using UnityEngine;

/// <summary>
/// Este script simples ativa o modo de c�mera livre (sem limites) quando o jogador
/// entra em seu trigger. Deve ser colocado em GameObjects que funcionam como portas ou
/// zonas de transi��o entre salas.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DoorTrigger : MonoBehaviour
{
    private void Awake()
    {
        // Garante que o collider deste objeto seja sempre um trigger para evitar colis�es f�sicas.
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou no trigger � o jogador.
        if (other.CompareTag("Player"))
        {
            // Chama o m�todo est�tico para colocar a c�mera em modo "Follow".
            // Isso efetivamente "mata" o collider da sala atual.
            RoomBoundary.SetFollowMode();

            Debug.Log($"Jogador tocou a porta '{gameObject.name}'. Ativando modo de c�mera livre.");
        }
    }
}