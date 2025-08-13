using UnityEngine;

// O AIManager � o c�rebro central que observa o mundo e fornece informa��es para todos os inimigos.
// Ele usa o padr�o Singleton para garantir que s� exista um dele e para ser facilmente acess�vel.
public class AIManager : MonoBehaviour
{
    // --- Padr�o Singleton ---
    public static AIManager Instance { get; private set; }

    // --- Refer�ncias Globais ---
    [Tooltip("Refer�ncia ao transform do jogador. Preenchida automaticamente se a tag 'Player' existir.")]
    public Transform playerTarget;

    private void Awake()
    {
        // L�gica do Singleton
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
                Debug.LogError("ERRO CR�TICO: O AIManager n�o conseguiu encontrar nenhum objeto com a tag 'Player'. A IA n�o funcionar�!");
            }
        }
    }
}