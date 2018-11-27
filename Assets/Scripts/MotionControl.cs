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
        //Vector3 grav = gyr.gravity;
      if (SystemInfo.supportsGyroscope)
        {
            gyr.enabled = true;
        }
        else
        {
            gyr.enabled = false;
        }
    }

    public Vector2 startPos;
    public Vector2 direction;
    public bool directionChosen;

    void Update() {
        if (!isLocalPlayer) { return; }

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
                        direction = (touch.position - startPos) * 0.01f;
                        break;
                    case TouchPhase.Ended:
                        directionChosen = true;
                        break;
                }
            }
            if (directionChosen)
            {
                transform.Translate(0, 0, - direction.y);
            }
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
