using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(AdvancedPlayerMovement2D))]
public class PlayerClimbingVD : MonoBehaviour
{
    [Header("Configuração da Escada")]
    [SerializeField] private float climbingSpeed = 5f;
    [SerializeField] private LayerMask ladderLayer;

    // Referências
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
        // É mais seguro pegar a gravidade do script de movimento, caso ele a modifique.
        originalGravityScale = playerMovement.baseGravity;
    }

    void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");

        // --- LÓGICA DE PULO ADICIONADA ---
        // Se o jogador está escalando e aperta o botão de pulo...
        if (isClimbing && Input.GetButtonDown("Jump"))
        {
            isClimbing = false; // Para de escalar
            // Não precisa chamar playerMovement.DoJump() aqui, 
            // pois o script principal vai ler o input no mesmo frame.
            return; // Sai da função Update deste script para o playerMovement assumir.
        }
        // --- FIM DA LÓGICA DE PULO ---

        if (isOnLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }

        if (!isOnLadder)
        {
            isClimbing = false;
        }

        // Se o jogador está escalando...
        if (isClimbing)
        {
            // O movimento horizontal é zerado para que o jogador não deslize para os lados
            rb.linearVelocity = new Vector2(0, verticalInput * climbingSpeed);
            playerMovement.SetGravityScale(0f);
            playerMovement.enabled = false; // Desativa o script principal
        }
        else
        {
            // Se não está escalando, devolve o controle.
            playerMovement.enabled = true; // Reativa o script principal
            // O próprio script principal agora é responsável por restaurar a gravidade.
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