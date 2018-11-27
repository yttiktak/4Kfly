using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MotionControl : NetworkBehaviour {

    public bool followCam = false;
    Gyroscope gyr;
    Quaternion originalQ;

    float deltaZ = 0;

    void Start() {
        // if (Input.gyro.enabled) 
        gyr = Input.gyro;
        //Vector3 grav = gyr.gravity;
      if (SystemInfo.supportsGyroscope)
        {
            gyr.enabled = true;
            originalQ = gyr.attitude;
            followCam = true;
        }
        else
        {
            gyr.enabled = false;
            followCam = false;
        }
    }

    public Vector2 startPos;
    public Vector2 direction;
    public bool directionChosen;

    void Update() {
        if (!isLocalPlayer) { return; }

        if (Input.GetKey("1")) {
            followCam = true;
        }
        if (Input.GetKey("2")){
            followCam = false;
        }

        if (gyr.enabled)
        {
            Quaternion gq = gyr.attitude;
            Quaternion grq = gq;
            grq.x = gq.z; // try to flip round y axis
            grq.z = gq.x;
            transform.rotation = grq;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch(touch.phase)
                {
                    case TouchPhase.Began:
                        startPos = touch.position;
                        directionChosen = false;
                        break;
                    case TouchPhase.Moved:
                        direction = (touch.position - startPos);
                        break;
                    case TouchPhase.Ended:
                        directionChosen = true;
                        break;
                }
            }
            if (directionChosen)
            {
                transform.Translate(0, 0, - direction.y);
                direction *= 0.7f;
            }

        }
        else
        {

            var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * 2.0f;
            var y = Input.GetAxis("Pitch") * Time.deltaTime * 150.0f;
            var r = Input.GetAxis("Roll") * Time.deltaTime * 150.0f;

            deltaZ -= z;

            transform.Rotate(y, x, r);
            transform.Translate(0, 0, deltaZ);
            deltaZ *= 0.95f;
        }
        if (followCam)
        {
            Vector3 nowat = transform.position;
            Camera.main.transform.position = nowat;
            Camera.main.transform.rotation = transform.rotation;
            Camera.main.transform.Translate(new Vector3(0, 30, 40), transform);
            Camera.main.transform.LookAt(transform);
        }
    }

}
