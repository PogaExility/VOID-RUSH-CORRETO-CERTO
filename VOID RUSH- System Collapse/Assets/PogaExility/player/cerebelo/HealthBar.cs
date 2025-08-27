using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // MUDAN�A 1: Adicionamos o namespace do TextMesh Pro. � essencial.

public class HealthBar : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("Arraste o objeto do jogador que cont�m o script PlayerStats.")]
    public PlayerStats playerStats;

    [Tooltip("Arraste o componente Image que representa o preenchimento (fill) da barra de vida.")]
    public Image healthBarFill;

    // MUDAN�A 2: Trocamos o tipo da vari�vel de 'Text' para 'TextMeshProUGUI'.
    [Tooltip("Arraste o componente TextMeshPro - UI que mostra os valores num�ricos (ex: 100/100).")]
    public TextMeshProUGUI healthText;

    [Header("Configura��es de Anima��o")]
    [Tooltip("A velocidade com que a barra de vida se move. Valores maiores s�o mais r�pidos.")]
    public float updateSpeed = 0.5f;

    private Coroutine healthUpdateCoroutine;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += HandleHealthChanged;

            // Sincroniza a barra de vida com os valores atuais no in�cio.
            // Acessamos a propriedade p�blica _currentHealth diretamente se precisarmos do valor inicial.
            // Vamos assumir que come�a cheio para simplificar a primeira chamada.
            HandleHealthChanged(playerStats.MaxHealth, playerStats.MaxHealth);
        }
        else
        {
            Debug.LogWarning("HealthBar: A refer�ncia ao PlayerStats n�o foi definida no Inspector!");
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        float targetFillAmount = currentHealth / maxHealth;

        if (healthUpdateCoroutine != null)
        {
            StopCoroutine(healthUpdateCoroutine);
        }

        healthUpdateCoroutine = StartCoroutine(AnimateHealthChange(targetFillAmount));

        if (healthText != null)
        {
            // A sintaxe para mudar o texto � a mesma.
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    private IEnumerator AnimateHealthChange(float targetFillAmount)
    {
        float initialFillAmount = healthBarFill.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < updateSpeed)
        {
            elapsedTime += Time.deltaTime;
            healthBarFill.fillAmount = Mathf.Lerp(initialFillAmount, targetFillAmount, elapsedTime / updateSpeed);
            yield return null;
        }

        healthBarFill.fillAmount = targetFillAmount;
    }
}