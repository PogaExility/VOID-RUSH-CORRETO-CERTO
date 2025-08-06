lsing UnityEngine;

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
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public float coyoteTime = 0.1f;
    
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
    private bool hasDoubleJump = true;
    private bool hasUsedDoubleJump;
    
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
        CheckGround();
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
        moveInput = Input.GetAxisRaw("Horizontal");
        
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
            if (isGrounded || coyoteTimeCounter > 0)
            {
                Jump();
            }
            else if (!hasUsedDoubleJump && hasDoubleJump)
            {
                DoubleJump();
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

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // Usar colisão com TerrainTypeSO para detectar chão
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        foreach (var hit in hits)
        {
            Terrain terrain = hit.GetComponent<Terrain>();
            if (terrain != null && terrain.terrainType != null)
            {
                isGrounded = true;
                groundFriction = terrain.terrainType.friction;

                // Atualizar layer do chão para colisão correta
                groundLayer = terrain.terrainType.layer;
                break;
            }
        }

        // Corrigir pulo infinito: só resetar hasUsedDoubleJump se realmente tocou chão
        if (isGrounded && !wasGrounded)
        {
            hasUsedDoubleJump = false;
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDash = true;
            isWallSliding = false;

            // Executar animação de pousando
            if (!wasGrounded)
            {
                if (playerAnimatorController != null)
                {
                    playerAnimatorController.UpdateAnimator(false, false, true, false, false);
                }
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

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
        rb.velocity = new Vector2(0, -wallSlideSpeed);
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
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
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
}
