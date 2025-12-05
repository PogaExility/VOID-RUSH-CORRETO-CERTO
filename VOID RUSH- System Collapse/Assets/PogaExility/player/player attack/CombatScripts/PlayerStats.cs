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
    public bool IsDead()
    {
        return _currentHealth <= 0;
    }


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
    [Header("Física de Combate")]
    [Tooltip("A capacidade do jogador de resistir a repulsão. Subtraído da 'Força' de um ataque recebido.")]
    public float knockbackResistance = 5f;


    public float MaxHealth => baseMaxHealth + _healthBonus;
    public float MaxBlockGauge => baseMaxBlockGauge + _blockGaugeBonus;

    // ADIÇÃO AQUI: Propriedade pública para ler a variável privada _currentHealth
    public float CurrentHealth => _currentHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnBlockGaugeChanged;
    public event Action OnDeath;

    private AdvancedPlayerMovement2D movementScript;

    private PlayerSounds playerSounds;

    void Awake()
    {
        movementScript = GetComponent<AdvancedPlayerMovement2D>();

        // --- ADIÇÃO: Cache do PlayerSounds ---
        playerSounds = GetComponent<PlayerSounds>();
        if (playerSounds == null)
        {
            // Tenta achar nos filhos caso a estrutura seja diferente
            playerSounds = GetComponentInChildren<PlayerSounds>();
        }

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
        animatorController = GetComponent<PlayerAnimatorController>();
        OnDeath += PlayDeathAnimation;
    }
    public void TakeDamage(float amount, Vector2 attackDirection)
    {
        // Chama a função principal de dano, passando 0 como força de knockback padrão.
        TakeDamage(amount, attackDirection, 0f);
    }
    public void TakeDamage(float amount, Vector2 attackDirection, float incomingKnockbackPower)
    {
        // 1. Checagem de Invencibilidade
        if (isInvincible) return;

        // 2. Checagem de Dash
        if (movementScript != null && (movementScript.IsDashing() || movementScript.IsWallDashing()))
        {
            return;
        }

        // 3. Aplica o Dano
        _currentHealth -= amount;

        // 4. Trava de segurança
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
        }

        // --- TOCA O SOM DE DANO ---
        // Agora usa a variável cached, que é mais garantida
        if (playerSounds != null)
        {
            playerSounds.PlayDamageSound();
        }
        else
        {
            // Debug de segurança caso o som não saia
            Debug.LogWarning("PlayerStats: Não foi possível tocar o som de dano. PlayerSounds não encontrado.");
        }
        // --------------------------------

        // 5. Notifica a UI
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

        // 6. Sobreviveu ou Morreu?
        if (_currentHealth > 0)
        {
            float finalForce = incomingKnockbackPower - knockbackResistance;

            if (finalForce > 0)
            {
                movementScript.ExecuteKnockback(finalForce, attackDirection);
            }

            StartCoroutine(InvincibilityCoroutine());
        }
        else
        {
            OnDeath?.Invoke();
        }
    }
    private void PlayDeathAnimation()
    {
       // animatorController.PlayState(PlayerAnimState.morrendo);
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
 
    public bool IsHealthLow()
    {
        // Retorna verdadeiro se a vida atual for menor ou igual a 25% (ou o que você configurou)
        return (_currentHealth / MaxHealth) <= lowHealthThreshold;
    }

    internal void TakeDamage(float danoAoContato)
    {
        throw new NotImplementedException();
    }
}