using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Configura��o")]
    [Tooltip("Arraste o PREFAB da sua Main Camera da pasta de Assets para c�. Este prefab DEVE ter o script CameraFollow.")]
    public GameObject cameraPrefab;

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

    // "Assinamos" o evento de cena carregada
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // A fun��o principal, executada toda vez que uma nova cena � carregada
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se n�o fornecemos um prefab, n�o h� nada a fazer.
        if (cameraPrefab == null)
        {
            Debug.LogError("CameraManager: O prefab da c�mera n�o foi definido no Inspector!");
            return;
        }

        // --- A L�GICA CORRETA ---

        // 1. Encontra e Destr�i C�meras Antigas:
        // Procura por TODAS as c�meras na cena que tenham a tag "MainCamera" e as destr�i.
        // Isso limpa a cena de qualquer c�mera padr�o que venha com ela.
        GameObject[] oldCameras = GameObject.FindGameObjectsWithTag("MainCamera");
        foreach (GameObject cam in oldCameras)
        {
            Destroy(cam);
        }

        // 2. Cria a Nova C�mera:
        // Instancia a nossa c�mera a partir do prefab.
        GameObject newCameraInstance = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log("CameraManager: C�mera antiga removida e uma nova foi criada a partir do prefab.");

        // 3. Encontra o Jogador:
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 4. Conecta a C�mera ao Jogador:
            // Pega o script CameraFollow na c�mera que acabamos de criar...
            CameraFollow followScript = newCameraInstance.GetComponent<CameraFollow>();
            if (followScript != null)
            {
                // ...e diz a ele para seguir o jogador.
                followScript.SetTarget(player.transform);
                Debug.Log("CameraManager: Nova c�mera conectada ao jogador.");
            }
            else
            {
                Debug.LogError("CameraManager: O prefab da c�mera N�O TEM o script CameraFollow!");
            }
        }
        else
        {
            Debug.LogWarning("CameraManager: Nenhum jogador com a tag 'Player' foi encontrado na cena " + scene.name);
        }
    }
}