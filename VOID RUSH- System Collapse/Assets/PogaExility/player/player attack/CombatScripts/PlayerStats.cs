using UnityEngine;
using System; // Necess�rio para usar 'Action'

public class PlayerStats : MonoBehaviour
{
    [Header("Configura��es de Vida")]
    [Tooltip("A vida m�xima base do jogador, sem contar b�nus.")]
    public float baseMaxHealth = 100f;
    private float _currentHealth;

    [Header("Configura��es de Defesa (Block)")]
    [Tooltip("A 'stamina' m�xima da defesa, sem contar b�nus.")]
    public float baseMaxBlockGauge = 100f;
    private float _currentBlockGauge;

    // --- B�nus (ser�o modificados por outros sistemas como a Skill Tree) ---
    private float _healthBonus = 0f;
    private float _blockGaugeBonus = 0f;

    // --- Propriedades Finais (Calculadas em tempo real) ---
    // O resto do jogo vai ler estes valores para ter sempre o status atualizado.
    public float MaxHealth => baseMaxHealth + _healthBonus;
    public float MaxBlockGauge => baseMaxBlockGauge + _blockGaugeBonus;

    // --- Eventos para a UI e outros sistemas ---
    // A UI da barra de vida vai "ouvir" este evento para se atualizar.
    public event Action<float, float> OnHealthChanged; // (vidaAtual, vidaMaxima)
    // A UI da barra de defesa vai "ouvir" este evento.
    public event Action<float, float> OnBlockGaugeChanged; // (defesaAtual, defesaMaxima)
    // O sistema de 'Game Over' vai "ouvir" este evento.
    public event Action OnDeath;

    // 'Awake' � chamado antes de 'Start'. Ideal para inicializar valores.
    void Awake()
    {
        // Define a vida e a defesa iniciais como o m�ximo.
        _currentHealth = MaxHealth;
        _currentBlockGauge = MaxBlockGauge;
    }

    // --- Fun��es P�blicas para Intera��o ---

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth < 0) _currentHealth = 0;

        // Avisa os "ouvintes" (como a UI) que a vida mudou.
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

        if (_currentHealth <= 0)
        {
            // Avisa os "ouvintes" que o jogador morreu.
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;
        if (_currentHealth > MaxHealth) _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
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

    // --- Fun��es para a Skill Tree e Buffs ---

    public void UpdateHealthBonus(float bonusAmount)
    {
        _healthBonus = bonusAmount;
        // Garante que a vida atual n�o ultrapasse o novo m�ximo.
        if (_currentHealth > MaxHealth) _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }

    public void UpdateBlockGaugeBonus(float bonusAmount)
    {
        _blockGaugeBonus = bonusAmount;
        if (_currentBlockGauge > MaxBlockGauge) _currentBlockGauge = MaxBlockGauge;
        OnBlockGaugeChanged?.Invoke(_currentBlockGauge, MaxBlockGauge);
    }
}