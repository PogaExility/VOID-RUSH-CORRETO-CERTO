using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("O alvo que a c�mera deve seguir. Ser� encontrado automaticamente pela tag 'Player'.")]
    private Transform target;

    [Header("Configura��es de Movimento")]
    [Tooltip("Qu�o suavemente a c�mera segue o jogador. Valores menores = mais suave.")]
    public float smoothSpeed = 0.125f;

    [Tooltip("O deslocamento da c�mera em rela��o ao jogador (eixo Z � importante para a profundidade).")]
    public Vector3 offset = new Vector3(0, 0, -10);

    void Start()
    {
        // Encontra o jogador na cena assim que o jogo come�a
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogError("CameraFollow: N�o foi poss�vel encontrar um objeto com a tag 'Player'!");
        }
    }

    // Usamos LateUpdate para o movimento da c�mera.
    // Isso garante que a c�mera s� se mova DEPOIS que o jogador j� se moveu naquele frame,
    // evitando qualquer tipo de "tremida" ou "lag" visual.
    void LateUpdate()
    {
        // Se n�o tivermos um alvo, n�o faz nada.
        if (target == null) return;

        // Posi��o desejada = posi��o do alvo + deslocamento
        Vector3 desiredPosition = target.position + offset;

        // Interpola suavemente (Lerp) da posi��o atual da c�mera para a posi��o desejada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Aplica a nova posi��o � c�mera
        transform.position = smoothedPosition;
    }
}