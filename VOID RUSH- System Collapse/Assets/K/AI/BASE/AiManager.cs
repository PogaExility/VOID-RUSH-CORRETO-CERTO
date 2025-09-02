using UnityEngine;

public class AIManager : MonoBehaviour
{
    // --- Padrão Singleton ---
    public static AIManager Instance { get; private set; }

    // --- Referências Globais ---
    [Tooltip("Referência ao transform do jogador. Preenchida automaticamente se a tag 'Player' existir.")]
    public Transform playerTarget;

    private void Awake()
    {
        // Lógica do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTarget = playerObject.transform;
                Debug.Log("AIManager encontrou e travou a mira no jogador: " + playerTarget.name);
            }
            else
            {
                Debug.LogError("ERRO CRÍTICO: O AIManager não conseguiu encontrar nenhum objeto com a tag 'Player'. A IA não funcionará!");
            }
        }
    }
}