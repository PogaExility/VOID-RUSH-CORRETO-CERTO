using UnityEngine;

public class MissionBoard : MonoBehaviour
{
    [Header("Refer�ncias de UI")]
    [Tooltip("Arraste o objeto do Panel que cont�m os cartazes das quests.")]
    public GameObject missionPanel;

    private bool isPanelOpen = false;

    void Start()
    {
        // Garante que o painel comece fechado
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }
    }

    // A fun��o de intera��o agora abre/fecha o painel de miss�es
    public void Interact()
    {
        isPanelOpen = !isPanelOpen;
        missionPanel.SetActive(isPanelOpen);

        // Opcional: Pausar o jogo enquanto o quadro est� aberto
        // Time.timeScale = isPanelOpen ? 0f : 1f;
    }
}