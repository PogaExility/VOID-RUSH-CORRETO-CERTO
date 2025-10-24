using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(AdvancedPlayerMovement2D))]
public class PlayerClimbingVD : MonoBehaviour
{
    [Header("Configura��o da Escada")]
    [SerializeField] private float climbingSpeed = 5f;
    [SerializeField] private LayerMask ladderLayer;

    // Refer�ncias
    private Rigidbody2D rb;
    private AdvancedPlayerMovement2D playerMovement;

    // Controle de estado
    private bool isOnLadder = false;
    private bool isClimbing = false;
    private float verticalInput;
    private float originalGravityScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<AdvancedPlayerMovement2D>();
        // � mais seguro pegar a gravidade do script de movimento, caso ele a modifique.
        originalGravityScale = playerMovement.baseGravity;
    }

    void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");

        // --- L�GICA DE PULO ADICIONADA ---
        // Se o jogador est� escalando e aperta o bot�o de pulo...
        if (isClimbing && Input.GetButtonDown("Jump"))
        {
            isClimbing = false; // Para de escalar
            // N�o precisa chamar playerMovement.DoJump() aqui, 
            // pois o script principal vai ler o input no mesmo frame.
            return; // Sai da fun��o Update deste script para o playerMovement assumir.
        }
        // --- FIM DA L�GICA DE PULO ---

        if (isOnLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }

        if (!isOnLadder)
        {
            isClimbing = false;
        }

        // Se o jogador est� escalando...
        if (isClimbing)
        {
            // O movimento horizontal � zerado para que o jogador n�o deslize para os lados
            rb.linearVelocity = new Vector2(0, verticalInput * climbingSpeed);
            playerMovement.SetGravityScale(0f);
            playerMovement.enabled = false; // Desativa o script principal
        }
        else
        {
            // Se n�o est� escalando, devolve o controle.
            playerMovement.enabled = true; // Reativa o script principal
            // O pr�prio script principal agora � respons�vel por restaurar a gravidade.
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((ladderLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isOnLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((ladderLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isOnLadder = false;
            isClimbing = false;
        }
    }
}