using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SceneButton : MonoBehaviour
{
    [Header("Configuração da Cena")]
    [Tooltip("Digite EXATAMENTE o nome da cena que este botão deve carregar.")]
    public string sceneNameToLoad;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TravelToScene);
    }

    private void TravelToScene()
    {
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError("O nome da cena não foi definido neste botão!");
            return;
        }

        // --- LÓGICA CORRIGIDA ---
        if (RespawnManager.Instance != null)
        {
            // Encontra a posição atual do jogador
            Transform playerTransform = FindAnyObjectByType<PlayerController>().transform;

            // Chama a nova função para definir o HUB como o ponto de retorno
            RespawnManager.Instance.SetReturnPoint(playerTransform.position, SceneManager.GetActiveScene().name);
        }

        // Carrega a cena da missão
        SceneManager.LoadScene(sceneNameToLoad);
    }
}