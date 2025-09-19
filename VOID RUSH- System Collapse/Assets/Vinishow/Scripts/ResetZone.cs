using UnityEngine;
using UnityEngine.SceneManagement; // É MUITO importante adicionar esta linha!

public class ResetZone : MonoBehaviour
{
    // Esta função é chamada automaticamente pela Unity quando outro objeto
    // com um Rigidbody entra no Trigger deste objeto.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nós verificamos se o objeto que entrou tem a tag "Player".
        // Isso evita que a cena resete se um inimigo ou outro objeto encostar.
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entrou na zona de reset. Recarregando a cena...");

            // Recarrega a cena ATUAL.
            // A gente pega o nome da cena ativa e manda carregar ela de novo.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}