using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlonksMovement : MonoBehaviour
{
    //-PUBLIC ATTRIBUTES-
    //---------------------------------------------------------------------------------------------
    /*
     * Attributes that determine the movement speeds (both jump and walk)
     */
    public float walkSpeed; //Walking speed
    public float jumpDirectionalSpeed; //Jump-speed in x-direction
    public float increaseJumpSpeed = 0.6f; //How fast a space-btn will charge jump.
    public float jumpValue = 0.0f; //How far the jump-value has charged
    public float maxJumpValue = 23; //Max value jump can charge until automatic jump
    public bool isGrounded; //Used throughout the script to see if ok to jump etc. Public for debugging-purposes

    /*
     * Attributes that determine bounce-amount
     * These I've set via trial and error via the editor (according to what felt good?)
     * Determine how much bounce, and how much bounce redices and increases
     */
    public float bounceSpeed = 3f;
    public float bounceUpMultiplier = 3;
    public float bounceDownMultiplier = 2;

    /*
    * These attributes relate to where collider on Plonks is (needed to for example check if standing on ground)
    * Also hold bounce/non-bounce material
    */
    public PhysicsMaterial2D bounceMat, normalMat; //When Plonks is in air, bounce-mat (bounce, no friction) is "equipped" otherwise normalMat (with friction, no bounce)
    public LayerMask groundMask; //This is set to ground in editor, meaning Plonks will only interact with colliders that have a tag called "ground"
    public Transform collCenter; //I use this as a reference-point for "isGrounded near-radius because it needs to know the middle-point of the collider I THINK?
    public Vector2 nearRadius; //This describes the collider under Plonk's feet. Used to check if she's grounded (took from tutorial, undertand poorly!)

    //-PRIVATE ATTRIBUTES-
    //---------------------------------------------------------------------------------------------
    /*
     * Player objects that are needed in the script
     * Are set via Referenceholder (see Start() below)
     */
    private Rigidbody2D rb;
    private Animator playerAnim;
    private Transform playerTrans;


    /*
     * Numbers to keep track of playerInput and bounce
     */
    private float moveInput; //Directional Input from player
    private float lastInput; //If player no longer presses button, this attr. keeps track to keep Plonks turned in the direction last pressed.
    private float inputBeforeJump; //Used to keep track of direction to side-bounce Plonks. If inputBeforeJump was -1, then reflection will be +1 and vice versa.
    private int amountOfBounces = 0;//Use this to calculate bounce-strength, and depending on nbr of bounces the bounces will get weaker.
    private Vector2 reflectionBounce = new Vector2(0, 0); //Use this to hold which value Plonks should bounce by.

    /*
     * Bools to keep track on movement status and changes
     */
    private bool canJump = true; //if true will allow player to chargejump
    private bool jumpNow = false; //will be set to true if Plonks can jump, so fixedJump can handle it
    private bool isMidJump; //This is used to see if Plonks is midair and "Mid-jump"-animation should play
    private bool hasHitRoof; //If hasHitRoof = true, plonks should do another "bounce" in comparison with "side bounces"
    private bool shouldBounce = false; //if true will 'bounce' Plonks.
    private bool previousIsGrounded; //If PrevGrounded = false, but isGrounded = true now, that means Plonks has JUST landed and landing-animation should be played
    //---------------------------------------------------------------------------------------------

    private void Start()
    {
        rb = ReferenceHolder.Instance.playerRb;
        playerAnim = ReferenceHolder.Instance.playerAnim;
        playerTrans = ReferenceHolder.Instance.playerTrans;
    }

    /*
    * According to internet, player variable's should change/be updated in UPDATE-function, 
    * but actions to player movement should be done in FIXED update (see below)
    * I ignored that advice and put everything here : D think it works SOMEWHAT ANYWAY?
    */
    void Update()
    {
        //TODO: DO THIS, make it make more sense
        //If on ground handle walking OR loading jump
        //Else If in air handle only than

        


        if (IsFalling()) //If is falling, make Plonks fall!
        {
            playerAnim.SetBool("isFalling", true);

            //Dont know how this function works really, but using it (via tutorial) to see if Plonks is on ground. 
            //I set the "nearRadius" is trial and error via editor hahah
            isGrounded = Physics2D.OverlapCapsule(collCenter.position, nearRadius, CapsuleDirection2D.Horizontal, 0f, groundMask);

            //Because if Plonks is on the ground again, she has finished bouncing around
            if (isGrounded)
                amountOfBounces = 0;
        }
        else //If not, make her walk/jump/charge depending on onput from player
        {

            playerAnim.SetBool("isFalling", false);
            moveInput = Input.GetAxisRaw("Horizontal");
            MakePlayerFaceRightDirection();
            //Update grounded-status so we can compare it later and see if it's changed. If previousIsGrounded != isGrounded either landed/jumped/bounced 
            previousIsGrounded = isGrounded;

            PlayLandingSoundIfProper();


            //Få walk/idle animationerna spela när spelaren står still eller rör sig        
            if (moveInput != 0 && isGrounded && !Input.GetKey(KeyCode.Space))
            {
                playerAnim.SetBool("Walk", true);
            }
            else
            {
                playerAnim.SetBool("Walk", false);
            }


            if (hasHitRoof)
            {
                isGrounded = false;
            }
            else
            {
                UpgradeGroundedStatus();
            }

            if (isGrounded && playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Plonks_MidJump"))
            {
                playerAnim.SetBool("Charge", false);
                playerAnim.SetBool("Jump", false);
                playerAnim.SetBool("Land", true);
                isMidJump = false;
            }
            //If pressing jump, increase jump-value
            if (Input.GetKey("space") && isGrounded && canJump)
            {
                //turn on charge animation
                playerAnim.SetBool("Jump", false);
                playerAnim.SetBool("Land", false);
                playerAnim.SetBool("Charge", true);
                jumpValue += increaseJumpSpeed;
            } 
            else if (Input.GetKeyUp("space") && isGrounded)
            {
                if (isGrounded)
                {
                    ReferenceHolder.Instance.jumpAS.Play();
                    jumpNow = true;
                }
                inputBeforeJump = moveInput;
                playerAnim.SetBool("Jump", true);
                isMidJump = true;
                playerAnim.SetBool("Charge", false);

                canJump = true;
            }
        }

        if (shouldBounce && !isGrounded)
        {
            BouncePlayer();

        }
        else
        {
            shouldBounce = false;

            //You can move only if you aren't in air/aren't jumping
            if (jumpValue == 0.0f && isGrounded)// && !Input.GetKey(KeyCode.Space))
            {
                rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);
            }

            //if charged jump to maximum dp maximum jump
            if (jumpValue >= maxJumpValue && isGrounded)
            {
                DoMaximumJump();
            }

            //If charging make Plonk stand still
            if (Input.GetKeyDown("space") && isGrounded && canJump)
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
            }

            //If player pressed jump-button before reaching maximum, jump now!
            if (jumpNow)
            {
                rb.velocity = new Vector2(moveInput * jumpDirectionalSpeed, jumpValue);
                jumpNow = false;
                jumpValue = 0f;
            }
        }
    }

    /*
     * Used to check if Plonks has collided with her head/body of wall or roof
     * I stole much of this code and dont know too well why it works/if it works :D
     */
    private void OnCollisionEnter2D(Collision2D collision)
    {

        /*This code was stolen, dont know really how it works!!*/
        Vector3 N = collision.contacts[0].normal;

        //Direction
        Vector3 V = rb.velocity.normalized;

        // Reflection
        Vector3 R = Vector3.Reflect(V, N).normalized;

        if (N.y < 0)
        {
            hasHitRoof = true;
        }
        else
        {
            hasHitRoof = false;
        }
        /*End of stolen code!*/

        if (!isGrounded)
        {
            amountOfBounces++;
            shouldBounce = true;

            if (N.y < 0)
            {
                reflectionBounce = new Vector2(inputBeforeJump * bounceUpMultiplier, 0);
            }
            else if (R.y > 0)
                reflectionBounce = new Vector2(N.x, R.y * bounceUpMultiplier / (amountOfBounces * amountOfBounces)) * bounceSpeed;
            else
                reflectionBounce = new Vector2(N.x, R.y * bounceDownMultiplier / (amountOfBounces * amountOfBounces)) * bounceSpeed;
        }
    }


    private void DoMaximumJump()
    {
        playerAnim.SetBool("Jump", true);
        isMidJump = true;
        playerAnim.SetBool("Charge", false);
        playerAnim.SetBool("Land", false);
        ReferenceHolder.Instance.jumpAS.Play();
        float tempx = moveInput * jumpDirectionalSpeed;
        float tempy = jumpValue;
        rb.velocity = new Vector2(tempx, tempy);
        inputBeforeJump = moveInput;
        Invoke("ResetJump", 0.2f);
    }

    private void UpgradeGroundedStatus()
    {
        //Dont know how this function works really, but using it (via tutorial) to see if Plonks is on ground. 
        //I set the "nearRadius" is trial and error via editor hahah
        isGrounded = Physics2D.OverlapCapsule(collCenter.position, nearRadius, CapsuleDirection2D.Horizontal, 0f, groundMask);

        ResetBounceCount();
    }

    private void ResetBounceCount()
    {
        //Because if Plonks is on the ground again, she has finished bouncing around
        if (isGrounded)
            amountOfBounces = 0;
    }

    private bool IsFalling()
    {
        return !isMidJump && !isGrounded;
    }


    private void BouncePlayer()
    {
        ReferenceHolder.Instance.bounceAS.Play();
        if (hasHitRoof)
        {
            playerAnim.SetTrigger("RoofBounce");
        }
        else
        {
            playerAnim.SetTrigger("Bounce");
        }
        rb.velocity = reflectionBounce;
        shouldBounce = false;
    }

    //Is called from ANIMATOR!!!! says (0 reference) but it is used and plonks will start bouncing
    //if you remove this lol! xD
    private void ResetJump()
    {
        canJump = false;
        jumpValue = 0;
    }



    private void PlayLandingSoundIfProper()
    {
        if (isGrounded && (previousIsGrounded != isGrounded) && !hasHitRoof)
        {
            ReferenceHolder.Instance.landAS.Play();
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
            if (isGrounded)
                lastInput = moveInput;
        }

        //Vänd karaktären rätt according till vilket sista hållen man rörde sig
        if (lastInput < 0)
        {
            playerTrans.localScale = new Vector3(Mathf.Abs(playerTrans.localScale.x), playerTrans.localScale.y, playerTrans.localScale.y);
        }
        else
        {
            playerTrans.localScale = new Vector3(-Mathf.Abs(playerTrans.localScale.x), playerTrans.localScale.y, playerTrans.localScale.y);
        }
    }
}

