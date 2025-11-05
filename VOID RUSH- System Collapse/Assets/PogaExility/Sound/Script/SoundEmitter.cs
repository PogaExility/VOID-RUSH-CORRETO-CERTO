using UnityEngine;

// Garante que o objeto tenha um SphereCollider para ser detectado pela IA.
[RequireComponent(typeof(SphereCollider))]
public class SoundEmitter : MonoBehaviour
{
    [Header("Configurações da Onda Sonora")]
    [Tooltip("O raio máximo que o som alcançará.")]
    [SerializeField] private float maxRadius = 10f;

    [Tooltip("A velocidade com que o raio do som se expande (metros por segundo).")]
    [SerializeField] private float expansionSpeed = 15f;

    [Tooltip("O tempo em segundos que o emissor permanecerá no mundo antes de se autodestruir.")]
    [SerializeField] private float duration = 2f;

    // Referência para o nosso colisor.
    private SphereCollider soundCollider;
    // Cronômetro para a autodestruição.
    private float lifeTimer;

    void Awake()
    {
        // Pega a referência do SphereCollider.
        soundCollider = GetComponent<SphereCollider>();
        // Garante que o colisor seja um Trigger, para que ele possa detectar IAs sem barrá-las fisicamente.
        soundCollider.isTrigger = true;
        // O som começa com um raio mínimo.
        soundCollider.radius = 0f;
    }

    void Start()
    {
        // Inicia o cronômetro para a duração do som.
        lifeTimer = duration;
    }

    void Update()
    {
        // --- Lógica de Expansão ---
        // Se o raio do colisor ainda não atingiu o máximo...
        if (soundCollider.radius < maxRadius)
        {
            // ...aumenta o raio com base na velocidade de expansão e no tempo.
            soundCollider.radius += expansionSpeed * Time.deltaTime;
        }

        // Garante que o raio não ultrapasse o valor máximo.
        soundCollider.radius = Mathf.Min(soundCollider.radius, maxRadius);

        // --- Lógica de Duração e Autodestruição ---
        // Diminui o cronômetro.
        lifeTimer -= Time.deltaTime;

        // Se o tempo de vida acabou...
        if (lifeTimer <= 0f)
        {
            // ...destrói o objeto, limpando a cena.
            Destroy(gameObject);
        }
    }

    // --- Visualização no Editor ---
    // Esta função desenha coisas na janela "Scene" da Unity, mas não aparece no jogo final.
    // É extremamente útil para depurar e visualizar o alcance do som.
    void OnDrawGizmos()
    {
        // Define a cor do Gizmo (o desenho).
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Laranja semitransparente

        // Se o SphereCollider já existe, desenha uma esfera de arame com o raio atual.
        if (soundCollider != null)
        {
            Gizmos.DrawWireSphere(transform.position, soundCollider.radius);
        }
        else // Se o jogo não está rodando, desenha com o raio máximo para termos uma referência.
        {
            Gizmos.DrawWireSphere(transform.position, maxRadius);
        }
    }
}