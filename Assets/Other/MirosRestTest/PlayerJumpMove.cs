using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpMove : MonoBehaviour
{
    public float walkSpeed;
    public float jumpDirectionalSpeed;
    private float moveInput;
    private bool previousIsGrounded;
    public bool isGrounded;
    private Rigidbody2D rb;
    public LayerMask groundMask;

    public PhysicsMaterial2D bounceMat, normalMat;

    public bool canJump = true;
    public float jumpValue = 0.0f;
    public Vector2 nearRadius;

    public float maxJumpValue = 23;
    public float increaseJumpSpeed = 0.6f;

    private bool jumpNow = false;    
    public bool shouldBounce = false;
    public float bounceSpeed = 3f;
    public float bounceUpMultiplier = 3;
    public float bounceDownMultiplier = 2;

    private Vector2 reflectionBounce = new Vector2(0,0);

    public int amountOfBounces = 0;

    public AudioSource jumpAS;
    public AudioSource bounceAS;
    public AudioSource landAS;

    public Animator playerAnim;

    public Transform playerTrans;
    private float lastInput;
    private bool hasHitRoof;

    private float inputBeforeJump;

    //private bool hasJumped;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        HandleWalkingAnim();
        
        previousIsGrounded = isGrounded;
        
        if (hasHitRoof)
        {
            isGrounded = false;
        } else
        {
            isGrounded = Physics2D.OverlapCapsule(transform.position, nearRadius, CapsuleDirection2D.Horizontal, 0f, groundMask);
        }
        // TEST THIS TOMORROW: collhitRoof = Physics2D.OverlapCapsule(transform.position, nearRadius, CapsuleDirection2D.Horizontal, 0f, groundMask);

        //var anim = playerAnim.GetCurrentAnimatorClipInfo(0);
        //string animName = anim.clip.name;
        if (isGrounded && playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Temp_Player_JumpBoing"))
        {
            playerAnim.SetBool("Charge", false);
            playerAnim.SetBool("Jump", false);
            playerAnim.SetBool("Land", true);
        }

        PlayLandingSoundIfProper();
           
        //If pressing jump, increase jump-value
        if (Input.GetKey("space") && isGrounded && canJump)
        {
            playerAnim.SetBool("Jump", false);
            playerAnim.SetBool("Land", false);
            playerAnim.SetBool("Charge", true);
            jumpValue += increaseJumpSpeed;
        }

        //If release jump button, activate the jump
        if (Input.GetKeyUp("space"))
        {
            if (isGrounded)
            {
                //Set minimum value for a jump
                //if (jumpValue < 5)
                //{
                //    jumpValue = 5;
                //}
                jumpAS.Play();
                jumpNow = true;
            }
            inputBeforeJump = moveInput;
            playerAnim.SetBool("Jump",true);
            playerAnim.SetBool("Charge",false);

            canJump = true;
        }
    }

    private void FixedUpdate()
    {
        if (shouldBounce && !isGrounded)
        {
            BouncePlayer();

        } else 
        {
            shouldBounce = false;
            amountOfBounces = 0;

            //You can move only if you aren't in air/aren't jumping
            if (jumpValue == 0.0f && isGrounded)
            {
                rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);
            } else
            {

            }

            if (jumpValue >= maxJumpValue && isGrounded)
            {
                //hasJumped = true;
                playerAnim.SetBool("Jump", true);
                playerAnim.SetBool("Charge", false);
                playerAnim.SetBool("Land", false);
                jumpAS.Play();
                float tempx = moveInput * jumpDirectionalSpeed;
                float tempy = jumpValue;
                rb.velocity = new Vector2(tempx, tempy);
                //lastInput = move
                inputBeforeJump = moveInput;
                Invoke("ResetJump", 0.2f);
            }

            if (Input.GetKeyDown("space") && isGrounded && canJump)
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
            }

            if (jumpNow)
            {
                rb.velocity = new Vector2(moveInput * jumpDirectionalSpeed, jumpValue);
                jumpNow = false;
                jumpValue = 0f;
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        Vector3 N = collision.contacts[0].normal;
        //Debug.Log(N);

        //Direction
        Vector3 V = rb.velocity.normalized;
        //Debug.Log("V: " + V);

        // Reflection
        Vector3 R = Vector3.Reflect(V, N).normalized;
        //Debug.Log("R: " + R);

        if (N.y < 0)
        {
            hasHitRoof = true;
        } else
        {
            hasHitRoof = false;
        }

        if (!isGrounded)
        {            
            amountOfBounces++;
            shouldBounce = true;

            // Assign normalized reflection with the constant speed
            //rb.velocity = new Vector2(-R.x, R.y) * bounceSpeed;
            //reflectionBounce = new Vector2(-R.x, R.y) * bounceSpeed;
            if (N.y < 0)
            {
            //Debug.Log("dOiNG THIS!! downwards push!1");
            //reflectionBounce = new Vector2(V.x * bounceUpMultiplier, -bounceSpeed);
                reflectionBounce = new Vector2(inputBeforeJump * bounceUpMultiplier, 0);
                //Debug.Log(inputBeforeJump);
            }
            else if (R.y > 0)
                reflectionBounce = new Vector2(N.x, R.y* bounceUpMultiplier/amountOfBounces) * bounceSpeed;
            else
                reflectionBounce = new Vector2(N.x, R.y* bounceDownMultiplier/amountOfBounces) * bounceSpeed;
        } else if (N.y < 0)
        {
               // shouldBounce = true;
                //Debug.Log("dOiNG THIS!! downwards push!2, reflectios: " + reflectionBounce);
               // reflectionBounce = new Vector2(lastInput * bounceUpMultiplier, 0);
              //  Debug.Log(lastInput);
        }
    }

    private void BouncePlayer()
    {
        //Debug.Log("Reflectionbounce: " + reflectionBounce);
        bounceAS.Play();
        rb.velocity = reflectionBounce;
        //reflectionBounce = new Vector2(0, 0);
        shouldBounce = false;
    }

    private void ResetJump()
    {
        canJump = false;
        jumpValue = 0;
    }

    private void HandleWalkingAnim()
    {
        ToggleWalkingAnimation();
        MakePlayerFaceRightDirection();
    }

    private void PlayLandingSoundIfProper()
    {
        if (isGrounded && (previousIsGrounded != isGrounded) && !hasHitRoof)
        {
            landAS.Play();
            //hasJumped = false;
            //playerAnim.SetTrigger("Land");
        }
    }

    private void ToggleWalkingAnimation()
    {
        //Få walk/idle animationerna spela när spelaren står still eller rör sig        
        if (moveInput != 0 && isGrounded)
        {
            playerAnim.SetBool("Walk", true);
        }
        else
        {
            playerAnim.SetBool("Walk", false);
        }
    }

    private void MakePlayerFaceRightDirection()
    {
        //Kolla last input så att rätt vi kan vända rätt på karaktären
        if (moveInput != 0)
        {
            if(isGrounded)
              lastInput = moveInput;
        }

        //Vänd karaktären rätt according till vilket sista hållen man rörde sig
        if (lastInput < 0)
        {
            playerTrans.localScale = new Vector3(-Mathf.Abs(playerTrans.localScale.x), playerTrans.localScale.y, playerTrans.localScale.y);
        }
        else
        {
            playerTrans.localScale = new Vector3(Mathf.Abs(playerTrans.localScale.x), playerTrans.localScale.y, playerTrans.localScale.y);
        }
    }
}


