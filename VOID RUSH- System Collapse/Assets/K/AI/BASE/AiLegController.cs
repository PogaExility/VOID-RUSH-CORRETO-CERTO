using UnityEngine;

// Este script controla as pernas do Ragdoll para faz�-lo andar.
public class AILegController : MonoBehaviour
{
    [Header("Refer�ncias das Pernas")]
    public Rigidbody2D torsoRb; // O corpo principal
    public Rigidbody2D rightUpperLeg;
    public Rigidbody2D leftUpperLeg;

    [Header("Par�metros da Andada")]
    public float moveForce = 200f; // A for�a aplicada para dar um passo
    public float stepFrequency = 1.0f; // Passos por segundo
    private float stepTimer;

    private bool rightLegStep = true; // Qual perna dar� o pr�ximo passo

    void FixedUpdate()
    {
        // Mant�m o torso de p� aplicando uma for�a anti-gravidade e torque anti-rota��o
        if (torsoRb != null)
        {
            torsoRb.AddForce(Vector2.up * torsoRb.mass * -Physics2D.gravity.y);
            torsoRb.AddTorque(-torsoRb.angularVelocity * 2f); // Amortecimento de rota��o
        }
    }

    // --- Fun��es de Comando ---

    public void MoveInDirection(float direction)
    {
        stepTimer += Time.fixedDeltaTime;
        if (stepTimer > 1f / stepFrequency)
        {
            stepTimer = 0;
            if (rightLegStep)
            {
                // D� um passo com a perna direita
                rightUpperLeg.AddForce(new Vector2(direction * moveForce, moveForce * 0.5f));
            }
            else
            {
                // D� um passo com a perna esquerda
                leftUpperLeg.AddForce(new Vector2(direction * moveForce, moveForce * 0.5f));
            }
            rightLegStep = !rightLegStep; // Alterna a perna
        }
    }

    public void StopMoving()
    {
        // L�gica para parar de andar
    }

    public Transform GetBodyTransform()
    {
        return torsoRb.transform;
    }
}