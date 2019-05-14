using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiviewScript : MonoBehaviour
{
    private Vector3 cameraSetback = new Vector3(0,0,0);
    // probably oughta make these private, with getter-setters. Or not. Speediest?
    [HideInInspector]
    public Vector3[] cameraPositions;
    [HideInInspector]
    public int nPos = 0;
    public Camera viewerPositionCamera; // srp multiview uses this camera to set global culling. Place it to view all other camearas views
    public RenderTexture auxCamTex;

    // kinda pointless though. Just move the scene wrt camera. So z slider is changed to something else right now.
    public Vector3 CameraSetback
    {
        get { return cameraSetback; }
        set { cameraSetback = value;}
    }
    public void ChangeFlyCamZ( float news ) {
        cameraSetback.z = news;
    }
    public void ChangeFlyCamX( float news ) { // setback is also used to change camera position for different mosaic taking lens positions
        cameraSetback.x = news;
    }

}
