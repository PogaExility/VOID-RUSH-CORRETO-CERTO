using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
public class OneWayPlatformVD : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Duração em segundos que a colisão com o jogador será ignorada.")]
    [SerializeField] private float ignoreDuration = 0.5f;

    // Referência para o colisor da plataforma.
    private TilemapCollider2D platformCollider;

    // Controle para evitar que a corrotina seja chamada múltiplas vezes.
    private bool isIgnoringCollision = false;

    void Awake()
    {
        platformCollider = GetComponent<TilemapCollider2D>();
    }

    // Usamos OnCollisionEnter2D para detectar o primeiro toque do jogador subindo.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isIgnoringCollision) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Verifica se o jogador está se movendo para cima com velocidade suficiente.
            // O ponto de contato (contacts[0].normal.y) nos diz a direção da colisão. 
            // Um valor próximo de -1 significa que o jogador bateu na parte de baixo da plataforma.
            if (collision.contacts[0].normal.y < -0.5f && collision.rigidbody.velocity.y > 0.1f)
            {
                StartCoroutine(IgnoreCollisionCoroutine(collision.collider));
            }
        }
    }

    // Usamos OnCollisionStay2D para checar continuamente se o jogador quer descer.
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isIgnoringCollision) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Se o jogador pressionar 'S' e estiver em cima da plataforma.
            // O ponto de contato (contacts[0].normal.y) próximo de 1 significa que o jogador está em cima.
            if (Input.GetKey(KeyCode.S) && collision.contacts[0].normal.y > 0.5f)
            {
                StartCoroutine(IgnoreCollisionCoroutine(collision.collider));
            }
        }
    }

    // Corrotina unificada para ignorar a colisão temporariamente.
    private IEnumerator IgnoreCollisionCoroutine(Collider2D playerCollider)
    {
        // 1. Ativa a trava de controle.
        isIgnoringCollision = true;

        // 2. Manda a física ignorar a colisão entre a plataforma e o jogador.
        Physics2D.IgnoreCollision(platformCollider, playerCollider, true);

        // 3. Espera o tempo definido.
        yield return new WaitForSeconds(ignoreDuration);

        // 4. Manda a física voltar a considerar a colisão.
        Physics2D.IgnoreCollision(platformCollider, playerCollider, false);

        // 5. Libera a trava de controle.
        isIgnoringCollision = false;
    }
}