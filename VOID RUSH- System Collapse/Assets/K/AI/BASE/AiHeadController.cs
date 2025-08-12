using UnityEngine;

// Vers�o F�sica do Head Controller. Aplica torque para olhar para um alvo.
// Deve ser anexado ao GameObject da Cabe�a, que tem seu pr�prio Rigidbody2D.
[RequireComponent(typeof(Rigidbody2D))]
public class AIHeadController : MonoBehaviour
{
    [Header("F�sica da Cabe�a")]
    [Tooltip("A for�a do torque usada para girar a cabe�a em dire��o ao alvo.")]
    public float lookTorque = 50f;

    [Tooltip("Amortecimento da rota��o para evitar que a cabe�a gire para sempre e para estabiliz�-la.")]
    public float rotationDamping = 2f;

    private Rigidbody2D rb;

    void Start()
    {
        // Pega a refer�ncia do Rigidbody2D da pr�pria cabe�a.
        rb = GetComponent<Rigidbody2D>();
    }

    // --- Fun��es P�blicas de Comando (Chamadas pelo AIRagdollController) ---

    /// <summary>
    /// Aplica um torque cont�nuo para fazer a cabe�a olhar na dire��o de um ponto no espa�o.
    /// Deve ser chamada dentro do Update ou FixedUpdate do c�rebro principal.
    /// </summary>
    /// <param name="targetPosition">O ponto no mundo para onde a cabe�a deve tentar olhar.</param>
    public void LookAt(Vector3 targetPosition)
    {
        // Pega a dire��o atual para a qual a cabe�a est� olhando ("frente" do sprite)
        Vector2 currentDirection = transform.right;

        // Pega a dire��o desejada, do centro da cabe�a at� o alvo
        Vector2 targetDirection = (targetPosition - transform.position).normalized;

        // Calcula o "erro" de rota��o usando o produto vetorial (Cross Product).
        // O resultado em Z nos diz se precisamos girar no sentido hor�rio (valor negativo) ou anti-hor�rio (valor positivo).
        float rotationError = Vector3.Cross(currentDirection, targetDirection).z;

        // Aplica o torque com base nesse erro. Quanto maior o erro, maior a for�a para corrigir.
        rb.AddTorque(rotationError * lookTorque * Time.fixedDeltaTime); // Usamos fixedDeltaTime para consist�ncia com a f�sica

        // Aplica um amortecimento para estabilizar o movimento e evitar oscila��o infinita.
        // Isso funciona como um freio rotacional.
        rb.angularVelocity *= (1.0f - Time.fixedDeltaTime * rotationDamping);
    }

    /// <summary>
    /// Aplica um torque para fazer a cabe�a voltar a ficar "reta" em rela��o ao corpo.
    /// </summary>
    /// <param name="bodyTransform">O Transform do corpo principal (geralmente o Torso).</param>
    public void ResetLookDirection(Transform bodyTransform)
    {
        // Se a refer�ncia do corpo n�o existir, n�o faz nada.
        if (bodyTransform == null) return;

        // A dire��o alvo � a mesma "frente" do corpo.
        // Chamamos LookAt usando um ponto imagin�rio na frente do corpo.
        LookAt(transform.position + bodyTransform.right);
    }
}