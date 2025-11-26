using UnityEngine;

public class ResetZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verifica se quem entrou foi o jogador.
        if (other.CompareTag("Player"))
        {
            // 2. Verifica se o RespawnManager existe.
            if (RespawnManager.Instance != null)
            {
                // 3. Pega o Transform do jogador que colidiu.
                Transform playerTransform = other.transform;

                // 4. Chama a função RespawnPlayer, passando a referência do Transform do jogador.
                RespawnManager.Instance.RespawnPlayer(playerTransform);
            }
            else
            {
                Debug.LogError("O jogador entrou na ResetZone, mas não há um RespawnManager na cena!");
            }
        }
    }
}