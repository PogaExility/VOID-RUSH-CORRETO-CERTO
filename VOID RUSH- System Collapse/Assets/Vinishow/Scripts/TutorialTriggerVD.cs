using UnityEngine;

public class TutorialTriggerVD : MonoBehaviour
{
    [Tooltip("Arraste para cá o objeto do Canvas de Tutorial que deve ser ativado.")]
    [SerializeField] private TutorialCanvasVD tutorialCanvas;

    void Awake()
    {
        // Verificação de segurança para garantir que o tutorial foi configurado.
        if (tutorialCanvas == null)
        {
            Debug.LogError("O gatilho de tutorial não tem uma referência para o TutorialCanvasVD!", this);
            this.enabled = false;
        }
    }

    // Esta função é chamada pela Unity quando um outro colisor 2D entra na área do gatilho.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se quem entrou foi o jogador.
        if (other.CompareTag("Player"))
        {
            // Chama a função para iniciar o tutorial no script do Canvas.
            tutorialCanvas.IniciarTutorial();

            // Desativa este objeto de gatilho para que ele não seja usado novamente.
            gameObject.SetActive(false);
        }
    }
}