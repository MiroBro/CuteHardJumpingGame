using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 This class holds references to objects in scene that
 we might want to access from multiple scripts.

 I made this singleton for easy access, and so each class
 doesnt have to have have public attributes dragged into them!
 (Have seen someone do this online so I stole this idea!)
 */
public class ReferenceHolder : MonoBehaviour
{
    //Below, code to make this a singleton
        private static ReferenceHolder _instance;

        public static ReferenceHolder Instance { get { return _instance; } }


        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
    //Singleton-code ends here

    //Temp sounds for Plonk's jumping
    public AudioSource jumpAS;
    public AudioSource bounceAS;
    public AudioSource landAS;

    //Player objects
    public Rigidbody2D playerRb;
    public Animator playerAnim;
    public Transform playerTrans;
}
