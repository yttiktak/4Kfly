using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeZ : MonoBehaviour
{

    public void SetZ( float value ) {
        // float wasz = transform.position.z;
        Vector3 wasP = transform.position;
        wasP.z = value;
        transform.position = wasP;
    }
}
