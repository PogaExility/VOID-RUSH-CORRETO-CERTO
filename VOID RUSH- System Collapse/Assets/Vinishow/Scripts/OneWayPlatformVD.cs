using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

// ADI��O: Garantir que o PlatformEffector2D tamb�m exista.
[RequireComponent(typeof(TilemapCollider2D), typeof(PlatformEffector2D))]
public class OneWayPlatformVD : MonoBehaviour
{
    [Header("Configura��o da Plataforma")]
    [Tooltip("Dura��o em segundos que a plataforma ficar� desativada para o jogador cair.")]
    [SerializeField] private float dropDuration = 0.5f; // Aumentei o valor padr�o para evitar o problema de ser "puxado" de volta.

    // Refer�ncias para os componentes.
    private TilemapCollider2D tilemapCollider;
    private PlatformEffector2D platformEffector; // NOVO: Refer�ncia para o effector.

    // Vari�vel de controle para evitar que a fun��o seja chamada m�ltiplas vezes seguidas.
    private bool isDropping = false;

    // A fun��o Awake � chamada quando o script � carregado. Ideal para pegar refer�ncias.
    void Awake()
    {
        tilemapCollider = GetComponent<TilemapCollider2D>();
        platformEffector = GetComponent<PlatformEffector2D>(); // NOVO: Pega o componente effector.

        if (tilemapCollider == null || platformEffector == null)
        {
            Debug.LogError("Um ou mais componentes necess�rios (TilemapCollider2D, PlatformEffector2D) n�o foram encontrados!", this);
        }
    }

    // Esta fun��o � chamada continuamente para cada frame que um outro colisor est� em contato com este.
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Verifica se � o jogador.
        if (collision.gameObject.CompareTag("Player"))
        {
            // --- NOVA L�GICA PARA EVITAR O PULO EXTRA ---
            // Verifica se o jogador est� se movendo para cima (pulando atrav�s da plataforma).
            // collision.rigidbody.velocity.y > 0.1f � uma forma segura de checar se ele tem velocidade vertical positiva.
            if (collision.rigidbody.linearVelocity.y > 0.1f)
            {
                // Desativa o effector para que o jogador passe direto sem "pousar".
                platformEffector.enabled = false;
            }

            // --- L�GICA ANTIGA PARA DESCER DA PLATAFORMA ---
            // Se a rotina de queda j� estiver ativa, n�o faz nada.
            if (isDropping)
            {
                return;
            }

            // Verifica se a tecla "S" est� sendo segurada.
            if (Input.GetKey(KeyCode.S))
            {
                StartCoroutine(DisableColliderCoroutine());
            }
        }
    }

    // --- NOVA FUN��O ---
    // Esta fun��o � chamada no exato momento em que um colisor PARA de tocar no colisor deste objeto.
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Verifica se foi o jogador que parou de tocar a plataforma.
        if (collision.gameObject.CompareTag("Player"))
        {
            // Reativa o effector, garantindo que a plataforma volte a ser s�lida.
            // Isso acontece depois que o jogador j� atravessou ela por baixo.
            platformEffector.enabled = true;
        }
    }

    // Coroutine para descer da plataforma (pressionando 'S').
    private IEnumerator DisableColliderCoroutine()
    {
        isDropping = true;

        // Desativa o colisor do Tilemap inteiro para o jogador cair.
        tilemapCollider.enabled = false;

        yield return new WaitForSeconds(dropDuration);

        // Reativa o colisor.
        tilemapCollider.enabled = true;
        isDropping = false;
    }
}