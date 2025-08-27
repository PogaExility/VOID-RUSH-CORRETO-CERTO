using UnityEngine;

public class MissionBoard : MonoBehaviour
{
    [Header("Referências de UI")]
    [Tooltip("Arraste o objeto do Panel que contém os cartazes das quests.")]
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

    // A função de interação agora abre/fecha o painel de missões
    public void Interact()
    {
        isPanelOpen = !isPanelOpen;
        missionPanel.SetActive(isPanelOpen);

        // Opcional: Pausar o jogo enquanto o quadro está aberto
        // Time.timeScale = isPanelOpen ? 0f : 1f;
    }
}