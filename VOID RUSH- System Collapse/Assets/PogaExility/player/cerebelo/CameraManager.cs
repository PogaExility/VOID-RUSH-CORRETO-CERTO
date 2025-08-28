using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Configuração")]
    [Tooltip("Arraste o PREFAB da sua Main Camera da pasta de Assets para cá.")]
    public GameObject cameraPrefab;

    // Esta é a referência pública que outros scripts usarão para encontrar a câmera.
    public Camera MainCameraInstance { get; private set; }

    void Awake()
    {
        // Padrão Singleton para garantir que este manager seja único e persistente
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
        // 1. Procura por uma câmera principal que já exista
        if (Camera.main != null)
        {
            MainCameraInstance = Camera.main;
        }
        else
        {
            // 2. Se não encontrou, e nós temos um prefab...
            if (cameraPrefab != null)
            {
                // ...então a criamos a partir do prefab.
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

        // 3. Agora que temos uma câmera, conectamos o CameraFollow a ela
        CameraFollow followScript = MainCameraInstance.GetComponent<CameraFollow>();
        if (followScript == null)
        {
            // Adiciona o script se ele não existir
            followScript = MainCameraInstance.gameObject.AddComponent<CameraFollow>();
        }

        Debug.Log("CameraManager: Sistema de Câmera pronto.");
    }
}