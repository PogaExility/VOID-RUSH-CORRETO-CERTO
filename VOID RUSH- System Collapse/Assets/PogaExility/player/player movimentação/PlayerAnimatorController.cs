
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private int isRunningHash;
    private int isJumpingHash;
    private int isFallingHash;
    private int isWallSlidingHash;
    private int isDashingHash;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        isRunningHash = Animator.StringToHash("andando");
        isJumpingHash = Animator.StringToHash("pulando");
        isFallingHash = Animator.StringToHash("pousando");
        isWallSlidingHash = Animator.StringToHash("derrapagem");
        isDashingHash = Animator.StringToHash("isDashing");
    }

    private bool currentRunning;
    private bool currentJumping;
    private bool currentFalling;
    private bool currentWallSliding;
    private bool currentDashing;

    public void UpdateAnimator(bool running, bool jumping, bool falling, bool wallSliding, bool dashing)
    {
        // Corrigir prioridade para evitar ativar andando durante derrapagem
        if (dashing)
        {
            SetAnimatorState(isDashingHash, ref currentDashing, true);
            SetAnimatorState(isWallSlidingHash, ref currentWallSliding, false);
            SetAnimatorState(isJumpingHash, ref currentJumping, false);
            SetAnimatorState(isFallingHash, ref currentFalling, false);
            SetAnimatorState(isRunningHash, ref currentRunning, false);
        }
        else if (wallSliding)
        {
            SetAnimatorState(isWallSlidingHash, ref currentWallSliding, true);
            SetAnimatorState(isDashingHash, ref currentDashing, false);
            SetAnimatorState(isJumpingHash, ref currentJumping, false);
            SetAnimatorState(isFallingHash, ref currentFalling, false);
            SetAnimatorState(isRunningHash, ref currentRunning, false);
        }
        else if (jumping)
        {
            SetAnimatorState(isJumpingHash, ref currentJumping, true);
            SetAnimatorState(isDashingHash, ref currentDashing, false);
            SetAnimatorState(isWallSlidingHash, ref currentWallSliding, false);
            SetAnimatorState(isFallingHash, ref currentFalling, false);
            SetAnimatorState(isRunningHash, ref currentRunning, false);
        }
        else if (falling)
        {
            SetAnimatorState(isFallingHash, ref currentFalling, true);
            SetAnimatorState(isDashingHash, ref currentDashing, false);
            SetAnimatorState(isWallSlidingHash, ref currentWallSliding, false);
            SetAnimatorState(isJumpingHash, ref currentJumping, false);
            SetAnimatorState(isRunningHash, ref currentRunning, false);
        }
        else if (running)
        {
            SetAnimatorState(isRunningHash, ref currentRunning, true);
            SetAnimatorState(isDashingHash, ref currentDashing, false);
            SetAnimatorState(isWallSlidingHash, ref currentWallSliding, false);
            SetAnimatorState(isJumpingHash, ref currentJumping, false);
            SetAnimatorState(isFallingHash, ref currentFalling, false);
        }
        else
        {
            // Nenhum movimento, todos falsos
            SetAnimatorState(isRunningHash, ref currentRunning, false);
            SetAnimatorState(isDashingHash, ref currentDashing, false);
            SetAnimatorState(isWallSlidingHash, ref currentWallSliding, false);
            SetAnimatorState(isJumpingHash, ref currentJumping, false);
            SetAnimatorState(isFallingHash, ref currentFalling, false);
        }
    }

    private void SetAnimatorState(int hash, ref bool currentState, bool newState)
    {
        if (currentState != newState)
        {
            animator.SetBool(hash, newState);
            currentState = newState;
        }
    }
}
