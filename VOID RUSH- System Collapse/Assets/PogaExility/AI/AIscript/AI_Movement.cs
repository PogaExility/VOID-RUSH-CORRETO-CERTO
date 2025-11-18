using UnityEngine;

/// <summary>
/// Gerencia a movimentação física e a detecção do ambiente imediato para a IA.
/// </summary>
[RequireComponent(typeof(AI_Controller), typeof(Rigidbody2D))]
public class AI_Movement : MonoBehaviour
{
    [Header("Configuração de Detecção")]
    [Tooltip("Define quais camadas são consideradas 'Chão' para a detecção.")]
    [SerializeField] private LayerMask camadaChao;
    [Tooltip("Um objeto filho que marca a origem do raio de detecção de parede.")]
    [SerializeField] private Transform posicaoDetectorParede;
    [Tooltip("Um objeto filho que marca a origem do raio de detecção de chão.")]
    [SerializeField] private Transform posicaoDetectorChao;
    [Space]
    [Tooltip("O quão longe o raio frontal detecta uma parede.")]
    [SerializeField] private float distanciaDetectorParede = 0.5f;
    [Tooltip("O quão para baixo o raio detecta a ausência de chão.")]
    [SerializeField] private float distanciaDetectorChao = 1f;

    // --- REFERÊNCIAS DE COMPONENTES ---
    private AI_Controller aiController;
    private Rigidbody2D rb;

    // --- VARIÁVEIS DE ESTADO ---
    private float direcaoMovimento = 1f; // 1 para direita, -1 para esquerda

    private void Awake()
    {
        aiController = GetComponent<AI_Controller>();
        rb = GetComponent<Rigidbody2D>();

        // Validação para garantir que os pontos de detecção foram configurados no Inspector
        if (posicaoDetectorParede == null || posicaoDetectorChao == null)
        {
            Debug.LogError($"O inimigo '{gameObject.name}' não tem os 'Transforms' de detecção configurados no AI_Movement!", this);
            this.enabled = false; // Desativa o script para evitar erros contínuos
        }
    }

    private void FixedUpdate()
    {
        // Toda a lógica de física deve estar no FixedUpdate

        // 1. Verifica se precisa virar (encontrou parede ou beirada)
        if (PrecisaVirar())
        {
            Virar();
        }

        // 2. Aplica o movimento na direção atual
        Mover();
    }

    /// <summary>
    /// Aplica a velocidade ao Rigidbody2D para mover o personagem.
    /// </summary>
    private void Mover()
    {
        // Usa a velocidade calculada pelo AI_Controller
        rb.linearVelocity = new Vector2(direcaoMovimento * aiController.VelocidadeAtual, rb.linearVelocity.y);
    }

    /// <summary>
    /// Dispara Raycasts para verificar se há uma parede à frente ou uma beirada de plataforma.
    /// </summary>
    /// <returns>True se for necessário virar, False caso contrário.</returns>
    private bool PrecisaVirar()
    {
        // Dispara um raio para frente para detectar paredes
        bool temParedeNaFrente = Physics2D.Raycast(posicaoDetectorParede.position, Vector2.right * direcaoMovimento, distanciaDetectorParede, camadaChao);

        // Dispara um raio para baixo a partir do detector de chão
        bool temChaoNaFrente = Physics2D.Raycast(posicaoDetectorChao.position, Vector2.down, distanciaDetectorChao, camadaChao);

        // Retorna true se encontrou uma parede OU se não encontrou chão
        return temParedeNaFrente || !temChaoNaFrente;
    }

    /// <summary>
    /// Inverte a direção de movimento e a orientação visual do inimigo.
    /// </summary>
    private void Virar()
    {
        direcaoMovimento *= -1; // Inverte a direção matemática
        transform.Rotate(0f, 180f, 0f); // Gira o objeto 180 graus no eixo Y
    }

    /// <summary>
    /// Desenha os raios de detecção na Scene View para facilitar o debug e configuração.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (posicaoDetectorParede != null)
        {
            Gizmos.color = Color.red; // Raio de parede em vermelho
            Gizmos.DrawLine(posicaoDetectorParede.position, posicaoDetectorParede.position + (Vector3.right * direcaoMovimento * distanciaDetectorParede));
        }

        if (posicaoDetectorChao != null)
        {
            Gizmos.color = Color.green; // Raio de chão em verde
            Gizmos.DrawLine(posicaoDetectorChao.position, posicaoDetectorChao.position + (Vector3.down * distanciaDetectorChao));
        }
    }
}