using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SceneButton : MonoBehaviour
{
    [Header("Configura��o da Cena")]
    [Tooltip("Digite EXATAMENTE o nome da cena que este bot�o deve carregar.")]
    public string sceneNameToLoad;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TravelToScene);
    }

    private void TravelToScene()
    {
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError("O nome da cena n�o foi definido neste bot�o!");
            return;
        }

        // --- L�GICA CORRIGIDA ---
        if (RespawnManager.Instance != null)
        {
            // Encontra a posi��o atual do jogador
            Transform playerTransform = FindAnyObjectByType<PlayerController>().transform;

            // Chama a nova fun��o para definir o HUB como o ponto de retorno
            RespawnManager.Instance.SetReturnPoint(playerTransform.position, SceneManager.GetActiveScene().name);
        }

        // Carrega a cena da miss�o
        SceneManager.LoadScene(sceneNameToLoad);
    }
}