using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class DefenseHandler : MonoBehaviour
{
    [Header("Referências")]
    private PlayerStats _playerStats;

    [Header("Configuração de Parry")]
    public float parryWindow = 0.2f;

    private bool _isBlocking = false;
    private bool _canParry = false;
    private float _blockStartTime;
    public bool IsInParryWindow()
    {
        // Retorna true apenas se o jogador estiver bloqueando E dentro da janela de tempo.
        return _isBlocking && Time.time - _blockStartTime <= parryWindow;
    }

    void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    public void StartBlock()
    {
        if (_isBlocking) return;
        _isBlocking = true;
        _canParry = true;
        _blockStartTime = Time.time;
        Debug.Log("Bloqueio Ativo! Janela de Parry aberta.");
    }

    public void EndBlock()
    {
        if (!_isBlocking) return;
        _isBlocking = false;
        _canParry = false;
        Debug.Log("Bloqueio Terminou.");
    }

    public void OnTakeDamage(float damageAmount)
    {
        if (!_isBlocking) return;

        if (_canParry && Time.time - _blockStartTime <= parryWindow)
        {
            PerformParry();
        }
        else
        {
            PerformBlock(damageAmount);
        }
    }

    private void PerformParry()
    {
        Debug.Log("PARPY! Dano negado!");
        _playerStats.RecoverBlockGauge(1000);
        _canParry = false;
    }

    private void PerformBlock(float damageAmount)
    {
        Debug.Log("Dano bloqueado!");
        _playerStats.DrainBlockGauge(damageAmount);
    }

    // ====================================================================
    // FUNÇÃO DE ESTADO ADICIONADA
    // ====================================================================
    public bool IsBlocking() => _isBlocking;
}