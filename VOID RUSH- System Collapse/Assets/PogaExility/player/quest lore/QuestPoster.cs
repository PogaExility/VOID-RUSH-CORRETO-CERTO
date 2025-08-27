using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuestPoster : MonoBehaviour
{
    void Start()
    {
        // Adiciona um "ouvinte" ao bot�o para que, quando clicado,
        // a nossa fun��o AcceptQuest seja chamada.
        GetComponent<Button>().onClick.AddListener(AcceptQuest);
    }

    private void AcceptQuest()
    {
        // 1. Avisa o QuestManager para iniciar a quest
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest();
        }

        // 2. Remove o cartaz do quadro
        gameObject.SetActive(false);

        // 3. Opcional: Fecha o painel de miss�es automaticamente
        // (Encontra o MissionBoard e chama a fun��o Interact para fechar)
        FindAnyObjectByType<MissionBoard>()?.Interact();

        Debug.Log("Quest aceita!");
    }
}