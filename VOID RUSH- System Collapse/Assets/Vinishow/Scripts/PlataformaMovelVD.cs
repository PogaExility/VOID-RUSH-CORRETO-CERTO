using UnityEngine;

public class PlataformaMovelVD : MonoBehaviour
{
    [Header("Configuração do Trajeto")]
    [SerializeField] private Transform pontoA;
    [SerializeField] private Transform pontoB;

    [Header("Configuração de Movimento")]
    [SerializeField] private float velocidade = 2.0f;
    [SerializeField] private float tempoDeEspera = 1.0f;

    private Rigidbody2D rb;
    private float proximoTempoDeMovimento = 0f;

    // --- NOVAS VARIÁVEIS para guardar as posições ---
    private Vector3 posicaoDestino;
    private Vector3 posicaoA_inicial;
    private Vector3 posicaoB_inicial;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (pontoA == null || pontoB == null)
        {
            Debug.LogError("Os pontos A e B da plataforma móvel não foram definidos!", this);
            this.enabled = false;
            return;
        }

        // --- MODIFICADO: Memorizamos as posições aqui ---
        posicaoA_inicial = pontoA.position;
        posicaoB_inicial = pontoB.position;

        // Define o destino inicial.
        posicaoDestino = posicaoA_inicial;
    }

    void FixedUpdate()
    {
        if (Time.time >= proximoTempoDeMovimento)
        {
            // --- MODIFICADO: Usamos as posições memorizadas ---
            Vector2 proximaPosicao = Vector2.MoveTowards(transform.position, posicaoDestino, velocidade * Time.fixedDeltaTime);
            rb.MovePosition(proximaPosicao);

            // Compara a posição atual com o destino memorizado.
            if (Vector2.Distance(transform.position, posicaoDestino) < 0.01f)
            {
                proximoTempoDeMovimento = Time.time + tempoDeEspera;

                // Troca o destino memorizado.
                if (posicaoDestino == posicaoA_inicial)
                {
                    posicaoDestino = posicaoB_inicial;
                }
                else
                {
                    posicaoDestino = posicaoA_inicial;
                }
            }
        }
    }

    // As funções de colisão não precisam de mudança.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D point in collision.contacts)
            {
                if (point.normal.y > 0.5f)
                {
                    collision.transform.SetParent(this.transform);
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}