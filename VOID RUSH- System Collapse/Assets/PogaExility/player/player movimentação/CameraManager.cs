using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Configura��o dos Prefabs")]
    [Tooltip("PREFAB da C�mera Principal. DEVE conter o componente CinemachineBrain.")]
    public GameObject mainCameraPrefab;

    [Tooltip("PREFAB da C�mera Virtual. DEVE conter CinemachineVirtualCamera, Confiner2D e o script CinemachineTargetSetter.")]
    public GameObject virtualCameraPrefab;

    // Usaremos esta tag para encontrar e limpar as c�meras virtuais antigas.
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
        // Valida��o para garantir que os prefabs foram atribu�dos no Inspector.
        if (mainCameraPrefab == null || virtualCameraPrefab == null)
        {
            Debug.LogError("CameraManager: Prefabs da Main Camera ou da Virtual Camera n�o foram definidos no Inspector!");
            return;
        }

        // --- L�GICA DA C�MERA PRINCIPAL (com Brain) ---
        // Se n�o existir uma Main Camera, criamos uma e a tornamos persistente.
        if (GameObject.FindGameObjectWithTag("MainCamera") == null)
        {
            GameObject mainCamInstance = Instantiate(mainCameraPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(mainCamInstance);
            Debug.Log("CameraManager: Main Camera com Brain criada e marcada como persistente.");
        }

        // --- L�GICA DA C�MERA VIRTUAL (espec�fica da cena) ---
        // 1. Destr�i a VCam antiga, se houver.
        GameObject oldVirtualCam = GameObject.FindGameObjectWithTag(VIRTUAL_CAMERA_TAG);
        if (oldVirtualCam != null)
        {
            Destroy(oldVirtualCam);
        }

        // 2. Cria a nova VCam para a cena atual.
        GameObject newVirtualCam = Instantiate(virtualCameraPrefab, Vector3.zero, Quaternion.identity);
        // Garante que a nova VCam tenha a tag para que possamos encontr�-la na pr�xima cena.
        newVirtualCam.tag = VIRTUAL_CAMERA_TAG;
        Debug.Log("CameraManager: C�mera Virtual da cena anterior removida e uma nova foi criada.");
    }
}