//if (jumpValue >= maxJumpValue && isGrounded)
//{
//    jumpValue = maxJumpValue;
//}

//isGrounded = Physics2D.OverlapBox(transform.position, nearRadius, 0f, groundMask);

//If currently jumping equip bounce-physics
/*if (jumpValue > 0 && !isGrounded)
{
    rb.sharedMaterial = bounceMat;
}
else
{
    rb.sharedMaterial = normalMat;
}*/

/*
 public class Ball : MonoBehaviour
{
// Constant speed of the ball
private float speed = 5f;

// Keep track of the direction in which the ball is moving
private Vector2 velocity;

// used for velocity calculation
private Vector2 lastPos;

void Start ()
{
    // Random direction
    rigidbody2D.velocity = Random.insideUnitCircle.normalized * speed;
}

void FixedUpdate ()
{
    // Get pos 2d of the ball.
    Vector3 pos3D = transform.position;
    Vector2 pos2D = new Vector2(pos3D.x, pos3D.y);

    // Velocity calculation. Will be used for the bounce
    velocity = pos2D - lastPos;
    lastPos = pos2D;
}

private void OnCollisionEnter2D(Collision2D col)
{
    // Normal
    Vector3 N = col.contacts[0].normal;

    //Direction
    Vector3 V = velocity.normalized;

    // Reflection
    Vector3 R = Vector3.Reflect(V, N).normalized;

    // Assign normalized reflection with the constant speed
    rigidbody2D.velocity = new Vector2(R.x, R.y) * speed;
}
}
     */
