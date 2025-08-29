using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // A refer�ncia ao alvo que a c�mera deve seguir.
    // � 'private' porque apenas o CameraManager deve ter permiss�o para mud�-lo.
    private Transform target;

    [Header("Configura��es de Movimento")]
    [Tooltip("Qu�o suavemente a c�mera segue o jogador. Valores menores = mais suave.")]
    public float smoothSpeed = 0.125f;

    [Tooltip("O deslocamento da c�mera em rela��o ao jogador (eixo Z � importante para a profundidade).")]
    public Vector3 offset = new Vector3(0, 0, -10);

    // LateUpdate � chamado depois de todos os Updates.
    // � o melhor lugar para a l�gica da c�mera, pois garante que o jogador j� se moveu.
    void LateUpdate()
    {
        // Se, por algum motivo, n�o tivermos um alvo, a c�mera n�o se move.
        if (target == null) return;

        // Calcula a posi��o para onde a c�mera quer ir.
        Vector3 desiredPosition = target.position + offset;

        // Move a c�mera suavemente da sua posi��o atual para a posi��o desejada.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime); // Usamos Time.deltaTime para ser independente de framerate

        // Aplica a nova posi��o.
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// ESTA � A FUN��O QUE FALTAVA.
    /// � uma fun��o p�blica que o CameraManager usa para dar a ordem: "Siga este alvo".
    /// </summary>
    /// <param name="newTarget">O Transform do novo alvo (o jogador).</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}