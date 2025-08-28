using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Configura��o")]
    [Tooltip("Arraste o PREFAB da sua Main Camera da pasta de Assets para c�.")]
    public GameObject cameraPrefab;

    // Esta � a refer�ncia p�blica que outros scripts usar�o para encontrar a c�mera.
    public Camera MainCameraInstance { get; private set; }

    void Awake()
    {
        // Padr�o Singleton para garantir que este manager seja �nico e persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Chamado automaticamente toda vez que uma nova cena termina de carregar
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Procura por uma c�mera principal que j� exista
        if (Camera.main != null)
        {
            MainCameraInstance = Camera.main;
        }
        else
        {
            // 2. Se n�o encontrou, e n�s temos um prefab...
            if (cameraPrefab != null)
            {
                // ...ent�o a criamos a partir do prefab.
                GameObject camInstance = Instantiate(cameraPrefab);
                MainCameraInstance = camInstance.GetComponent<Camera>();
                Debug.Log("CameraManager: Nenhuma Main Camera encontrada. Uma nova foi criada a partir do prefab.");
            }
            else
            {
                Debug.LogError("CameraManager: Nenhuma Main Camera foi encontrada na cena e nenhum prefab foi fornecido!");
                return;
            }
        }

        // 3. Agora que temos uma c�mera, conectamos o CameraFollow a ela
        CameraFollow followScript = MainCameraInstance.GetComponent<CameraFollow>();
        if (followScript == null)
        {
            // Adiciona o script se ele n�o existir
            followScript = MainCameraInstance.gameObject.AddComponent<CameraFollow>();
        }

        Debug.Log("CameraManager: Sistema de C�mera pronto.");
    }
}