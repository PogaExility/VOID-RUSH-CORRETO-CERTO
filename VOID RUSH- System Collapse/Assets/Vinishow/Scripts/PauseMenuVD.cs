using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para carregar novas cenas

public class PauseMenuVD : MonoBehaviour
{
    [Header("Referências dos Painéis")]
    [Tooltip("Arraste aqui o painel principal do menu de pause.")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Arraste aqui o seu painel de configurações (o prefab).")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Configuração de Cenas")]
    [Tooltip("Digite o nome exato da sua cena do Menu Principal.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Mude "MainMenu" para o nome real da sua cena

    // Variável para controlar se o jogo está pausado ou não
    private bool isPaused = false;

    void Start()
    {
        // Garante que ambos os painéis comecem desativados e o jogo rodando normalmente
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        // "Escuta" pela tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // Se já está pausado, despausa
                Resume();
            }
            else
            {
                // Se não está pausado, pausa
                Pause();
            }
        }
    }

    // --- FUNÇÕES DE CONTROLE DO JOGO ---

    public void Pause()
    {
        isPaused = true;
        // Mostra o painel de pause
        pauseMenuPanel.SetActive(true);
        // Garante que o painel de configurações esteja escondido ao pausar
        settingsPanel.SetActive(false);
        // Congela o tempo do jogo
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        // Esconde TODOS os painéis do menu
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        // Restaura o tempo do jogo
        Time.timeScale = 1f;
    }

    // --- FUNÇÕES PARA OS BOTÕES ---

    public void OpenSettings()
    {
        // Esconde o painel principal de pause
        pauseMenuPanel.SetActive(false);
        // Mostra o painel de configurações
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        // Esconde o painel de configurações
        settingsPanel.SetActive(false);
        // Mostra o painel principal de pause de volta
        pauseMenuPanel.SetActive(true);
    }

    public void LoadMainMenu()
    {
        // É uma boa prática garantir que o tempo volte ao normal antes de sair da cena
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}