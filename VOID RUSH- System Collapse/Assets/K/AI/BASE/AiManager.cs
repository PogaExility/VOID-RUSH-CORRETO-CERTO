using UnityEngine;

// O AIManager � o c�rebro central que observa o mundo e fornece informa��es para todos os inimigos.
// Ele usa o padr�o Singleton para garantir que s� exista um dele e para ser facilmente acess�vel.
public class AIManager : MonoBehaviour
{
    // --- Padr�o Singleton ---
    // A propriedade est�tica 'Instance' � o nosso ponto de acesso global.
    public static AIManager Instance { get; private set; }

    // --- Refer�ncias Globais ---
    // Armazena a refer�ncia ao Transform do jogador.
    [Tooltip("Refer�ncia ao transform do jogador. Preenchida automaticamente se a tag 'Player' existir.")]
    public Transform playerTarget;

    private void Awake()
    {
        // L�gica do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Garante que s� haja uma inst�ncia.
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Descomente se precisar que ele persista entre cenas.
        }
    }

    private void Start()
    {
        // Se a refer�ncia do jogador n�o foi definida manualmente, procure-a pela tag.
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
                Debug.LogError("ERRO CR�TICO: O AIManager n�o conseguiu encontrar nenhum objeto com a tag 'Player'. A IA n�o funcionar�!");
            }
        }
    }
}