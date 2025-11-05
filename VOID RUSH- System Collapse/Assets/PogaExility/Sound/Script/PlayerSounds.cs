using UnityEngine;

// Este script serve como uma biblioteca de sons centralizada para o jogador.
// Ele não faz nada sozinho, apenas guarda as referências para os outros scripts usarem.
public class PlayerSounds : MonoBehaviour
{
    [Header("Sons de Movimento")]
    [Tooltip("O som que toca quando o jogador pula.")]
    public AudioClip jumpSound;

    [Tooltip("O som que toca quando o jogador aterrissa no chão.")]
    public AudioClip landSound;

    [Tooltip("O som que toca ao executar um dash.")]
    public AudioClip dashSound;

    [Header("Sons de Passos")]
    [Tooltip("Uma lista de sons de passos. Um será escolhido aleatoriamente a cada passo.")]
    public AudioClip[] footstepSounds;


    /// <summary>
    /// Retorna um clipe de áudio de passo aleatório da lista.
    /// </summary>
    /// <returns>Um AudioClip de passo, ou nulo se a lista estiver vazia.</returns>
    public AudioClip GetRandomFootstep()
    {
        // Se não houver sons de passos configurados, não faz nada.
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            return null;
        }

        // Escolhe um índice aleatório da lista e retorna o som correspondente.
        int index = Random.Range(0, footstepSounds.Length);
        return footstepSounds[index];
    }
}