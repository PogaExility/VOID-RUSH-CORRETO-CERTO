using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para carregar novas cenas

public class PauseMenuVD : MonoBehaviour
{
    [Header("Refer�ncias dos Pain�is")]
    [Tooltip("Arraste aqui o painel principal do menu de pause.")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Arraste aqui o seu painel de configura��es (o prefab).")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Configura��o de Cenas")]
    [Tooltip("Digite o nome exato da sua cena do Menu Principal.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Mude "MainMenu" para o nome real da sua cena

    // Vari�vel para controlar se o jogo est� pausado ou n�o
    private bool isPaused = false;

    void Start()
    {
        // Garante que ambos os pain�is comecem desativados e o jogo rodando normalmente
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
                // Se j� est� pausado, despausa
                Resume();
            }
            else
            {
                // Se n�o est� pausado, pausa
                Pause();
            }
        }
    }

    // --- FUN��ES DE CONTROLE DO JOGO ---

    public void Pause()
    {
        isPaused = true;
        // Mostra o painel de pause
        pauseMenuPanel.SetActive(true);
        // Garante que o painel de configura��es esteja escondido ao pausar
        settingsPanel.SetActive(false);
        // Congela o tempo do jogo
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        // Esconde TODOS os pain�is do menu
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        // Restaura o tempo do jogo
        Time.timeScale = 1f;
    }

    // --- FUN��ES PARA OS BOT�ES ---

    public void OpenSettings()
    {
        // Esconde o painel principal de pause
        pauseMenuPanel.SetActive(false);
        // Mostra o painel de configura��es
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        // Esconde o painel de configura��es
        settingsPanel.SetActive(false);
        // Mostra o painel principal de pause de volta
        pauseMenuPanel.SetActive(true);
    }

    public void LoadMainMenu()
    {
        // � uma boa pr�tica garantir que o tempo volte ao normal antes de sair da cena
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}