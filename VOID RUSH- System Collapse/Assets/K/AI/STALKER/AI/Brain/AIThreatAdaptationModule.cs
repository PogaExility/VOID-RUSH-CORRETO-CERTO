using UnityEngine;
using System.Collections.Generic;

public class AIThreatAdaptationModule : MonoBehaviour
{
    public class AIBiases
    {
        public float aggression = 0.5f;
        public float coverPreference = 0.3f;
        public float preferredEngagementDistance = 10f;
    }

    public AIBiases Biases { get; private set; }

    private Dictionary<string, int> _playerActionLog;
    private float _analysisTimer = 0f;
    private const float ANALYSIS_INTERVAL = 10f;

    void Awake()
    {
        Biases = new AIBiases();

        // CORREÇÃO: Inicializa o dicionário de forma explícita e robusta.
        InitializeActionLog();
    }

    void Update()
    {
        _analysisTimer += Time.deltaTime;
        if (_analysisTimer >= ANALYSIS_INTERVAL)
        {
            AnalyzeAndAdapt();
            _analysisTimer = 0f;
        }
    }

    public void LogPlayerAction(string actionKey)
    {
        // Esta função já é segura, pois verifica se a chave existe.
        if (_playerActionLog.ContainsKey(actionKey))
        {
            _playerActionLog[actionKey]++;
        }
    }

    private void AnalyzeAndAdapt()
    {
        Debug.Log("[ADAPTATION] Analisando perfil do jogador...");

        // CORREÇÃO: Usamos TryGetValue para obter os valores de forma segura.
        // Se a chave "RangedAttack" não existir, totalRanged será 0, em vez de causar um erro.
        _playerActionLog.TryGetValue("RangedAttack", out int totalRanged);
        _playerActionLog.TryGetValue("MeleeAttack", out int totalMelee);

        if (totalRanged > totalMelee * 1.5f)
        {
            Biases.coverPreference = Mathf.Clamp(Biases.coverPreference + 0.1f, 0.1f, 0.9f);
            Biases.aggression = Mathf.Clamp(Biases.aggression - 0.05f, 0.2f, 1f);
            Debug.Log("[ADAPTATION] Perfil: Sniper. Aumentando preferência por cobertura.");
        }
        else if (totalMelee > totalRanged * 1.5f)
        {
            Biases.aggression = Mathf.Clamp(Biases.aggression + 0.1f, 0.2f, 1f);
            Biases.coverPreference = Mathf.Clamp(Biases.coverPreference - 0.05f, 0.1f, 0.9f);
            Debug.Log("[ADAPTATION] Perfil: Rusher. Aumentando agressividade.");
        }

        // CORREÇÃO: Em vez de assumir que as chaves existem, reinicializamos o dicionário.
        // Isto limpa-o e recria-o, garantindo um estado limpo para o próximo ciclo.
        InitializeActionLog();
    }

    /// <summary>
    /// Limpa e inicializa o dicionário com os valores padrão.
    /// </summary>
    private void InitializeActionLog()
    {
        _playerActionLog = new Dictionary<string, int>
        {
            { "RangedAttack", 0 },
            { "MeleeAttack", 0 },
            { "UsedCover", 0 }
        };
    }
}