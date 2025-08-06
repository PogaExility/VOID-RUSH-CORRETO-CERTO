

using TMPro;
using UnityEngine;

public class AdvancedPlayerMovement2D : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    
    [Header("Pulo")]
    public float jumpForce = 12f;
    public float maxJumpTime = 0.3f;
    public float jumpCutMultiplier = 0.5f;
    
    [Header("Verificação de Chão")]
    public LayerMask groundLayer;
    public float coyoteTime = 0.1f;

    [Header("UI")]
    public TextMeshProUGUI groundStatusText;
    
    [Header("Mouse")]
    public bool useMouseDirection = true;
    
    [Header("Wall Sliding")]
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 8f;
    public float wallJumpUpForce = 6f;
    
    [Header("Dash")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    
    [Header("Controles")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.Q;
    
    // Componentes
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isJumping;
    private bool isWallSliding;
    private bool isDashing;
    private bool canDash = true;
    
    private int jumpCount = 1; // 1 = pode pular, 0 = não pode pular
    private float jumpTimeCounter;
    private float coyoteTimeCounter;
    private float dashTimeCounter;
    private float dashCooldownCounter;
    private float moveInput;
    private Vector2 mouseDirection;
    
    private bool isTouchingWallLeft;
    private bool isTouchingWallRight;
    private bool isHoldingLeft;
    private bool isHoldingRight;
    private bool hasUsedDoubleJump = false;
    private bool hasDoubleJump = true;
    
    private PlayerAnimatorController playerAnimatorController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAnimatorController = GetComponent<PlayerAnimatorController>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        HandleInput();
        CheckWall();
        HandleDash();

        // Atualizar animações
        if (playerAnimatorController != null)
        {
            bool running = Mathf.Abs(moveInput) > 0.1f && !isWallSliding && !isDashing && isGrounded;
            bool falling = rb.linearVelocity.y < -0.1f && !isGrounded && !isWallSliding;
            bool jumping = isJumping && !isWallSliding;
            bool wallSlidingAnim = isWallSliding;
            playerAnimatorController.UpdateAnimator(running, jumping, falling, wallSlidingAnim, isDashing);
        }
    }
    
    void FixedUpdate()
    {
        if (!isDashing)
        {
            if (isWallSliding)
            {
                HandleWallSlide();
            }
            else
            {
                Move();
            }
        }
    }
    
    void HandleInput()
    {
        // Se estiver derrapando, bloquear movimentação horizontal
        if (isWallSliding)
        {
            moveInput = 0f;
        }
        else
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        
        // Flip horizontal do player baseado na posição do mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        
        // Check wall input
        isHoldingLeft = Input.GetKey(KeyCode.A);
        isHoldingRight = Input.GetKey(KeyCode.D);
        
        // Dash detection
        if (Input.GetKeyDown(dashKey) && canDash && !isDashing)
        {
            StartDash();
        }
        
        // Jump
        if (Input.GetKeyDown(jumpKey))
        {
            if (jumpCount > 0)
            {
                Jump();
                jumpCount--;
            }
            else if (!hasUsedDoubleJump && hasDoubleJump)
            {
                DoubleJump();
                hasUsedDoubleJump = true;
            }
            else if (isWallSliding)
            {
                if ((isTouchingWallLeft && isHoldingLeft) || (isTouchingWallRight && isHoldingRight))
                {
                    WallJump();
                }
            }
        }
        
        if (Input.GetKeyUp(jumpKey) && isJumping)
        {
            EndJump();
        }
    }
    
    private float groundFriction = 1f;

    // Removido método CheckGround pois groundCheck será feito via colisão com collider

    void HandleWallSlide()
    {
        // Ativar animação de derrapagem e garantir que o parâmetro seja atualizado no PlayerAnimatorController
        if (playerAnimatorController != null)
        {
            playerAnimatorController.UpdateAnimator(false, false, false, true, false);
        }
        else if (animator != null)
        {
            animator.SetBool("derrapagem", true);
        }

        // Cancelar todo movimento horizontal e impedir movimentação lateral
        rb.linearVelocity = new Vector2(0, -wallSlideSpeed);
    }
    
    void CheckWall()
    {
        isTouchingWallLeft = Physics2D.Raycast(wallCheckLeft.position, Vector2.left, wallCheckDistance, wallLayer);
        isTouchingWallRight = Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallCheckDistance, wallLayer);
        
        bool shouldWallSlide = false;
        
        if (isTouchingWallLeft && !isGrounded && rb.linearVelocity.y < 0)
        {
            if (isHoldingLeft)
            {
                shouldWallSlide = true;
            }
        }
        else if (isTouchingWallRight && !isGrounded && rb.linearVelocity.y < 0)
        {
            if (isHoldingRight)
            {
                shouldWallSlide = true;
            }
        }
        
        isWallSliding = shouldWallSlide;
    }
    
    void Move()
    {
        if (!isWallSliding)
        {
            if (animator != null)
            {
                animator.SetBool("derrapagem", false);
            }
            
            float targetSpeed = moveInput * moveSpeed;
            float speedDiff = targetSpeed - rb.linearVelocity.x;
            
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            
            float movement = speedDiff * accelRate;
            
            rb.AddForce(movement * Vector2.right);
        }
    }
    
    void Jump()
    {
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    
    void DoubleJump()
    {
        hasUsedDoubleJump = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    
    void WallJump()
    {
        isWallSliding = false;
        
        Vector2 wallJumpDirection = Vector2.zero;
        
        if (isTouchingWallLeft)
        {
            wallJumpDirection = new Vector2(1, 1);
        }
        else if (isTouchingWallRight)
        {
            wallJumpDirection = new Vector2(-1, 1);
        }
        
        rb.linearVelocity = new Vector2(wallJumpDirection.x * wallJumpForce, wallJumpUpForce);
    }
    
    void EndJump()
    {
        isJumping = false;
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }
    
    void StartDash()
    {
        Vector2 dashDirection = new Vector2(moveInput, 0).normalized;
        if (dashDirection == Vector2.zero)
            dashDirection = new Vector2(transform.localScale.x, 0);
        
        isDashing = true;
        canDash = false;
        dashTimeCounter = dashDuration;
        dashCooldownCounter = dashCooldown;
        
        rb.linearVelocity = dashDirection * dashForce;
    }
    
    void HandleDash()
    {
        if (isDashing)
        {
            dashTimeCounter -= Time.deltaTime;
            if (dashTimeCounter <= 0)
            {
                isDashing = false;
            }
        }
        
        if (!canDash)
        {
            dashCooldownCounter -= Time.deltaTime;
            if (dashCooldownCounter <= 0)
            {
                canDash = true;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (wallCheckLeft != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheckLeft.position, wallCheckLeft.position + Vector3.left * wallCheckDistance);
        }
        
        if (wallCheckRight != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheckRight.position, wallCheckRight.position + Vector3.right * wallCheckDistance);
        }
    }

    // Métodos de colisão para detecção de chão
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            jumpCount = 1; // Resetar contador de pulos quando tocar o chão
            coyoteTimeCounter = coyoteTime;
            
            // Resetar double jump
            hasUsedDoubleJump = false;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
            coyoteTimeCounter = coyoteTime;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            coyoteTimeCounter = coyoteTime;
        }
    }
}
