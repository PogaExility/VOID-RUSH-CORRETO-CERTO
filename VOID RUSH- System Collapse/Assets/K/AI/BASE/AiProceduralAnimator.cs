using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Precisamos disso para a busca mais inteligente

// Este script controla o ragdoll fazendo-o seguir uma "marionete" invisível (TargetSkeleton).
public class AIProceduralAnimator : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O GameObject raiz do esqueleto físico (ragdoll). Ex: FeralBody")]
    public Transform physicalSkeletonRoot;
    [Tooltip("O GameObject raiz do esqueleto alvo (marionete). Ex: TargetSkeleton")]
    public Transform targetSkeletonRoot;

    [Header("Parâmetros Físicos")]
    [Tooltip("Força geral para mover os membros em direção à pose alvo.")]
    public float poseStrength = 2500f;
    [Tooltip("Força para corrigir a rotação dos membros.")]
    public float rotationStrength = 600f;

    private Rigidbody2D[] physicalRigidbodies;
    private Transform[] targetTransforms;
    private Vector2 moveIntention = Vector2.zero;

    void Start()
    {
        // Pega todos os RBs do corpo físico
        physicalRigidbodies = physicalSkeletonRoot.GetComponentsInChildren<Rigidbody2D>();

        // Pega todos os Transforms da marionete (exceto a própria raiz)
        List<Transform> allTargetChildren = targetSkeletonRoot.GetComponentsInChildren<Transform>().ToList();
        allTargetChildren.Remove(targetSkeletonRoot); // Remove a raiz da lista
        targetTransforms = new Transform[physicalRigidbodies.Length];

        // Popula as listas garantindo que os nomes correspondam
        for (int i = 0; i < physicalRigidbodies.Length; i++)
        {
            Rigidbody2D rb = physicalRigidbodies[i];
            // Encontra o alvo correspondente ignorando "(1)", "(Clone)", etc.
            string cleanName = rb.name.Split('(')[0].Trim();

            Transform correspondingTarget = allTargetChildren.FirstOrDefault(t => t.name.Split('(')[0].Trim() == cleanName);

            if (correspondingTarget != null)
            {
                targetTransforms[i] = correspondingTarget;
            }
            else
            {
                Debug.LogWarning("Não foi possível encontrar o alvo correspondente para: " + rb.name, this);
            }
        }

        if (physicalRigidbodies.Length != targetTransforms.Length)
        {
            Debug.LogError("ERRO: O número de partes físicas e partes alvo não é o mesmo! Verifique a hierarquia e os nomes.", this);
        }
    }

    void FixedUpdate()
    {
        if (physicalRigidbodies.Length != targetTransforms.Length) return;

        // Move a marionete inteira com a intenção de movimento
        targetSkeletonRoot.position += (Vector3)moveIntention * Time.fixedDeltaTime;

        // Aplica forças para fazer o corpo físico seguir a marionete
        for (int i = 0; i < physicalRigidbodies.Length; i++)
        {
            Rigidbody2D rb = physicalRigidbodies[i];
            Transform target = targetTransforms[i];

            if (rb == null || target == null) continue;

            // Força de Posição
            Vector2 forceDirection = ((Vector2)target.position - rb.position);
            rb.AddForce(forceDirection * poseStrength * Time.fixedDeltaTime);

            // Força de Rotação
            float angleDiff = Mathf.DeltaAngle(rb.rotation, target.eulerAngles.z);
            rb.AddTorque(angleDiff * rotationStrength * Time.fixedDeltaTime);
        }
    }

    public void SetMoveIntention(Vector2 intention)
    {
        moveIntention = intention;
    }
}