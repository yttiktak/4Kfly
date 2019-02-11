using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiviewScript : MonoBehaviour
{
    public Vector3[] cameraPositions;
    public int nPos = 0;
    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {

    }
}
