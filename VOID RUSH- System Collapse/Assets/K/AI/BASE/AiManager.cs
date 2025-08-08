using UnityEngine;

// O AIManager é o cérebro central que observa o mundo e fornece informações para todos os inimigos.
// Ele usa o padrão Singleton para garantir que só exista um dele e para ser facilmente acessível.
public class AIManager : MonoBehaviour
{
    // --- Padrão Singleton ---
    // A propriedade estática 'Instance' é o nosso ponto de acesso global.
    public static AIManager Instance { get; private set; }

    // --- Referências Globais ---
    // Armazena a referência ao Transform do jogador.
    [Tooltip("Referência ao transform do jogador. Preenchida automaticamente se a tag 'Player' existir.")]
    public Transform playerTarget;

    private void Awake()
    {
        // Lógica do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Garante que só haja uma instância.
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Descomente se precisar que ele persista entre cenas.
        }
    }

    private void Start()
    {
        // Se a referência do jogador não foi definida manualmente, procure-a pela tag.
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