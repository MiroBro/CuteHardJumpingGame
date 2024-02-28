using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleAnimBools : MonoBehaviour
{

    public Animator playerAnim;

    public void ResetAnimatorJumpBools()
    {
        playerAnim.SetBool("Charge", false);
        playerAnim.SetBool("Jump", false);
        playerAnim.SetBool("Land", false);
    }

    public void RandomIdleVariant()
    {
        float ran = Random.Range(0, 10);
        if (ran == 2 || ran == 3)
        {
            playerAnim.SetTrigger("IdleVariant1");
        } else if (ran == 7 || ran == 8 || ran == 9)
        {
            playerAnim.SetTrigger("IdleVariant2");
        }
    }

}
