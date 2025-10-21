using UnityEngine;

public class OneWayPlatformVD : MonoBehaviour
{
    // Vari�veis para guardar os n�meros das layers para efici�ncia
    private int platformLayerIndex;
    private int groundLayerIndex;

    // Refer�ncia ao collider da plataforma
    private Collider2D platformCollider;

    void Start()
    {
        // Pega os n�meros inteiros que representam as layers
        platformLayerIndex = LayerMask.NameToLayer("Plataforma");
        groundLayerIndex = LayerMask.NameToLayer("Chao");

        platformCollider = GetComponent<Collider2D>();
    }

    // Chamado quando outro collider entra em contato com este
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica se o objeto que colidiu � o jogador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Verifica se o jogador est� vindo de cima.
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
        // Verifica se o objeto que est� saindo � o jogador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Muda a layer da plataforma de volta para "Plataforma"
            gameObject.layer = platformLayerIndex;
        }
    }
}