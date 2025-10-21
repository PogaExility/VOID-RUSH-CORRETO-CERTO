using UnityEngine;

public class OneWayPlatformVD : MonoBehaviour
{
    // Variáveis para guardar os números das layers para eficiência
    private int platformLayerIndex;
    private int groundLayerIndex;

    // Referência ao collider da plataforma
    private Collider2D platformCollider;

    void Start()
    {
        // Pega os números inteiros que representam as layers
        platformLayerIndex = LayerMask.NameToLayer("Plataforma");
        groundLayerIndex = LayerMask.NameToLayer("Chao");

        platformCollider = GetComponent<Collider2D>();
    }

    // Chamado quando outro collider entra em contato com este
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica se o objeto que colidiu é o jogador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Verifica se o jogador está vindo de cima.
            // Pega o ponto de contato mais baixo do jogador e compara com o ponto mais alto da plataforma.
            if (collision.transform.position.y > platformCollider.bounds.max.y)
            {
                // Se o jogador pousou em cima, muda a layer da plataforma para "Chao"
                gameObject.layer = groundLayerIndex;
            }
        }
    }

    // Chamado quando o outro collider para de tocar neste
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Verifica se o objeto que está saindo é o jogador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Muda a layer da plataforma de volta para "Plataforma"
            gameObject.layer = platformLayerIndex;
        }
    }
}