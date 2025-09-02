using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float baseMaxHealth = 100f;
    private float _currentHealth;

    [Header("Configurações de Defesa")]
    public float baseMaxBlockGauge = 100f;
    private float _currentBlockGauge;
    public float lowHealthThreshold = 0.25f; // 25% da vida máxima
    private PlayerAnimatorController animatorController;

    private float _healthBonus = 0f;
    private float _blockGaugeBonus = 0f;

    // --- ESTE É O ÚNICO BLOCO QUE DEVE EXISTIR ---
    [Header("Efeitos de Dano e Invencibilidade")]
    [Tooltip("Arraste o painel da UI (Image) que piscará em vermelho.")]
    public Image damageFlashImage;
    [Tooltip("A duração do flash vermelho na tela.")]
    public float flashDuration = 0.1f;
    [Tooltip("A duração total da invencibilidade em segundos.")]
    public float invincibilityDuration = 1.5f;
    [Tooltip("O SpriteRenderer do jogador que irá piscar.")]
    public SpriteRenderer playerSprite;
    [Tooltip("A velocidade do pisca-pisca (valores maiores = mais rápido).")]
    public float flashSpeed = 10f;

    private bool isInvincible = false;

    public float MaxHealth => baseMaxHealth + _healthBonus;
    public float MaxBlockGauge => baseMaxBlockGauge + _blockGaugeBonus;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnBlockGaugeChanged;
    public event Action OnDeath;

    private AdvancedPlayerMovement2D movementScript;

    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>();

        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
            if (playerSprite == null)
            {
                playerSprite = GetComponentInChildren<SpriteRenderer>();
            }
        }


        _currentHealth = MaxHealth;
        _currentBlockGauge = MaxBlockGauge;
        // ...
        animatorController = GetComponent<PlayerAnimatorController>();
        OnDeath += PlayDeathAnimation; 
    }

    public void TakeDamage(float amount, Vector2 attackDirection)
    {
        if (isInvincible) return;

        _currentHealth -= amount;
        if (_currentHealth < 0) _currentHealth = 0;

        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

        if (_currentHealth > 0)
        {
            animatorController.PlayState(PlayerAnimState.dano);
            movementScript.ApplyKnockback(attackDirection);
            StartCoroutine(InvincibilityCoroutine());
        }
        else
        {
            OnDeath?.Invoke();
        }
    }

    private void PlayDeathAnimation()
    {
        animatorController.PlayState(PlayerAnimState.morrendo);
        // Opcional: Desativar controles do jogador aqui
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;
        if (_currentHealth > MaxHealth) _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        if (damageFlashImage != null)
        {
            damageFlashImage.color = new Color(1, 0, 0, 0.4f);
            yield return new WaitForSeconds(flashDuration);
            damageFlashImage.color = new Color(1, 0, 0, 0);
        }

        if (playerSprite != null)
        {
            float endTime = Time.time + invincibilityDuration;
            Color originalColor = playerSprite.color;

            while (Time.time < endTime)
            {
                float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f * 0.7f + 0.3f;
                playerSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            playerSprite.color = originalColor;
        }

        isInvincible = false;
    }

    public void Update()
    {
        CheckLowHealthAnimation();
    }
    public void DrainBlockGauge(float amount)
    {
        _currentBlockGauge -= amount;
        if (_currentBlockGauge < 0) _currentBlockGauge = 0;
        OnBlockGaugeChanged?.Invoke(_currentBlockGauge, MaxBlockGauge);
    }

    public void RecoverBlockGauge(float amount)
    {
        _currentBlockGauge += amount;
        if (_currentBlockGauge > MaxBlockGauge) _currentBlockGauge = MaxBlockGauge;
        OnBlockGaugeChanged?.Invoke(_currentBlockGauge, MaxBlockGauge);
    }

    public void UpdateHealthBonus(float bonusAmount)
    {
        _healthBonus = bonusAmount;
        if (_currentHealth > MaxHealth) _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }

    public void UpdateBlockGaugeBonus(float bonusAmount)
    {
        _blockGaugeBonus = bonusAmount;
        if (_currentBlockGauge > MaxBlockGauge) _currentBlockGauge = MaxBlockGauge;
        OnBlockGaugeChanged?.Invoke(_currentBlockGauge, MaxBlockGauge);
    }
    private void CheckLowHealthAnimation()
    {
        // Condições: pouca vida, parado, no chão, e não em outro estado prioritário
        if (_currentHealth / MaxHealth <= lowHealthThreshold &&
            !GetComponent<AdvancedPlayerMovement2D>().IsMoving() &&
            GetComponent<AdvancedPlayerMovement2D>().IsGrounded() &&
            animatorController.GetCurrentAnimatorStateInfo(0).IsName("parado")) // Só substitui a animação de parado
        {
            animatorController.PlayState(PlayerAnimState.poucaVidaParado);
        }
    }
}