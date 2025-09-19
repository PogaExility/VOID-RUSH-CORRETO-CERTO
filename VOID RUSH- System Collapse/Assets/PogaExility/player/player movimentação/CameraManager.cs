using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Configuração dos Prefabs")]
    [Tooltip("PREFAB da Câmera Principal. DEVE conter o componente CinemachineBrain.")]
    public GameObject mainCameraPrefab;

    [Tooltip("PREFAB da Câmera Virtual. DEVE conter CinemachineVirtualCamera, Confiner2D e o script CinemachineTargetSetter.")]
    public GameObject virtualCameraPrefab;

    // Usaremos esta tag para encontrar e limpar as câmeras virtuais antigas.
    private const string VIRTUAL_CAMERA_TAG = "VirtualCamera";

    void Awake()
    {
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Validação para garantir que os prefabs foram atribuídos no Inspector.
        if (mainCameraPrefab == null || virtualCameraPrefab == null)
        {
            Debug.LogError("CameraManager: Prefabs da Main Camera ou da Virtual Camera não foram definidos no Inspector!");
            return;
        }

        // --- LÓGICA DA CÂMERA PRINCIPAL (com Brain) ---
        // Se não existir uma Main Camera, criamos uma e a tornamos persistente.
        if (GameObject.FindGameObjectWithTag("MainCamera") == null)
        {
            GameObject mainCamInstance = Instantiate(mainCameraPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(mainCamInstance);
            Debug.Log("CameraManager: Main Camera com Brain criada e marcada como persistente.");
        }

        // --- LÓGICA DA CÂMERA VIRTUAL (específica da cena) ---
        // 1. Destrói a VCam antiga, se houver.
        GameObject oldVirtualCam = GameObject.FindGameObjectWithTag(VIRTUAL_CAMERA_TAG);
        if (oldVirtualCam != null)
        {
            Destroy(oldVirtualCam);
        }

        // 2. Cria a nova VCam para a cena atual.
        GameObject newVirtualCam = Instantiate(virtualCameraPrefab, Vector3.zero, Quaternion.identity);
        // Garante que a nova VCam tenha a tag para que possamos encontrá-la na próxima cena.
        newVirtualCam.tag = VIRTUAL_CAMERA_TAG;
        Debug.Log("CameraManager: Câmera Virtual da cena anterior removida e uma nova foi criada.");
    }
}