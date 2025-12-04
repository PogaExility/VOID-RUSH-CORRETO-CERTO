using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneGoalManagerVD : MonoBehaviour
{
    public static SceneGoalManagerVD Instance { get; private set; }

    [Header("Configuração do Objetivo")]
    [SerializeField] private int inimigosParaDerrotar;
    [SerializeField] private string proximaCena;

    // --- MODIFICADO: A referência da UI agora é privada ---
    private TextMeshProUGUI placarTexto;

    private int inimigosDerrotados = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // --- ADIÇÃO: Lógica para encontrar a UI dinamicamente ---
        // Procura por um GameObject na cena com a tag específica.
        GameObject placarObject = GameObject.FindGameObjectWithTag("PlacarInimigosUI");

        if (placarObject != null)
        {
            // Se encontrou o objeto, pega o componente de texto dele.
            placarTexto = placarObject.GetComponent<TextMeshProUGUI>();
        }

        // Se, mesmo depois da busca, não encontrou, avisa no console.
        if (placarTexto == null)
        {
            Debug.LogWarning("SceneGoalManager não conseguiu encontrar o TextMeshProUGUI com a tag 'PlacarInimigosUI'. O placar não será atualizado.");
        }
        // --- FIM DA ADIÇÃO ---

        inimigosDerrotados = 0;
        AtualizarPlacar();
    }

    public void OnEnemyDefeated()
    {
        inimigosDerrotados++;
        AtualizarPlacar();

        if (inimigosDerrotados >= inimigosParaDerrotar)
        {
            Debug.Log("Objetivo da cena concluído! Carregando a próxima cena...");
            CarregarProximaCena();
        }
    }

    private void AtualizarPlacar()
    {
        if (placarTexto != null)
        {
            placarTexto.text = $"Inimigos: {inimigosDerrotados} / {inimigosParaDerrotar}";
        }
    }

    private void CarregarProximaCena()
    {
        SceneManager.LoadScene(proximaCena);
    }
}