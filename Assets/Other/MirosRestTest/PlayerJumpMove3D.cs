using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpMove3D : MonoBehaviour
{
    public float walkSpeed;
    public float jumpDirectionalSpeed;
    private float moveInput;
    public bool isGrounded;
    private Rigidbody rb;
    public LayerMask groundMask;

    public PhysicMaterial bounceMat, normalMat;

    public bool canJump = true;
    public float jumpValue = 0.0f;
    public Vector3 nearRadius;

    public float maxJumpValue = 23;
    public float increaseJumpSpeed = 0.6f;

    private bool jumpNow = false;

    public float radius;
    public Collider playerColl;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();    
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");



        var colls = Physics.OverlapCapsule(transform.position, nearRadius, radius, groundMask);
        isGrounded = colls.Length != 0;//Physics.OverlapCapsule(transform.position, nearRadius, radius, groundMask);
        //isGrounded = Physics.OverlapCapsule(transform.position, nearRadius, CapsuleDirection.Horizontal, 0f, groundMask);
        //isGrounded = Physics2D.OverlapBox(transform.position, nearRadius, 0f, groundMask);

        //If currently jumping equip bounce-physics
        if (jumpValue > 0 && !isGrounded)
        {
            playerColl.sharedMaterial = bounceMat;
        }
        else
        {
            playerColl.sharedMaterial = normalMat;
        }

        if (Input.GetKeyDown("space") && isGrounded && canJump)
        {
            jumpValue += increaseJumpSpeed;
        }

        if (jumpValue >= maxJumpValue && isGrounded)
        {
            jumpValue = maxJumpValue;
        }

        if (Input.GetKeyUp("space"))
        {
            if (isGrounded)
            {
                jumpNow = true;
            }
            canJump = true;
        }
    }

    private void FixedUpdate()
    {
        //You can move only if you aren't in air/aren't jumping
        if (jumpValue == 0.0f && isGrounded)
        {
            rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);
        }

        if (jumpValue >= maxJumpValue && isGrounded)
        {
            float tempx = moveInput * jumpDirectionalSpeed;
            float tempy = jumpValue;
            rb.velocity = new Vector2(tempx, tempy);

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collided!");
    }

    private void ResetJump()
    {
        canJump = false;
        jumpValue = 0;
    }
}
