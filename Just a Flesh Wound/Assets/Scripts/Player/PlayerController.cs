using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ledgeCheck;
    //Reference to "Platforms" to know what should be detected as ground
    public LayerMask whatIsGround;
   
    private bool isFacingRight = true;
    private bool isRunning;
    public bool isGrounded;
    private bool isTouchingWall;
    public bool isWallSliding;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isAttemptingToJump;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;
    private bool isTouchingLedge;
    private bool canClimLedge = false;
    private bool ledgeDetected;
    private bool isDashing;

    private int amoutJumpsLeft;
    private int facingDirection = 1;
    public int amountOfJump = 1;
    private int lastWallJumpDirection;
    
    //Private float
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private float movementInputDirection;
    private float dashTimeLeft;
    private float lastImageXpos;
    private float lastDash = -100f;

    //Public float
    public float jumpForce = 16.0f;
    public float groundCheckRadius = 1.0f;
    public float movementSpeed = 8.0f;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce;
    public float wallJumpForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f ;
    public float wallJumpTimerSet = 0.5f;
    public float dashTime;
    public float dashSpeed;
    public float distanceBetweenImages;
    public float dashCoolDown;

    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;
    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        amoutJumpsLeft = amountOfJump;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
        CheckLedgeClimb();
        CheckDash();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurrounding();
    }

    private void ApplyMovement()
    {

        //if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        //{
            if( !isGrounded && !isWallSliding && movementInputDirection == 0)
            {
                rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
            }
            else if( canMove )
            {
                rb.velocity = new Vector2( movementSpeed * movementInputDirection, rb.velocity.y );
            }
        //}

/*        if( !isGrounded && !isWallSliding && movementInputDirection != 0 )
        {
            Vector2 forceToAdd = new Vector2( movementForceInAir * movementInputDirection, 0 );
            rb.AddForce( forceToAdd );

            if( Mathf.Abs( rb.velocity.x ) > movementSpeed )
            {
                rb.velocity = new Vector2( movementSpeed * movementInputDirection, rb.velocity.y );
            }
        }*/

        if ( isWallSliding )
        {
            if( rb.velocity.y < -wallSlideSpeed )
            {
                rb.velocity = new Vector2( rb.velocity.x, -wallSlideSpeed );
            }
        }
    }
    #region JUMP


    private void NormalJump()
    {
        if (canNormalJump && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amoutJumpsLeft--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if (canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amoutJumpsLeft = amountOfJump;
            amoutJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }
#endregion
    #region CHECK FUNCTIONS
    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");
        
        if ( Input.GetButtonDown("Jump") )
        {
            if( isGrounded || ( amoutJumpsLeft > 0 && !isTouchingWall ) )
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if( Input.GetButtonDown("Horizontal") && isTouchingWall )
        {
            if ( !isGrounded && movementInputDirection != facingDirection )
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if ( turnTimer >= 0 )
        {
            turnTimer -= Time.deltaTime;

            if( turnTimer <= 0 )
            {
                canMove = true;
                canFlip = true;
            }
        }
        
        if ( checkJumpMultiplier && !Input.GetButton("Jump") )
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2( rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier );
        }

        //Check for dash
        if( Input.GetButtonUp("Dash"))
        {
            if( Time.time > (lastDash + dashCoolDown) )
            AttemptToDash();
        }

    }

    private void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;


    }
    //Check if we can dash or not + manage dash velocity 
    private void CheckDash()
    {
        if (isDashing)
        {
            if(dashTimeLeft > 0)
            {
                canMove = false;
                canFlip = false;
                //to make the player fall use rb.velocity.y as 2nd argument. If not, use "0"
                rb.velocity = new Vector2(dashSpeed * facingDirection, 0);
                dashTimeLeft -= Time.deltaTime;

                if (Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                }
            }

            if(dashTimeLeft <= 0 || isTouchingWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
        }
    }

    private void CheckJump()
    {
        if (jumpTimer > 0)
        {
            //wallJump
            if (!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection)
            {
                WallJump();
            }
            else if ( isGrounded )
            {
                NormalJump();
            }

        }

        if( isAttemptingToJump )
        {
            jumpTimer -= Time.deltaTime;
        }

        if ( wallJumpTimer > 0 )
        {
            if( hasWallJumped && movementInputDirection == -lastWallJumpDirection )
            {
                rb.velocity = new Vector2( rb.velocity.x, 0.0f );
                hasWallJumped = false;
            }
            else if( wallJumpTimer <= 0  )
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }
        /*else if ( (isWallSliding && movementInputDirection == 0 &&  canJump) )// to wall hop
                {
                    isWallSliding = false;
                    amoutJumpsLeft--;
                    Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y); 
                    rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }*/
    }
    private void CheckSurrounding()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);
        if( isTouchingWall && !isTouchingLedge)
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }

    private void CheckLedgeClimb()
    {
        if( ledgeDetected && !canClimLedge)
        {
            canClimLedge = true;

            if (isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;

            animator.SetBool("canClimbLedge", canClimLedge);
        }

        if (canClimLedge)
        {
            transform.position = ledgePos1;
        }
    }

    //Check if the Ledge climb animation is finished
    public void FinishLedgeClimb()
    {
        canClimLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        animator.SetBool("canClimbLedge", canClimLedge);

    }

    private void CheckIfWallSliding()
    {
        if ( isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0 && !canClimLedge)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if (!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if( Mathf.Abs(rb.velocity.x) >= 0.01f )
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }
    private void CheckIfCanJump()
    {
        if( isGrounded && rb.velocity.y <= 0.01f )
        {
            amoutJumpsLeft = amountOfJump;
        }

        if (isTouchingWall)
        {
            canWallJump = true;
        }

        if(amoutJumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }
#endregion
    private void Flip()
    {
        if (!isWallSliding && canFlip)
        {

            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);

        }

    }

    private void UpdateAnimations()
    {
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isWallSliding", isWallSliding);
    }

    public void DisableFlip()
    {
        canFlip = false;
    }

    public void EnableFlip()
    {
        canFlip = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3( wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z ));

        Gizmos.DrawLine(ledgePos1, ledgePos2);
    }
}
