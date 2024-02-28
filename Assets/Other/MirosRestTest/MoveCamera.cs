using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform playerTrans;

    private void Update()
    {
        transform.position = GetPlayerPos();
    }

    private Vector3 GetPlayerPos()
    {
        int y = (int)Math.Ceiling((playerTrans.position.y - 5) / 10);
        return new Vector3(transform.position.x, y * 10, transform.position.z);
    }

    /*
    public Transform playerTrans;
    public Transform camPos1;
    public Transform camPos2;
    public Transform camPos3;
    public Transform camPos4;
    void Update()
    {
        if (playerTrans.position.y < 4.9f)
        {
            transform.position = new Vector3(camPos1.position.x, camPos1.position.y, transform.position.z);
        }
        else if (playerTrans.position.y >= 4.9f && playerTrans.position.y < 15f )
        {
            transform.position = new Vector3(camPos2.position.x, camPos2.position.y, transform.position.z); 
        }
        else if (playerTrans.position.y >= 15f && playerTrans.position.y < 25f)
        {
            transform.position = new Vector3(camPos3.position.x, camPos3.position.y, transform.position.z);
        }
        else if (playerTrans.position.y >= 25f)
        {
            transform.position = new Vector3(camPos4.position.x, camPos4.position.y, transform.position.z);
        }
    }
    */
}
