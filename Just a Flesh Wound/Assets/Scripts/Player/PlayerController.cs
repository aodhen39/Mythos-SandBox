using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    public Transform groundCheck;
    public Transform wallCheck;
    //Reference to "Platforms" to know what should be detected as ground
    public LayerMask whatIsGround;
   
    private float movementInputDirection;
    private bool isFacingRight = true;
    private bool isRunning;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canJump;

    private int amoutJumpsLeft;
    private int facingDirection = 1;
    public int amountOfJump = 1;

    public float jumpForce = 16.0f;
    public float groundCheckRadius = 1.0f;
    public float movementSpeed = 10.0f;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce;
    public float wallJumpForce;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;
    
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

    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurrounding();
    }



    private void ApplyMovement()
    {
        rb.velocity = new Vector2( movementSpeed * movementInputDirection, rb.velocity.y );

        if( !isGrounded && !isWallSliding && movementInputDirection != 0 )
        {
            Vector2 forceToAdd = new Vector2( movementForceInAir * movementInputDirection, 0 );
            rb.AddForce( forceToAdd );

            if( Mathf.Abs( rb.velocity.x ) > movementSpeed )
            {
                rb.velocity = new Vector2( movementSpeed * movementInputDirection, rb.velocity.y );
            }
        }
        else if(!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }

        if (isWallSliding)
        {
            if(rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    private void Jump()
    {

        if (canJump && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amoutJumpsLeft--;
        }
        else if ( (isWallSliding && movementInputDirection == 0 &&  canJump) )// to wall hop
        {
            isWallSliding = false;
            amoutJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y); 
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        else if ( ( isWallSliding || isTouchingWall ) && movementInputDirection !=0 && canJump )
        {
            isWallSliding = false;
            amoutJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
    }
    #region Check Function
    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (Input.GetButtonUp("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }

    }

    private void CheckSurrounding()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && !isGrounded && rb.velocity.y < 0)
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

        if(rb.velocity.x > 0.01f || rb.velocity.x < -0.01f)
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
        if( (isGrounded && rb.velocity.y <= 0.01f) || isWallSliding)
        {
            amoutJumpsLeft = amountOfJump;
        }

        if(amoutJumpsLeft <= 0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
    }
#endregion
    private void Flip()
    {
        if (!isWallSliding)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3( wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z ));
    }
}
