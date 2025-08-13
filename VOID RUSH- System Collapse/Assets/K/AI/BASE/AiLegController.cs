using UnityEngine;

// Este script controla as pernas do Ragdoll para fazê-lo andar.
public class AILegController : MonoBehaviour
{
    [Header("Referências das Pernas")]
    public Rigidbody2D torsoRb; // O corpo principal
    public Rigidbody2D rightUpperLeg;
    public Rigidbody2D leftUpperLeg;

    [Header("Parâmetros da Andada")]
    public float moveForce = 200f; // A força aplicada para dar um passo
    public float stepFrequency = 1.0f; // Passos por segundo
    private float stepTimer;

    private bool rightLegStep = true; // Qual perna dará o próximo passo

    void FixedUpdate()
    {
        // Mantém o torso de pé aplicando uma força anti-gravidade e torque anti-rotação
        if (torsoRb != null)
        {
            torsoRb.AddForce(Vector2.up * torsoRb.mass * -Physics2D.gravity.y);
            torsoRb.AddTorque(-torsoRb.angularVelocity * 2f); // Amortecimento de rotação
        }
    }

    // --- Funções de Comando ---

    public void MoveInDirection(float direction)
    {
        stepTimer += Time.fixedDeltaTime;
        if (stepTimer > 1f / stepFrequency)
        {
            stepTimer = 0;
            if (rightLegStep)
            {
                // Dá um passo com a perna direita
                rightUpperLeg.AddForce(new Vector2(direction * moveForce, moveForce * 0.5f));
            }
            else
            {
                // Dá um passo com a perna esquerda
                leftUpperLeg.AddForce(new Vector2(direction * moveForce, moveForce * 0.5f));
            }
            rightLegStep = !rightLegStep; // Alterna a perna
        }
    }

    public void StopMoving()
    {
        // Lógica para parar de andar
    }

    public Transform GetBodyTransform()
    {
        return torsoRb.transform;
    }
}