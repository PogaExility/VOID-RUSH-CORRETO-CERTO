using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O alvo que a câmera deve seguir. Será encontrado automaticamente pela tag 'Player'.")]
    private Transform target;

    [Header("Configurações de Movimento")]
    [Tooltip("Quão suavemente a câmera segue o jogador. Valores menores = mais suave.")]
    public float smoothSpeed = 0.125f;

    [Tooltip("O deslocamento da câmera em relação ao jogador (eixo Z é importante para a profundidade).")]
    public Vector3 offset = new Vector3(0, 0, -10);

    void Start()
    {
        // Encontra o jogador na cena assim que o jogo começa
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogError("CameraFollow: Não foi possível encontrar um objeto com a tag 'Player'!");
        }
    }

    // Usamos LateUpdate para o movimento da câmera.
    // Isso garante que a câmera só se mova DEPOIS que o jogador já se moveu naquele frame,
    // evitando qualquer tipo de "tremida" ou "lag" visual.
    void LateUpdate()
    {
        // Se não tivermos um alvo, não faz nada.
        if (target == null) return;

        // Posição desejada = posição do alvo + deslocamento
        Vector3 desiredPosition = target.position + offset;

        // Interpola suavemente (Lerp) da posição atual da câmera para a posição desejada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Aplica a nova posição à câmera
        transform.position = smoothedPosition;
    }
}