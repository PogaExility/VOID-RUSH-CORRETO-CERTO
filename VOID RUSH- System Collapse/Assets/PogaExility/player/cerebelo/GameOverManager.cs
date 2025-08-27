using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste o objeto do jogador que contém o PlayerStats.")]
    public PlayerStats playerStats;

    [Tooltip("Arraste um objeto de Imagem da UI que cobrirá a tela (deve ser preto).")]
    public Image fadePanel;

    [Tooltip("Arraste o Animator do jogador aqui.")]
    public Animator playerAnimator; // Referência para o Animator do jogador

    [Header("Configurações de Fade")]
    [Tooltip("A velocidade com que a tela escurece e clareia.")]
    public float fadeSpeed = 0.5f;

    // Garante que a lógica não seja acionada múltiplas vezes
    private bool isPlayerDead = false;

    void Awake()
    {
        // Garante que o painel comece transparente e desativado para não bloquear cliques
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(false);
            fadePanel.color = new Color(0, 0, 0, 0); // Preto e totalmente transparente
        }
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            // Começa a ouvir o evento de morte do jogador
            playerStats.OnDeath += StartGameOverSequence;
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            // Para de ouvir para evitar erros
            playerStats.OnDeath -= StartGameOverSequence;
        }
    }

    private void StartGameOverSequence()
    {
        // Se o jogador já está no processo de morrer, não faz nada.
        if (isPlayerDead) return;

        isPlayerDead = true;

        // Inicia a coroutine que controla a sequência de eventos
        StartCoroutine(GameOverSequenceCoroutine());
    }

    private IEnumerator GameOverSequenceCoroutine()
    {
        // --- 1. Animação de Morte ---
        // (Assumindo que você tem um gatilho chamado "Death" no seu Animator)
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Death");
        }

        // Desativa o controle do jogador para que ele não possa se mover enquanto morre
        // (Você pode precisar adaptar esta linha para o seu PlayerController)
        playerStats.GetComponent<PlayerController>().enabled = false;

        // Espera um tempo para a animação de morte tocar.
        // Ajuste este valor para corresponder à duração da sua animação.
        yield return new WaitForSeconds(1.5f);

        // --- 2. Fade para Preto ---
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadeToBlack());
        }

        // --- 3. Respawn do Jogador ---
        // Pede para o RespawnManager encontrar o ponto e mover o jogador
        RespawnManager.Instance.RespawnPlayer(playerStats.transform);

        // "Ressuscita" o jogador, restaurando sua vida.
        playerStats.Heal(playerStats.MaxHealth);

        // --- 4. Fade para Transparente ---
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeToTransparent());
            fadePanel.gameObject.SetActive(false);
        }

        // --- 5. Finalização ---
        // Devolve o controle ao jogador
        playerStats.GetComponent<PlayerController>().enabled = true;
        isPlayerDead = false;
    }

    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.unscaledDeltaTime; // Usa tempo não escalado para funcionar mesmo se o jogo pausar
            float alpha = Mathf.Clamp01(elapsedTime / fadeSpeed);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadePanel.color = Color.black; // Garante que fique totalmente preto
    }

    private IEnumerator FadeToTransparent()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsedTime / fadeSpeed);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 0); // Garante que fique totalmente transparente
    }
}