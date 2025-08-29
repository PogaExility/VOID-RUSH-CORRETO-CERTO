using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Configuração")]
    [Tooltip("Arraste o PREFAB da sua Main Camera da pasta de Assets para cá. Este prefab DEVE ter o script CameraFollow.")]
    public GameObject cameraPrefab;

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

    // "Assinamos" o evento de cena carregada
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // A função principal, executada toda vez que uma nova cena é carregada
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se não fornecemos um prefab, não há nada a fazer.
        if (cameraPrefab == null)
        {
            Debug.LogError("CameraManager: O prefab da câmera não foi definido no Inspector!");
            return;
        }

        // --- A LÓGICA CORRETA ---

        // 1. Encontra e Destrói Câmeras Antigas:
        // Procura por TODAS as câmeras na cena que tenham a tag "MainCamera" e as destrói.
        // Isso limpa a cena de qualquer câmera padrão que venha com ela.
        GameObject[] oldCameras = GameObject.FindGameObjectsWithTag("MainCamera");
        foreach (GameObject cam in oldCameras)
        {
            Destroy(cam);
        }

        // 2. Cria a Nova Câmera:
        // Instancia a nossa câmera a partir do prefab.
        GameObject newCameraInstance = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log("CameraManager: Câmera antiga removida e uma nova foi criada a partir do prefab.");

        // 3. Encontra o Jogador:
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 4. Conecta a Câmera ao Jogador:
            // Pega o script CameraFollow na câmera que acabamos de criar...
            CameraFollow followScript = newCameraInstance.GetComponent<CameraFollow>();
            if (followScript != null)
            {
                // ...e diz a ele para seguir o jogador.
                followScript.SetTarget(player.transform);
                Debug.Log("CameraManager: Nova câmera conectada ao jogador.");
            }
            else
            {
                Debug.LogError("CameraManager: O prefab da câmera NÃO TEM o script CameraFollow!");
            }
        }
        else
        {
            Debug.LogWarning("CameraManager: Nenhum jogador com a tag 'Player' foi encontrado na cena " + scene.name);
        }
    }
}