// ProjectileAnimator.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ProjectileAnimator : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        // Pega a referência do Animator que está no mesmo objeto.
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Função pública que outros scripts (como GunpowderExplosion) vão chamar.
    /// </summary>
    /// <param name="animationName">O nome exato do estado da animação a ser tocado.</param>
    public void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
        else
        {
            Debug.LogError("Animator não encontrado neste objeto!", this.gameObject);
        }
    }
}