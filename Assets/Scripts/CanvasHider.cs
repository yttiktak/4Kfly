using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHider : MonoBehaviour
{

    public void Hide(){
        Canvas me = gameObject.GetComponent<Canvas>();
        me.planeDistance = 500.0f; // tuck it behind everything
    }
}
