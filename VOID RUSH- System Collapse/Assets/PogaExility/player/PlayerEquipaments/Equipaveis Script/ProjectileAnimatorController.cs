// ProjectileAnimatorController.cs - VERSÃO PROFISSIONAL COM HASHES
using UnityEngine;

public enum ProjectileAnimState
{
    None, // Estado padrão
    polvora,
    Corte1_Anim,
    Corte2_Anim,
    Corte3_Anim
}

[RequireComponent(typeof(Animator))]
public class ProjectileAnimatorController : MonoBehaviour
{
    private Animator animator;

    #region State Hashes
    private static readonly int PolvoraHash = Animator.StringToHash("polvora");
    private static readonly int Corte1Hash = Animator.StringToHash("Corte1_Anim");
    private static readonly int Corte2Hash = Animator.StringToHash("Corte2_Anim");
    private static readonly int Corte3Hash = Animator.StringToHash("Corte3_Anim");
    #endregion

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAnimation(ProjectileAnimState state)
    {
        if (animator == null) return;

        int hash = GetStateHash(state);
        if (hash != 0)
        {
            animator.Play(hash, 0, 0f);
        }
    }

    private int GetStateHash(ProjectileAnimState state)
    {
        switch (state)
        {
            case ProjectileAnimState.polvora: return PolvoraHash;
            case ProjectileAnimState.Corte1_Anim: return Corte1Hash;
            case ProjectileAnimState.Corte2_Anim: return Corte2Hash;
            case ProjectileAnimState.Corte3_Anim: return Corte3Hash;
            default: return 0;
        }
    }
}