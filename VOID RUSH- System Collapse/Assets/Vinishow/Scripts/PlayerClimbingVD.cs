using UnityEngine;

// Garante que este script s� possa ser adicionado a um objeto que j� tem os componentes essenciais.
[RequireComponent(typeof(Rigidbody2D), typeof(AdvancedPlayerMovement2D))]
public class PlayerClimbingVD : MonoBehaviour
{
    [Header("Configura��o da Escada")]
    [Tooltip("A velocidade com que o jogador sobe e desce as escadas.")]
    [SerializeField] private float climbingSpeed = 5f;
    [Tooltip("A camada (Layer) em que os objetos de escada se encontram.")]
    [SerializeField] private LayerMask ladderLayer;

    // Refer�ncias a outros componentes no jogador
    private Rigidbody2D rb;
    private AdvancedPlayerMovement2D playerMovement;

    // Controle de estado interno
    private bool isOnLadder = false;
    private bool isClimbing = false;
    private float verticalInput;
    private float originalGravityScale;

    void Awake()
    {
        // Pega as refer�ncias dos componentes no mesmo GameObject
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<AdvancedPlayerMovement2D>();

        // Guarda a escala de gravidade original para podermos restaur�-la depois
        originalGravityScale = rb.gravityScale;
    }

    void Update()
    {
        // L� o input vertical do jogador (W/S ou direcional para cima/baixo)
        verticalInput = Input.GetAxisRaw("Vertical");

        // Se o jogador est� em contato com uma escada e aperta para cima ou para baixo, come�a a escalar
        if (isOnLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }

        // Se o jogador n�o est� em contato com uma escada, ele n�o pode estar escalando
        if (!isOnLadder)
        {
            isClimbing = false;
        }

        // Se o jogador est� escalando...
        if (isClimbing)
        {
            // Desativa a gravidade
            playerMovement.SetGravityScale(0f);
            // Controla o movimento vertical
            rb.velocity = new Vector2(rb.velocity.x, verticalInput * climbingSpeed);
            // Trava o movimento horizontal do script principal
            playerMovement.enabled = false;
        }
        else
        {
            // Se n�o est� escalando, devolve o controle ao script de movimento principal
            playerMovement.SetGravityScale(originalGravityScale); // Restaura a gravidade
            playerMovement.enabled = true; // Reativa o script de movimento
        }
    }

    // Chamado quando o jogador entra em um trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto com que colidiu est� na camada "Escada"
        if ((ladderLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isOnLadder = true;
        }
    }

    // Chamado quando o jogador sai de um trigger
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Verifica se o objeto que est� deixando � uma escada
        if ((ladderLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isOnLadder = false;
            isClimbing = false; // For�a a parada da escalada ao sair
        }
    }
}