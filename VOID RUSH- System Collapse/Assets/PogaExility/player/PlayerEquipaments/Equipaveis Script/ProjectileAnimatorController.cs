// ProjectileAnimator.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ProjectileAnimator : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        // Pega a refer�ncia do Animator que est� no mesmo objeto.
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Fun��o p�blica que outros scripts (como GunpowderExplosion) v�o chamar.
    /// </summary>
    /// <param name="animationName">O nome exato do estado da anima��o a ser tocado.</param>
    public void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
        else
        {
            Debug.LogError("Animator n�o encontrado neste objeto!", this.gameObject);
        }
    }
}