using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // A referência ao alvo que a câmera deve seguir.
    // É 'private' porque apenas o CameraManager deve ter permissão para mudá-lo.
    private Transform target;

    [Header("Configurações de Movimento")]
    [Tooltip("Quão suavemente a câmera segue o jogador. Valores menores = mais suave.")]
    public float smoothSpeed = 0.125f;

    [Tooltip("O deslocamento da câmera em relação ao jogador (eixo Z é importante para a profundidade).")]
    public Vector3 offset = new Vector3(0, 0, -10);

    // LateUpdate é chamado depois de todos os Updates.
    // É o melhor lugar para a lógica da câmera, pois garante que o jogador já se moveu.
    void LateUpdate()
    {
        // Se, por algum motivo, não tivermos um alvo, a câmera não se move.
        if (target == null) return;

        // Calcula a posição para onde a câmera quer ir.
        Vector3 desiredPosition = target.position + offset;

        // Move a câmera suavemente da sua posição atual para a posição desejada.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime); // Usamos Time.deltaTime para ser independente de framerate

        // Aplica a nova posição.
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// ESTA É A FUNÇÃO QUE FALTAVA.
    /// É uma função pública que o CameraManager usa para dar a ordem: "Siga este alvo".
    /// </summary>
    /// <param name="newTarget">O Transform do novo alvo (o jogador).</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}