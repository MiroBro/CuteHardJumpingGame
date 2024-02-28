using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempResetPlonk : MonoBehaviour
{
    public Transform plonkTrans;

    public void ResetPosToBeginning()
    {
        plonkTrans.position = new Vector3(0, 0, 0);
    }
}
