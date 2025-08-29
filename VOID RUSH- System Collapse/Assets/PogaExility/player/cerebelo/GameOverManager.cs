using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Referências")]
    public PlayerStats playerStats;
    public Image fadePanel;
    public Animator playerAnimator;

    [Header("Configurações de Fade")]
    public float fadeSpeed = 0.5f;

    private bool isPlayerDead = false;

    void Awake()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(false);
            fadePanel.color = new Color(0, 0, 0, 0);
        }
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnDeath += StartGameOverSequence;
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnDeath -= StartGameOverSequence;
        }
    }

    private void StartGameOverSequence()
    {
        if (isPlayerDead) return;
        StartCoroutine(GameOverSequenceCoroutine());
    }

    private IEnumerator GameOverSequenceCoroutine()
    {
        isPlayerDead = true;

        // --- A CORREÇÃO DA VARIÁVEL DUPLICADA ---
        // Pegamos as referências UMA VEZ no início da corotina.
        var movementScript = playerStats.GetComponent<AdvancedPlayerMovement2D>();
        var playerController = playerStats.GetComponent<PlayerController>();

        if (movementScript != null) movementScript.Freeze();
        if (playerController != null) playerController.enabled = false;
        if (playerAnimator != null) playerAnimator.SetTrigger("Death");

        yield return new WaitForSeconds(1.5f);

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadeToBlack());
        }

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RespawnPlayer(playerStats.transform);
        }

        playerStats.Heal(playerStats.MaxHealth);

        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeToTransparent());
            fadePanel.gameObject.SetActive(false);
        }

        // Reativamos tudo usando as mesmas referências
        if (movementScript != null) movementScript.Unfreeze();
        if (playerController != null) playerController.enabled = true;

        isPlayerDead = false;
    }

    // --- AS FUNÇÕES DE FADE, AGORA DENTRO DA CLASSE ---
    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        Color panelColor = new Color(0, 0, 0, 0);
        fadePanel.color = panelColor;
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.unscaledDeltaTime;
            panelColor.a = Mathf.Clamp01(elapsedTime / fadeSpeed);
            fadePanel.color = panelColor;
            yield return null;
        }
        panelColor.a = 1f;
        fadePanel.color = panelColor;
    }

    private IEnumerator FadeToTransparent()
    {
        float elapsedTime = 0f;
        Color panelColor = Color.black;
        fadePanel.color = panelColor;
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.unscaledDeltaTime;
            panelColor.a = 1f - Mathf.Clamp01(elapsedTime / fadeSpeed);
            fadePanel.color = panelColor;
            yield return null;
        }
        panelColor.a = 0f;
        fadePanel.color = panelColor;
    }
}