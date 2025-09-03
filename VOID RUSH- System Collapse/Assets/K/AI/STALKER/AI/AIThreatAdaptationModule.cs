using UnityEngine;
using System.Collections.Generic;

public class AIThreatAdaptationModule : MonoBehaviour
{
    // Classe para armazenar as tendências de comportamento da IA
    public class AIBiases
    {
        public float aggression = 0.5f; // 0 = passivo, 1 = agressivo
        public float coverPreference = 0.3f; // 0 = nunca usa cover, 1 = sempre tenta usar
        public float preferredEngagementDistance = 10f;
    }

    public AIBiases Biases { get; private set; }

    // Dicionário para registrar ações do jogador
    private Dictionary<string, int> _playerActionLog = new Dictionary<string, int>();
    private float _analysisTimer = 0f;
    private const float ANALYSIS_INTERVAL = 10f; // Analisa o comportamento do jogador a cada 10s

    void Awake()
    {
        Biases = new AIBiases();
        // Inicializa o log de ações
        _playerActionLog["RangedAttack"] = 0;
        _playerActionLog["MeleeAttack"] = 0;
        _playerActionLog["UsedCover"] = 0;
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

    /// <summary>
    /// Outros sistemas chamam esta função para registrar as ações do jogador.
    /// </summary>
    public void LogPlayerAction(string actionKey)
    {
        if (_playerActionLog.ContainsKey(actionKey))
        {
            _playerActionLog[actionKey]++;
        }
    }

    private void AnalyzeAndAdapt()
    {
        Debug.Log("[ADAPTATION] Analisando perfil do jogador...");

        int totalRanged = _playerActionLog["RangedAttack"];
        int totalMelee = _playerActionLog["MeleeAttack"];

        // Se o jogador ataca mais de longe, a IA fica mais cautelosa e prefere cobertura.
        if (totalRanged > totalMelee * 1.5f)
        {
            Biases.coverPreference = Mathf.Clamp(Biases.coverPreference + 0.1f, 0.1f, 0.9f);
            Biases.aggression = Mathf.Clamp(Biases.aggression - 0.05f, 0.2f, 1f);
            Debug.Log("[ADAPTATION] Perfil: Sniper. Aumentando preferência por cobertura.");
        }
        // Se o jogador é mais agressivo, a IA também se torna mais agressiva.
        else if (totalMelee > totalRanged * 1.5f)
        {
            Biases.aggression = Mathf.Clamp(Biases.aggression + 0.1f, 0.2f, 1f);
            Biases.coverPreference = Mathf.Clamp(Biases.coverPreference - 0.05f, 0.1f, 0.9f);
            Debug.Log("[ADAPTATION] Perfil: Rusher. Aumentando agressividade.");
        }

        // Reseta o log para o próximo ciclo de análise
        _playerActionLog["RangedAttack"] = 0;
        _playerActionLog["MeleeAttack"] = 0;
        _playerActionLog["UsedCover"] = 0;
    }
}