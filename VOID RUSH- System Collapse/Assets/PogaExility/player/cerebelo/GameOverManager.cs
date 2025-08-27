using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("Arraste o objeto do jogador que cont�m o PlayerStats.")]
    public PlayerStats playerStats;

    [Tooltip("Arraste um objeto de Imagem da UI que cobrir� a tela (deve ser preto).")]
    public Image fadePanel;

    [Tooltip("Arraste o Animator do jogador aqui.")]
    public Animator playerAnimator; // Refer�ncia para o Animator do jogador

    [Header("Configura��es de Fade")]
    [Tooltip("A velocidade com que a tela escurece e clareia.")]
    public float fadeSpeed = 0.5f;

    // Garante que a l�gica n�o seja acionada m�ltiplas vezes
    private bool isPlayerDead = false;

    void Awake()
    {
        // Garante que o painel comece transparente e desativado para n�o bloquear cliques
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
            // Come�a a ouvir o evento de morte do jogador
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
        // Se o jogador j� est� no processo de morrer, n�o faz nada.
        if (isPlayerDead) return;

        isPlayerDead = true;

        // Inicia a coroutine que controla a sequ�ncia de eventos
        StartCoroutine(GameOverSequenceCoroutine());
    }

    private IEnumerator GameOverSequenceCoroutine()
    {
        // --- 1. Anima��o de Morte ---
        // (Assumindo que voc� tem um gatilho chamado "Death" no seu Animator)
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Death");
        }

        // Desativa o controle do jogador para que ele n�o possa se mover enquanto morre
        // (Voc� pode precisar adaptar esta linha para o seu PlayerController)
        playerStats.GetComponent<PlayerController>().enabled = false;

        // Espera um tempo para a anima��o de morte tocar.
        // Ajuste este valor para corresponder � dura��o da sua anima��o.
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

        // --- 5. Finaliza��o ---
        // Devolve o controle ao jogador
        playerStats.GetComponent<PlayerController>().enabled = true;
        isPlayerDead = false;
    }

    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.unscaledDeltaTime; // Usa tempo n�o escalado para funcionar mesmo se o jogo pausar
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