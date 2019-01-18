using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Vibrate : MonoBehaviour
{
    // Start is called before the first frame update
    public float amt = 0.001f;
    public float centeringForce = 0.0001f;
	public bool vibrate = false;
    void Start()
    {
        Debug.Log("hello");
		foreach (Renderer rend in gameObject.GetComponentsInChildren<Renderer>()) {
			rend.receiveShadows = false;
			rend.reflectionProbeUsage = ReflectionProbeUsage.Off;
			rend.shadowCastingMode = ShadowCastingMode.Off;
			rend.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
		}
        foreach (Transform myt in gameObject.GetComponentsInChildren<Transform>())
        {
            if (myt.childCount ==0 )
            {

                Transform original = Instantiate(myt);
                original.SetParent(myt);
                original.localPosition = myt.localPosition;
                original.localRotation = myt.localRotation;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vibes;
        Vector3 nowAt;
		if (vibrate) {
			foreach (Transform myt in gameObject.GetComponentsInChildren<Transform>()) {
				// Debug.Log(myt.name + " has ccnt =" + myt.childCount);
				if (myt.childCount != 0) {
					continue;
				}
				vibes = myt.localPosition;
				nowAt = myt.parent.localPosition;
				// random walk
				nowAt.x += Random.Range (-amt, amt);
				nowAt.y += Random.Range (-amt, amt);
				nowAt.z += Random.Range (-amt, amt);
				nowAt = nowAt * 0.9f + vibes * 0.1f;
				myt.parent.localPosition = nowAt;
			}
		}
    }
}
