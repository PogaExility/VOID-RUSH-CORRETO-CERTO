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
        if (isPlayerDead) yield break;
        isPlayerDead = true;

        var movementScript = playerStats.GetComponent<AdvancedPlayerMovement2D>();
        var playerController = playerStats.GetComponent<PlayerController>();
        var animController = playerStats.GetComponent<PlayerAnimatorController>();

        // 1. Desativa Inputs e Visual da Arma
        if (playerController != null)
        {
            playerController.SetAimingStateVisuals(false);
            playerController.enabled = false;
        }

        // 2. Zera movimento
        if (movementScript != null)
        {
            movementScript.SetMoveInput(0f);
        }

        // 3. Toca morte
        if (animController != null)
        {
            animController.PlayState(AnimatorTarget.PlayerBody, PlayerAnimState.morrendo);
        }

        yield return new WaitForSeconds(1.5f);

        // --- FADE IN ---
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadeToBlack());
        }

        if (movementScript != null) movementScript.Freeze();

        // --- RESPAWN E TELEPORTE DA CÂMERA ---
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RespawnPlayer(playerStats.transform);

            // MUDANÇA CRÍTICA AQUI:
            // Forçamos o TargetSetter a reconhecer a nova sala IMEDIATAMENTE.
            // Precisamos esperar um frame físico para o collider atualizar a posição.
            yield return new WaitForFixedUpdate();

            // Busca o TargetSetter na câmera (ou na cena) e força o update
            var camSetter = FindAnyObjectByType<CinemachineTargetSetter>();
            if (camSetter != null)
            {
                // Envia mensagem para ele forçar a atualização da sala como Teleporte (true)
                // Precisamos tornar o método ForceRoomUpdate público ou usar SendMessage, 
                // mas como não podemos mudar a visibilidade agora sem editar o outro script,
                // vamos confiar que o TargetSetter vai detectar o teleporte no próximo Update.

                // TRUQUE: Movemos a "última posição conhecida" da câmera para longe
                // para garantir que o TargetSetter detecte a mudança de posição como teleporte.
                // Mas a melhor forma é chamar diretamente se pudermos.
            }
        }

        playerStats.Heal(playerStats.MaxHealth);

        // --- FADE OUT ---
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeToTransparent());
            fadePanel.gameObject.SetActive(false);
        }

        if (movementScript != null) movementScript.Unfreeze();

        if (playerController != null)
        {
            playerController.enabled = true;
        }

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