using UnityEngine;

/// <summary>
/// Controla uma instância de inimigo na cena, inicializando seus status
/// e gerenciando sua percepção (visão) do ambiente.
/// </summary>
public class AI_Controller : MonoBehaviour
{
    [Header("Configuração de Dados")]
    [Tooltip("O ScriptableObject que define o tipo e os dados deste inimigo.")]
    [SerializeField] private EnemySO enemyData;

    [Tooltip("O nível desta instância específica do inimigo.")]
    [SerializeField] private int nivel = 1;

    [Header("Configuração da Visão")]
    [Tooltip("Um objeto filho que marca a origem do cone de visão.")]
    [SerializeField] private Transform pontoDeVisao;

    // --- ATRIBUTOS DA INSTÂNCIA ---
    public float VidaMaxima { get; private set; }
    public float VidaAtual { get; private set; }
    public float DanoAtual { get; private set; }
    public float VelocidadeAtual { get; private set; }
    public Transform AlvoDetectado { get; private set; }

    // --- REFERÊNCIAS DE COMPONENTES ---
    private Rigidbody2D rb;
    private Animator anim;

    private void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError($"O inimigo '{gameObject.name}' não possui um EnemySO atribuído no AI_Controller!", this);
            gameObject.SetActive(false);
            return;
        }
        if (pontoDeVisao == null)
        {
            Debug.LogError($"O inimigo '{gameObject.name}' não possui um 'Ponto de Visão' atribuído no AI_Controller!", this);
            gameObject.SetActive(false);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        InicializarStatus();
    }

    private void Update()
    {
        ProcurarAlvo();
    }

    private void ProcurarAlvo()
    {
        AlvoDetectado = null;
        float anguloInicial = (enemyData.anguloVisao / 2) * -1;
        float anguloStep = enemyData.anguloVisao / (enemyData.quantidadeRaiosVisao - 1);

        for (int i = 0; i < enemyData.quantidadeRaiosVisao; i++)
        {
            float anguloAtual = anguloInicial + (anguloStep * i);
            Vector3 direcaoRaio = Quaternion.Euler(0, 0, anguloAtual) * transform.right;

            RaycastHit2D hit = Physics2D.Raycast(pontoDeVisao.position, direcaoRaio, enemyData.raioVisao, enemyData.camadaAlvo | enemyData.camadaObstaculos);

            if (hit.collider != null)
            {
                // Verifica se o que atingimos está na camada do alvo
                if (((1 << hit.collider.gameObject.layer) & enemyData.camadaAlvo) != 0)
                {
                    AlvoDetectado = hit.transform;
                    break; // Encontrou o alvo, pode parar de procurar.
                }

                // Se não é o alvo, verifica se é um obstáculo que bloqueia a visão
                if (((1 << hit.collider.gameObject.layer) & enemyData.camadaObstaculos) != 0 && hit.collider.CompareTag("Chao"))
                {
                    // Este raio foi bloqueado por um obstáculo.
                    // Apenas continue para o próximo raio do loop.
                    continue;
                }
            }
        }
    }

    private void InicializarStatus()
    {
        int nivelCalculo = Mathf.Max(1, nivel);
        int nivelOffset = nivelCalculo - 1;

        float vidaMin = enemyData.vidaBase.x + (enemyData.aumentoVidaPorNivel.x * nivelOffset);
        float vidaMax = enemyData.vidaBase.y + (enemyData.aumentoVidaPorNivel.y * nivelOffset);
        VidaMaxima = Random.Range(vidaMin, vidaMax);
        VidaAtual = VidaMaxima;

        float danoMin = enemyData.danoBase.x + (enemyData.aumentoDanoPorNivel.x * nivelOffset);
        float danoMax = enemyData.danoBase.y + (enemyData.aumentoDanoPorNivel.y * nivelOffset);
        DanoAtual = Random.Range(danoMin, danoMax);

        VelocidadeAtual = enemyData.velocidadeMovimentoBase + (enemyData.aumentoVelocidadePorNivel * nivelOffset);

        float aumentoTotal = enemyData.escalaBase * enemyData.aumentoEscalaPercentualPorNivel * nivelOffset;
        float escalaFinal = enemyData.escalaBase + aumentoTotal;
        transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x) * escalaFinal, escalaFinal, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoDeVisao == null || enemyData == null) return;

        float anguloInicial = (enemyData.anguloVisao / 2) * -1;
        float anguloStep = enemyData.anguloVisao / (enemyData.quantidadeRaiosVisao - 1);

        for (int i = 0; i < enemyData.quantidadeRaiosVisao; i++)
        {
            float anguloAtual = anguloInicial + (anguloStep * i);
            Vector3 direcaoRaio = Quaternion.Euler(0, 0, anguloAtual) * transform.right;

            RaycastHit2D hit = Physics2D.Raycast(pontoDeVisao.position, direcaoRaio, enemyData.raioVisao, enemyData.camadaAlvo | enemyData.camadaObstaculos);

            Vector3 endPoint;
            if (hit.collider != null)
            {
                endPoint = hit.point;
                // Muda a cor baseado no que atingiu (COM VERIFICAÇÃO DE TAG)
                if (((1 << hit.collider.gameObject.layer) & enemyData.camadaAlvo) != 0)
                {
                    Gizmos.color = Color.green; // Verde para alvo
                }
                else if (((1 << hit.collider.gameObject.layer) & enemyData.camadaObstaculos) != 0 && hit.collider.CompareTag("Chao"))
                {
                    Gizmos.color = Color.red; // Vermelho para obstáculo válido
                }
                else
                {
                    Gizmos.color = Color.white; // Atingiu algo que não é alvo nem obstáculo
                }
            }
            else
            {
                endPoint = pontoDeVisao.position + direcaoRaio * enemyData.raioVisao;
                Gizmos.color = Color.yellow; // Amarelo para visão livre
            }

            Gizmos.DrawLine(pontoDeVisao.position, endPoint);
        }
    }
}