using UnityEngine;

// Versão Física do Head Controller. Aplica torque para olhar para um alvo.
// Deve ser anexado ao GameObject da Cabeça, que tem seu próprio Rigidbody2D.
[RequireComponent(typeof(Rigidbody2D))]
public class AIHeadController : MonoBehaviour
{
    [Header("Física da Cabeça")]
    [Tooltip("A força do torque usada para girar a cabeça em direção ao alvo.")]
    public float lookTorque = 50f;

    [Tooltip("Amortecimento da rotação para evitar que a cabeça gire para sempre e para estabilizá-la.")]
    public float rotationDamping = 2f;

    private Rigidbody2D rb;

    void Start()
    {
        // Pega a referência do Rigidbody2D da própria cabeça.
        rb = GetComponent<Rigidbody2D>();
    }

    // --- Funções Públicas de Comando (Chamadas pelo AIRagdollController) ---

    /// <summary>
    /// Aplica um torque contínuo para fazer a cabeça olhar na direção de um ponto no espaço.
    /// Deve ser chamada dentro do Update ou FixedUpdate do cérebro principal.
    /// </summary>
    /// <param name="targetPosition">O ponto no mundo para onde a cabeça deve tentar olhar.</param>
    public void LookAt(Vector3 targetPosition)
    {
        // Pega a direção atual para a qual a cabeça está olhando ("frente" do sprite)
        Vector2 currentDirection = transform.right;

        // Pega a direção desejada, do centro da cabeça até o alvo
        Vector2 targetDirection = (targetPosition - transform.position).normalized;

        // Calcula o "erro" de rotação usando o produto vetorial (Cross Product).
        // O resultado em Z nos diz se precisamos girar no sentido horário (valor negativo) ou anti-horário (valor positivo).
        float rotationError = Vector3.Cross(currentDirection, targetDirection).z;

        // Aplica o torque com base nesse erro. Quanto maior o erro, maior a força para corrigir.
        rb.AddTorque(rotationError * lookTorque * Time.fixedDeltaTime); // Usamos fixedDeltaTime para consistência com a física

        // Aplica um amortecimento para estabilizar o movimento e evitar oscilação infinita.
        // Isso funciona como um freio rotacional.
        rb.angularVelocity *= (1.0f - Time.fixedDeltaTime * rotationDamping);
    }

    /// <summary>
    /// Aplica um torque para fazer a cabeça voltar a ficar "reta" em relação ao corpo.
    /// </summary>
    /// <param name="bodyTransform">O Transform do corpo principal (geralmente o Torso).</param>
    public void ResetLookDirection(Transform bodyTransform)
    {
        // Se a referência do corpo não existir, não faz nada.
        if (bodyTransform == null) return;

        // A direção alvo é a mesma "frente" do corpo.
        // Chamamos LookAt usando um ponto imaginário na frente do corpo.
        LookAt(transform.position + bodyTransform.right);
    }
}