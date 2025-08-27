using UnityEngine;
using UnityEngine.UI; // Necessário para interagir com componentes de UI como Image e Text
using System.Collections;
using TMPro; // Necessário para usar Coroutines

public class HealthBar : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste o objeto do jogador que contém o script PlayerStats.")]
    public PlayerStats playerStats;

    [Tooltip("Arraste o componente Image que representa o preenchimento (fill) da barra de vida.")]
    public Image healthBarFill;

    [Tooltip("Arraste o componente Text (opcional) que mostra os valores numéricos (ex: 100/100).")]
    public TextMeshPro healthText;

    [Header("Configurações de Animação")]
    [Tooltip("A velocidade com que a barra de vida se move. Valores maiores são mais rápidos.")]
    public float updateSpeed = 0.5f;

    private Coroutine healthUpdateCoroutine;

    // 'OnEnable' é chamado quando o objeto é ativado. Ideal para "assinar" eventos.
    private void OnEnable()
    {
        if (playerStats != null)
        {
            // Começa a "ouvir" o evento de mudança de vida do PlayerStats.
            // Quando OnHealthChanged for disparado, a nossa função HandleHealthChanged será chamada.
            playerStats.OnHealthChanged += HandleHealthChanged;

            // Sincroniza a barra de vida com os valores atuais do jogador assim que o jogo começa.
            HandleHealthChanged(playerStats.MaxHealth, playerStats.MaxHealth); // Começa com a vida cheia
        }
        else
        {
            Debug.LogWarning("HealthBar: A referência ao PlayerStats não foi definida no Inspector!");
        }
    }

    // 'OnDisable' é chamado quando o objeto é desativado. Ideal para "cancelar a assinatura" de eventos.
    private void OnDisable()
    {
        if (playerStats != null)
        {
            // Para de "ouvir" o evento para evitar erros quando o objeto é destruído.
            playerStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    // Esta função é o "receptor" do evento do PlayerStats.
    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        // Calcula o novo valor de preenchimento (um número entre 0 e 1).
        float targetFillAmount = currentHealth / maxHealth;

        // Se já houver uma animação de vida rodando, paramos ela para começar uma nova.
        if (healthUpdateCoroutine != null)
        {
            StopCoroutine(healthUpdateCoroutine);
        }

        // Inicia a nova animação suave (Lerp) para atualizar a barra.
        healthUpdateCoroutine = StartCoroutine(AnimateHealthChange(targetFillAmount));

        // Atualiza o texto, se ele existir.
        if (healthText != null)
        {
            // Usamos Mathf.CeilToInt para arredondar a vida para cima, evitando "99.9"
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    // Uma Coroutine permite que a animação aconteça ao longo de vários frames, em vez de ser instantânea.
    private IEnumerator AnimateHealthChange(float targetFillAmount)
    {
        float initialFillAmount = healthBarFill.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < updateSpeed)
        {
            elapsedTime += Time.deltaTime;
            // 'Lerp' interpola suavemente entre o valor inicial e o valor alvo.
            healthBarFill.fillAmount = Mathf.Lerp(initialFillAmount, targetFillAmount, elapsedTime / updateSpeed);
            yield return null; // Espera até o próximo frame.
        }

        // Garante que a barra chegue exatamente no valor final, evitando pequenas imprecisões do Lerp.
        healthBarFill.fillAmount = targetFillAmount;
    }
}