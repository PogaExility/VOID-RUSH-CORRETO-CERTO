using UnityEngine;
using UnityEngine.UI; // Necess�rio para interagir com componentes de UI como Image e Text
using System.Collections;
using TMPro; // Necess�rio para usar Coroutines

public class HealthBar : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("Arraste o objeto do jogador que cont�m o script PlayerStats.")]
    public PlayerStats playerStats;

    [Tooltip("Arraste o componente Image que representa o preenchimento (fill) da barra de vida.")]
    public Image healthBarFill;

    [Tooltip("Arraste o componente Text (opcional) que mostra os valores num�ricos (ex: 100/100).")]
    public TextMeshPro healthText;

    [Header("Configura��es de Anima��o")]
    [Tooltip("A velocidade com que a barra de vida se move. Valores maiores s�o mais r�pidos.")]
    public float updateSpeed = 0.5f;

    private Coroutine healthUpdateCoroutine;

    // 'OnEnable' � chamado quando o objeto � ativado. Ideal para "assinar" eventos.
    private void OnEnable()
    {
        if (playerStats != null)
        {
            // Come�a a "ouvir" o evento de mudan�a de vida do PlayerStats.
            // Quando OnHealthChanged for disparado, a nossa fun��o HandleHealthChanged ser� chamada.
            playerStats.OnHealthChanged += HandleHealthChanged;

            // Sincroniza a barra de vida com os valores atuais do jogador assim que o jogo come�a.
            HandleHealthChanged(playerStats.MaxHealth, playerStats.MaxHealth); // Come�a com a vida cheia
        }
        else
        {
            Debug.LogWarning("HealthBar: A refer�ncia ao PlayerStats n�o foi definida no Inspector!");
        }
    }

    // 'OnDisable' � chamado quando o objeto � desativado. Ideal para "cancelar a assinatura" de eventos.
    private void OnDisable()
    {
        if (playerStats != null)
        {
            // Para de "ouvir" o evento para evitar erros quando o objeto � destru�do.
            playerStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    // Esta fun��o � o "receptor" do evento do PlayerStats.
    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        // Calcula o novo valor de preenchimento (um n�mero entre 0 e 1).
        float targetFillAmount = currentHealth / maxHealth;

        // Se j� houver uma anima��o de vida rodando, paramos ela para come�ar uma nova.
        if (healthUpdateCoroutine != null)
        {
            StopCoroutine(healthUpdateCoroutine);
        }

        // Inicia a nova anima��o suave (Lerp) para atualizar a barra.
        healthUpdateCoroutine = StartCoroutine(AnimateHealthChange(targetFillAmount));

        // Atualiza o texto, se ele existir.
        if (healthText != null)
        {
            // Usamos Mathf.CeilToInt para arredondar a vida para cima, evitando "99.9"
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    // Uma Coroutine permite que a anima��o aconte�a ao longo de v�rios frames, em vez de ser instant�nea.
    private IEnumerator AnimateHealthChange(float targetFillAmount)
    {
        float initialFillAmount = healthBarFill.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < updateSpeed)
        {
            elapsedTime += Time.deltaTime;
            // 'Lerp' interpola suavemente entre o valor inicial e o valor alvo.
            healthBarFill.fillAmount = Mathf.Lerp(initialFillAmount, targetFillAmount, elapsedTime / updateSpeed);
            yield return null; // Espera at� o pr�ximo frame.
        }

        // Garante que a barra chegue exatamente no valor final, evitando pequenas imprecis�es do Lerp.
        healthBarFill.fillAmount = targetFillAmount;
    }
}