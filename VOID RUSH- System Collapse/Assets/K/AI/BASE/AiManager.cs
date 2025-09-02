// --- SCRIPT CANÔNICO: AIManager.cs ---
// Projeto: NEXUS Singularity Collapse
// Versão: 1.0 "O Vigia"

using UnityEngine;

/// <summary>
/// Um gerente Singleton global que fornece referências essenciais para todas as IAs na cena,
/// como o alvo do jogador. Garante eficiência ao centralizar a busca por alvos.
/// </summary>
public class AIManager : MonoBehaviour
{
    #region Padrão Singleton

    /// <summary>
    /// A instância estática e pública do AIManager, acessível de qualquer script.
    /// </summary>
    public static AIManager Instance { get; private set; }

    private void Awake()
    {
        // Implementação clássica de Singleton: garante que apenas uma instância deste objeto exista.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Múltiplas instâncias de AIManager detectadas. Destruindo a duplicata.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Opcional: descomente para fazer o AIManager persistir entre as cenas.
            // DontDestroyOnLoad(gameObject);
        }
    }

    #endregion

    #region Lógica Principal

    [Header("▶ REFERÊNCIAS GLOBAIS")]
    [Tooltip("Referência ao transform do jogador. Preenchida automaticamente se a tag 'Player' existir.")]
    public Transform playerTarget;

    private void Start()
    {
        // Se a referência do jogador não for atribuída manualmente, o AIManager a encontrará.
        if (playerTarget == null)
        {
            FindPlayerByTag();
        }
    }

    /// <summary>
    /// Procura na cena por um GameObject com a tag "Player" e armazena sua referência.
    /// </summary>
    private void FindPlayerByTag()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
            Debug.Log($"[AIManager] Alvo '{playerTarget.name}' adquirido via tag 'Player'.");
        }
        else
        {
            Debug.LogError("[AIManager] ERRO CRÍTICO: Nenhum objeto com a tag 'Player' foi encontrado na cena. A IA não terá um alvo.");
        }
    }

    #endregion
}