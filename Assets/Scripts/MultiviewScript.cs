using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiviewScript : MonoBehaviour
{
    public Vector3[] cameraPositions;
    public int nPos = 10;
    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        cameraPositions = new Vector3[nPos];
        for (int i=0; i<nPos; i++)
        {
            cameraPositions[i].x = 0;
            cameraPositions[i].y = 5.0f * i;
            cameraPositions[i].z = 0;
        }
    }
}
