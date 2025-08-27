using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    // Esta � a fun��o que o PlayerController vai chamar quando o jogador apertar "E".
    public void Interact()
    {
        Debug.Log("INTERAGIU COM O QUEST GIVER! Tentando iniciar a quest..."); // <-- Adicione esta linha
        // Se houver um QuestManager na cena, inicia a quest.
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest();
        }

        // Desativa o objeto para que n�o possa ser usado novamente.
        gameObject.SetActive(false);
    }
}