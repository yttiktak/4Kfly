using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MotionControl : NetworkBehaviour {
    Gyroscope gyr;
    void Start() {
       // if (Input.gyro.enabled) 
        gyr = Input.gyro;
        Vector3 grav = gyr.gravity;
        if (grav.magnitude == 0)
        {
            gyr.enabled = false;
        }
        else
        {
            gyr.enabled = true;
        }
    }

    void Update() {
        if (!isLocalPlayer) { return; }

        if (gyr.enabled)
        {
            Quaternion gq = gyr.attitude;
            transform.rotation = gq;
        }
        else
        {

            var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;
            var y = Input.GetAxis("Pitch") * Time.deltaTime * 150.0f;
            var r = Input.GetAxis("Roll") * Time.deltaTime * 150.0f;

            transform.Rotate(y, x, r);
            transform.Translate(0, 0, -z);
        }
    }

}
