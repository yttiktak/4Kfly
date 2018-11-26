using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MotionControl : NetworkBehaviour {
    void Update() {
        if (!isLocalPlayer) { return; }

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;
        var y = Input.GetAxis("Pitch") * Time.deltaTime * 150.0f;
        var r = Input.GetAxis("Roll") * Time.deltaTime * 150.0f;

        transform.Rotate(y, x, r);
        transform.Translate(0, 0, -z);
    }

}
